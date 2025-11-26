using FluentValidation;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalary
{
    public class CalculateSalaryCommandValidation : AbstractValidator<CalculateSalaryCommand>
    {
        public CalculateSalaryCommandValidation()
        {
            RuleFor(x => x.QrCode)
                .NotEmpty()
                .WithMessage(ErrorMessage.QrCode_Require)
                .Matches(@"^[A-Za-z0-9\+/=]+$") // Base64
                .WithMessage(ErrorMessage.QrInvalid);
        }
    }
}