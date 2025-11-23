using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.UserManagement.Command.Add
{
    public sealed class AddUserCommandHandler : ICommandHandler<AddUserCommand>
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IAuditLogger _auditLogger;

        public AddUserCommandHandler(IGenericRepository<User> userRepository,
            IAuditLogger auditLogger,
            IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _auditLogger = auditLogger;
        }

        public async Task<Result> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            var passwordHash = _passwordService.Hash(request.Password);
            var user = User.Create(
               name: request.Name,
               email: request.Email,
               username: request.UserName,
               passwordHash: passwordHash,
               phoneNumber: request.PhoneNumber,
               nationalId: request.NationalId
           );
            user.SetRole(request.RoleId);
            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
            await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "AddUser",
                        EventCategory: "UserManagement",
                        IsSuccess: true,
                        StatusCode: 201,  // Created
                        Extra: new
                        {
                            user.Name,
                            user.Email,
                            user.Username,
                            user.PhoneNumber
                        }),
                    cancellationToken);
            return Result.Ok();
        }
    }
}