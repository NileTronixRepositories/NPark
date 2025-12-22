using CRM.Domain.Resources;
using FluentValidation;

namespace CRM.Application.Feature.AuthManagement.Command.Login
{
    internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ErrorMessage.Login_Email_Required)
                .EmailAddress().WithMessage(ErrorMessage.Login_Email_Invalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ErrorMessage.Login_Password_Required)
                .MinimumLength(8).WithMessage(ErrorMessage.Login_Password_MinLength)
                .MaximumLength(128).WithMessage(ErrorMessage.Login_Password_MaxLength);
        }
    }
}