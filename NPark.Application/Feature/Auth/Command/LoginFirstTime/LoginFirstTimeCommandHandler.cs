using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.UserSpecification;
using NPark.Domain.Entities;
using NPark.Domain.Resource;
using Serilog;

namespace NPark.Application.Feature.Auth.Command.LoginFirstTime
{
    public class LoginFirstTimeCommandHandler : ICommandHandler<LoginFirstTimeCommand>
    {
        private readonly IGenericRepository<User> _repository;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<LoginFirstTimeCommandHandler> _logger;
        private readonly IAuditLogger _auditLogger;

        public LoginFirstTimeCommandHandler(IGenericRepository<User> repository, IPasswordService passwordService,
            IAuditLogger auditLogger,
            ILogger<LoginFirstTimeCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        }

        public async Task<Result> Handle(LoginFirstTimeCommand request, CancellationToken cancellationToken)
        {
            var spec = new GetUserByUserName(request.UserName);

            User? user = await _repository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
            var matchPassword = _passwordService.Verify(request.Password, user.PasswordHash);
            if (!matchPassword)
                return Result.Fail(new Error
                    (ErrorMessage.WrongPassword, ErrorMessage.WrongPassword, ErrorType.Security));

            var newPassword = _passwordService.Hash(request.NewPassword);
            user.UpdatePasswordHash(newPassword);
            await _repository.SaveChangesAsync(cancellationToken);
            await _auditLogger.LogAsync(
               new AuditLogEntry(
                   EventName: "PasswordChangedFirstTime",
                   EventCategory: "Auth",
                   IsSuccess: true,
                   StatusCode: 200,  // OK
                   UserId: user.Id,
                   Extra: new { request.UserName, ChangedAt = DateTime.UtcNow }),
               cancellationToken);

            Log.ForContext("IsAudit", true)
                .Warning("User {UserName} changed password at {DateTime}", request.UserName, DateTime.UtcNow);
            _logger.LogWarning("User {UserName} changed password at {DateTime}", request.UserName, DateTime.UtcNow);
            return Result.Ok();
        }
    }
}