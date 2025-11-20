using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.UserManagement.Command.Update
{
    public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
    {
        private readonly IGenericRepository<User> _userRepository;

        public UpdateUserCommandHandler(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            entity.UpdateEmail(request.Email);
            entity.UpdateUserName(request.UserName);
            entity.SetRole(request.RoleId);
            await _userRepository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}