using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.UserSpecification;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.Login
{
    internal class LoginCommandHandler : ICommandHandler<LoginCommand, UserTokenDto>
    {
        private readonly IGenericRepository<User> _repository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordService _passwordService;
        private readonly IGenericRepository<ParkingGate> _parkingGateRepository;
        private readonly IAuditLogger _auditLogger;

        public LoginCommandHandler(IGenericRepository<User> repository,
            IJwtProvider jwtProvider, IPasswordService passwordService,
            IAuditLogger auditLogger,
            IGenericRepository<ParkingGate> parkingGateRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _parkingGateRepository = parkingGateRepository ?? throw new ArgumentNullException(nameof(parkingGateRepository));
        }

        public async Task<Result<UserTokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
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

            var response = await _jwtProvider.Generate(userEntity);

            await _auditLogger.LogAsync(
                new AuditLogEntry(
                    EventName: "LoginSucceeded",
                    EventCategory: "Auth",
                    IsSuccess: true,
                    StatusCode: StatusCodes.Status200OK,
                    UserId: userEntity.Id,
                    Extra: new
                    {
                        request.UserName
                    }),
                cancellationToken);

            return Result<UserTokenDto>.Ok(response);
        }
    }
}