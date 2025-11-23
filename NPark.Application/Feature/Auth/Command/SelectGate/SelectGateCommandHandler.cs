using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.ParkingGateSpec;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.Auth.Command.SelectGate
{
    public sealed class SelectGateCommandHandler : ICommandHandler<SelectGateCommand, UserTokenDto>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITokenReader _tokenReader;
        private readonly IGenericRepository<ParkingGate> _gateInfoRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IGenericRepository<User> _userInfoRepository;
        private readonly IAuditLogger _auditLogger;

        public SelectGateCommandHandler(IHttpContextAccessor contextAccessor,
            ITokenReader tokenReader,
            IGenericRepository<ParkingGate> gateInfoRepository, IJwtProvider jwtProvider,
            IAuditLogger auditLogger,
            IGenericRepository<User> userInfoRepository)
        {
            _contextAccessor = contextAccessor;
            _tokenReader = tokenReader;
            _gateInfoRepository = gateInfoRepository;
            _jwtProvider = jwtProvider;
            _userInfoRepository = userInfoRepository;
            _auditLogger = auditLogger;
        }

        public async Task<Result<UserTokenDto>> Handle(SelectGateCommand request, CancellationToken cancellationToken)
        {
            string number = request.GateNumber.Split(" ")[0];
            int numberParse;
            if (!int.TryParse(number, out numberParse))
            {
                return Result<UserTokenDto>.Fail(new Error("Invalid Gate Number", "The gate number is not a valid number.", ErrorType.Security));
            }
            var tokenInfo = _contextAccessor.HttpContext?.ReadToken(_tokenReader);
            if (tokenInfo == null)
                return Result<UserTokenDto>.Fail(new Error("Invalid Token", "Invalid Token", ErrorType.Security));

            if (!tokenInfo.UserId.HasValue)
                return Result<UserTokenDto>.Fail(new Error("Invalid Token", "User ID is missing in the token", ErrorType.Security));

            var userId = tokenInfo.UserId.Value;

            // Fetch the user entity using the UserId
            var userEntity = await _userInfoRepository.GetByIdAsync(userId, cancellationToken);

            // Fetch the gate entity based on GateNumber
            var gateEntity = await _gateInfoRepository.FirstOrDefaultWithSpecAsync(
                new GetGateByGateNumberSpec(numberParse, request.GateType), cancellationToken);

            // If the gate is invalid, return error
            if (gateEntity == null)
                return Result<UserTokenDto>.Fail(new Error("Invalid Gate Number", "Invalid Gate Number", ErrorType.Security));

            // Set the gate as occupied (marking the gate as occupied by this user)
            if (gateEntity.IsOccupied != null && gateEntity.IsOccupied.Value)
                return Result<UserTokenDto>.Fail(new Error("Gate is Occupied", "Gate is Occupied", ErrorType.Security));

            gateEntity.SetIsOccupied(true, tokenInfo.UserId, DateTime.Now);
            await _gateInfoRepository.SaveChangesAsync(cancellationToken);

            // Generate JWT token for the user and gate combination
            var token = await _jwtProvider.Generate(userEntity, gateEntity);
            var peripheral = new GateDevicePeripheral
            {
                LprIp = gateEntity.LprIp,
                PcIp = gateEntity.PcIp,
                HasLpr = gateEntity.HasLpr,
                HasPc = gateEntity.HasPc,
            };
            token.GateDevicePeripheral = peripheral;
            await _auditLogger.LogAsync(
              new AuditLogEntry(
                  EventName: "GateSelected",
                  EventCategory: "Gate",
                  IsSuccess: true,
                  StatusCode: StatusCodes.Status200OK,
                  UserId: userId,
                  GateId: gateEntity.Id,
                  Extra: new
                  {
                      gateNumber = gateEntity.GateNumber,     // التأكد من أن القيم واضحة في gateEntity
                      gateType = gateEntity.GateType.ToString() // تحويل الـ GateType في gateEntity إلى String
                  }),
              cancellationToken);
            return Result<UserTokenDto>.Ok(token);
        }
    }
}