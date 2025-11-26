using FluentValidation;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.LoginFirstTime
{
    public sealed class LoginFirstTimeCommandValidation : AbstractValidator<LoginFirstTimeCommand>
    {
        public LoginFirstTimeCommandValidation()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.UserName_Required)
                .MaximumLength(100)
                .WithMessage("يجب ألا يزيد اسم المستخدم عن 100 حرف (Username must not exceed 100 characters).");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.PasswordRequired);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage(ErrorMessage.NewPassword_Required);

            RuleFor(x => x.ConfirmedPassword)
                .NotEmpty()
                .WithMessage(ErrorMessage.ConfirmPasswordRequired);

            RuleFor(x => x.ConfirmedPassword)
                .Equal(x => x.NewPassword)
                .WithMessage(ErrorMessage.PasswordMismatch);
        }
    }
}