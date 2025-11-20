using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using FluentValidation;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.UserManagement.Command.Add
{
    public sealed class AddUserCommandValidation : AbstractValidator<AddUserCommand>
    {
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IGenericRepository<User> _UserRepo;
        private readonly IPasswordService _passwordService;

        public AddUserCommandValidation(
            IGenericRepository<Role> roleRepository,
            IPasswordService passwordService,
            IGenericRepository<User> UserRepo
           )
        {
            _roleRepository = roleRepository;
            _passwordService = passwordService;
            _UserRepo = UserRepo;

            // 🔹 Name Validation
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required. / الاسم مطلوب.");

            // 🔹 Email Validation
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ErrorMessage.Email_Required)
                .EmailAddress()
                .WithMessage(ErrorMessage.Invalid_Email)
                .MustAsync(async (email, ct) =>
                    !await _UserRepo.IsExistAsync(u => u.Email == email, ct))
                .WithMessage(ErrorMessage.Email_Exist);

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.UserName_Required)
                .MaximumLength(100)
                .WithMessage("User name must not exceed 100 characters. / يجب ألا يزيد اسم المستخدم عن 100 حرف.")
                .MustAsync(async (userName, ct) =>
                    !await _UserRepo.IsExistAsync(u => u.Username == userName, ct))
                .WithMessage(ErrorMessage.UserName_Exist);

            // 🔹 Password Validation (using PasswordStrength service)
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.PasswordRequired)
                .Must(password => _passwordService.IsStrongPassword(password))
                .WithMessage(ErrorMessage.Invalid_Password);

            // 🔹 RoleId Validation (must exist in the database)
            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage(ErrorMessage.Role_Requred)
                .MustAsync(async (roleId, ct) =>
                    await _roleRepository.IsExistAsync(r => r.Id == roleId, ct))
                .WithMessage(ErrorMessage.Role_Exist);

            RuleFor(x => x.NationalId)
              .NotEmpty()
              .WithMessage(ErrorMessage.IsRequired)
              .Length(14) // Assuming the National ID must be 14 digits
              .WithMessage(ErrorMessage.Invalid_NationalId)
              .Matches(@"^\d{14}$") // Ensure the ID is numeric
              .WithMessage(ErrorMessage.Invalid_NationalId)
              .MustAsync(async (nationalId, ct) =>
                  !await _UserRepo.IsExistAsync(u => u.NationalId == nationalId, ct))
              .WithMessage(ErrorMessage.Unique_NationalId);
        }
    }
}