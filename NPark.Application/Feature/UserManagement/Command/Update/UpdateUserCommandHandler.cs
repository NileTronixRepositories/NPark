using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.UserManagement.Command.Update
{
    public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IAuditLogger _auditLogger;

        public UpdateUserCommandHandler(IGenericRepository<User> userRepository, IAuditLogger auditLogger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _auditLogger = auditLogger;
        }

        public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            entity!.UpdateEmail(request.Email);
            entity.UpdateUserName(request.UserName);
            entity.SetRole(request.RoleId);
            await _userRepository.SaveChangesAsync(cancellationToken);

            await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "UpdateUser",
                        EventCategory: "UserManagement",
                        IsSuccess: true,
                        StatusCode: 200,
                        UserId: entity.Id,
                        Extra: new
                        {
                            entity.Name,
                            entity.Email,
                            entity.Username,
                            entity.Role
                        }),
                    cancellationToken);

            return Result.Ok();
        }
    }
}