using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.UserManagement.Command.Add
{
    public sealed class AddUserCommandHandler : ICommandHandler<AddUserCommand>
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IPasswordService _passwordService;

        public AddUserCommandHandler(IGenericRepository<User> userRepository,
            IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
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
            return Result.Ok();
        }
    }
}