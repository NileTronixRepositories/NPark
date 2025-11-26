using FluentValidation;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.Login
{
    public sealed class LoginCommandValidation : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidation()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.UserName_Required)
                .MaximumLength(100)
                .WithMessage("يجب ألا يزيد اسم المستخدم عن 100 حرف (Username must not exceed 100 characters).");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.PasswordRequired);
        }
    }
}