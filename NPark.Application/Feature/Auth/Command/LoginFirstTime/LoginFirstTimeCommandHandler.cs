using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.UserSpecification;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.LoginFirstTime
{
    public sealed class LoginFirstTimeCommandHandler : ICommandHandler<LoginFirstTimeCommand>
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<LoginFirstTimeCommandHandler> _logger;

        public LoginFirstTimeCommandHandler(
            IGenericRepository<User> userRepository,
            IPasswordService passwordService,
            IAuditLogger auditLogger,
            ILogger<LoginFirstTimeCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(
            LoginFirstTimeCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Get user by username
                // ---------------------------
                var spec = new GetUserByUserName(request.UserName);
                var user = await _userRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);

                var credentialsValid =
                    user is not null &&
                    _passwordService.Verify(request.Password, user.PasswordHash);

                if (!credentialsValid)
                {
                    _logger.LogWarning(
                        "Invalid first-time login attempt for username: {UserName}",
                        request.UserName);

                    await SafeAuditFirstTimeLoginFailedAsync(user?.Id, request.UserName, cancellationToken);

                    return Result.Fail(
                        new Error(
                            Code: "Auth.InvalidCredentials",
                            Message: ErrorMessage.Auth_InvalidCredentials,
                            Type: ErrorType.Security));
                }

                // ---------------------------
                // 2) Update password
                // ---------------------------
                var newPasswordHash = _passwordService.Hash(request.NewPassword);
                user.UpdatePasswordHash(newPasswordHash);

                await _userRepository.SaveChangesAsync(cancellationToken);

                // ---------------------------
                // 3) Audit success (Safe)
                // ---------------------------
                await SafeAuditPasswordChangedFirstTimeAsync(user.Id, request.UserName, cancellationToken);

                _logger.LogInformation(
                    "User {UserName} changed password on first login at {DateTime}",
                    request.UserName,
                    DateTime.UtcNow);

                return Result.Ok();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "LoginFirstTime operation was canceled for username: {UserName}",
                    request.UserName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while processing first-time login for username: {UserName}",
                    request.UserName);

                return Result.Fail(
                    new Error(
                        Code: "Auth.LoginFirstTime.Unexpected",
                        Message: ErrorMessage.AuthLogin_Unexpected,
                        Type: ErrorType.Infrastructure));
            }
        }

        // --------------------------------------------------------
        // Safe audit: first-time login failed
        // --------------------------------------------------------
        private async Task SafeAuditFirstTimeLoginFailedAsync(
            Guid? userId,
            string userName,
            CancellationToken cancellationToken)
        {
            try
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "LoginFirstTimeFailed",
                        EventCategory: "Auth",
                        IsSuccess: false,
                        StatusCode: StatusCodes.Status401Unauthorized,
                        ErrorCode: "Auth.InvalidCredentials",
                        ErrorMessage: "اسم المستخدم أو كلمة المرور غير صحيحة (Invalid username or password).",
                        UserId: userId,
                        Extra: new
                        {
                            UserName = userName,
                            AttemptAt = DateTime.UtcNow
                        }),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Audit logging failed for LoginFirstTimeFailed. UserName: {UserName}",
                    userName);
            }
        }

        // --------------------------------------------------------
        // Safe audit: password changed first time successfully
        // --------------------------------------------------------
        private async Task SafeAuditPasswordChangedFirstTimeAsync(
            Guid userId,
            string userName,
            CancellationToken cancellationToken)
        {
            try
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "PasswordChangedFirstTime",
                        EventCategory: "Auth",
                        IsSuccess: true,
                        StatusCode: StatusCodes.Status200OK,
                        UserId: userId,
                        Extra: new
                        {
                            UserName = userName,
                            ChangedAt = DateTime.UtcNow
                        }),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Audit logging failed for PasswordChangedFirstTime. UserId: {UserId}, UserName: {UserName}",
                    userId, userName);
            }
        }
    }
}