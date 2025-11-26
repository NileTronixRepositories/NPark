using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.UserSpecification;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.Login
{
    public sealed class LoginCommandHandler
        : ICommandHandler<LoginCommand, UserTokenDto>
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordService _passwordService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IGenericRepository<User> userRepository,
            IJwtProvider jwtProvider,
            IPasswordService passwordService,
            IAuditLogger auditLogger,
            ILogger<LoginCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<UserTokenDto>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Get user by username
                // ---------------------------
                var spec = new GetUserByUserName(request.UserName);
                var userEntity = await _userRepository
                    .FirstOrDefaultWithSpecAsync(spec, cancellationToken);

                var credentialsValid =
                    userEntity is not null &&
                    _passwordService.Verify(request.Password, userEntity.PasswordHash);

                if (!credentialsValid)
                {
                    _logger.LogWarning("Invalid login attempt for username: {UserName}", request.UserName);

                    await SafeAuditLoginFailedAsync(userEntity?.Id, request.UserName, cancellationToken);

                    return Result<UserTokenDto>.Fail(
                        new Error(
                            Code: "Auth.InvalidCredentials",
                            Message: "اسم المستخدم أو كلمة المرور غير صحيحة (Invalid username or password).",
                            Type: ErrorType.Security));
                }

                // ---------------------------
                // 2) Generate JWT token
                // ---------------------------
                var tokenDto = await _jwtProvider.Generate(userEntity);

                // ---------------------------
                // 3) Audit success (Safe)
                // ---------------------------
                await SafeAuditLoginSucceededAsync(userEntity.Id, request.UserName, cancellationToken);

                _logger.LogInformation("User logged in successfully. UserId: {UserId}, UserName: {UserName}",
                    userEntity.Id, request.UserName);

                return Result<UserTokenDto>.Ok(tokenDto);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Login operation was canceled for username: {UserName}", request.UserName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing login for username: {UserName}", request.UserName);

                return Result<UserTokenDto>.Fail(
                    new Error(
                        Code: "Auth.Login.Unexpected",
                        Message: ErrorMessage.AuthLogin_Unexpected,
                        Type: ErrorType.Infrastructure));
            }
        }

        // --------------------------------------------------------
        // Safe audit: LoginFailed
        // --------------------------------------------------------
        private async Task SafeAuditLoginFailedAsync(
            Guid? userId,
            string userName,
            CancellationToken cancellationToken)
        {
            try
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "LoginFailed",
                        EventCategory: "Auth",
                        IsSuccess: false,
                        StatusCode: StatusCodes.Status401Unauthorized,
                        ErrorCode: "Auth.InvalidCredentials",
                        ErrorMessage: "اسم المستخدم أو كلمة المرور غير صحيحة (Invalid username or password).",
                        UserId: userId,
                        Extra: new
                        {
                            UserName = userName
                        }),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Audit logging failed for LoginFailed. UserName: {UserName}", userName);
            }
        }

        // --------------------------------------------------------
        // Safe audit: LoginSucceeded
        // --------------------------------------------------------
        private async Task SafeAuditLoginSucceededAsync(
            Guid userId,
            string userName,
            CancellationToken cancellationToken)
        {
            try
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "LoginSucceeded",
                        EventCategory: "Auth",
                        IsSuccess: true,
                        StatusCode: StatusCodes.Status200OK,
                        UserId: userId,
                        Extra: new
                        {
                            UserName = userName
                        }),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Audit logging failed for LoginSucceeded. UserId: {UserId}, UserName: {UserName}",
                    userId, userName);
            }
        }
    }
}