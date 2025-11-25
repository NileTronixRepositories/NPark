using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.ParkingGateSpec;
using NPark.Application.Specifications.UserSpecification;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.SupervisorLogin
{
    public sealed class SupervisorLoginCommandHandler : ICommandHandler<SupervisorLoginCommand, UserTokenDto>
    {
        private readonly IGenericRepository<Domain.Entities.User> _repository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordService _passwordService;
        private readonly IAuditLogger _auditLogger;
        private readonly IGenericRepository<ParkingGate> _gateInfoRepository;

        public SupervisorLoginCommandHandler(
            IGenericRepository<Domain.Entities.User> repository,
      IJwtProvider jwtProvider, IPasswordService passwordService,
      IAuditLogger auditLogger,
      IGenericRepository<ParkingGate> gateInfoRepository,
      IGenericRepository<ParkingGate> parkingGateRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _gateInfoRepository = gateInfoRepository ?? throw new ArgumentNullException(nameof(gateInfoRepository));
        }

        public async Task<Result<UserTokenDto>> Handle(SupervisorLoginCommand request, CancellationToken cancellationToken)
        {
            var spec = new GetUserByUserName(request.UserName);
            var userEntity = await _repository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
            var isValid = _passwordService.Verify(request.Password, userEntity!.PasswordHash);

            if (!isValid)
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "LoginFailed",
                        EventCategory: "Auth",
                        IsSuccess: false,
                        StatusCode: StatusCodes.Status401Unauthorized,
                        ErrorCode: "Auth.InvalidCredentials",
                        ErrorMessage: "Invalid username or password.",
                        UserId: userEntity.Id,
                        Extra: new
                        {
                            request.UserName
                        }),
                    cancellationToken);

                return Result<UserTokenDto>.Fail(new Error(
                    ErrorMessage.WrongPassword,
                    ErrorMessage.WrongPassword,
                    ErrorType.Security));
            }
            var gateEntity = await _gateInfoRepository.FirstOrDefaultWithSpecAsync(
               new GetGateByGateNumberSpec(request.GateNumber, request.GateType), cancellationToken);

            if (gateEntity == null)
                return Result<UserTokenDto>.Fail(new Error("Invalid Gate Number", "Invalid Gate Number", ErrorType.Security));

            var token = await _jwtProvider.Generate(userEntity, gateEntity);
            var peripheral = new GateDevicePeripheral
            {
                LprIp = gateEntity.LprIp,
                PcIp = gateEntity.PcIp,
                HasLpr = gateEntity.HasLpr,
                HasPc = gateEntity.HasPc,
                GateNumber = gateEntity.GateNumber,
                GateType = gateEntity.GateType
            };
            token.GateDevicePeripheral = peripheral;
            await _auditLogger.LogAsync(
              new AuditLogEntry(
                  EventName: "GateSelected",
                  EventCategory: "Gate",
                  IsSuccess: true,
                  StatusCode: StatusCodes.Status200OK,
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