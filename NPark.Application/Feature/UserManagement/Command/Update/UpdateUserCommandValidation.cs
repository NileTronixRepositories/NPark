using BuildingBlock.Application.Repositories;
using FluentValidation;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.UserManagement.Command.Update
{
    public sealed class UpdateUserCommandValidation : AbstractValidator<UpdateUserCommand>
    {
        private readonly IGenericRepository<Role> _roleRepository;

        public UpdateUserCommandValidation(IGenericRepository<User> userRepository, IGenericRepository<Role> roleRepository)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            // 🔹 التحقق من Id (لازم يكون موجود)
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(ErrorMessage.IsRequired)
                .MustAsync(async (id, ct) =>
                    await userRepository.IsExistAsync(u => u.Id == id, ct))
                .WithMessage(ErrorMessage.NotFound);

            // 🔹 Email
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ErrorMessage.Email_Required)
                .EmailAddress()
                .WithMessage(ErrorMessage.Invalid_Email)
                .MustAsync(async (command, email, ct) =>
                    !await userRepository.IsExistAsync(
                        u => u.Email == email && u.Id != command.Id,
                        ct))
                .WithMessage(ErrorMessage.Email_Exist);

            // 🔹 UserName (Required + MaxLength + Unique مع استثناء نفس الـ Id)
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.UserName_Required)
                .MaximumLength(100)
                .WithMessage("User name must not exceed 100 characters. / يجب ألا يزيد اسم المستخدم عن 100 حرف.")
                .MustAsync(async (command, userName, ct) =>
                    !await userRepository.IsExistAsync(
                        u => u.Username == userName && u.Id != command.Id,
                        ct))
                .WithMessage(ErrorMessage.UserName_Exist);

            // 🔹 RoleId (مش فاضى ومش Guid.Empty)
            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage(ErrorMessage.Role_Requred)
                .Must(roleId => roleId != Guid.Empty)
                .WithMessage(ErrorMessage.Role_Requred)
                .MustAsync(async (roleId, ct) =>
                    await _roleRepository.IsExistAsync(r => r.Id == roleId, ct))
                .WithMessage(ErrorMessage.Role_Exist); ;
        }
    }
}