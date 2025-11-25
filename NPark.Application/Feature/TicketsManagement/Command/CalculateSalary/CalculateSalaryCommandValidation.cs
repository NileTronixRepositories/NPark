using FluentValidation;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalary
{
    public class CalculateSalaryCommandValidation : AbstractValidator<CalculateSalaryCommand>
    {
        public CalculateSalaryCommandValidation()
        {
            RuleFor(x => x.QrCode)
                .NotEmpty()
                .WithMessage("QR Code is required.")
                .Matches(@"^[A-Za-z0-9\+/=]+$")  // QR Code must be base64 encoded
                .WithMessage("QR Code must be a valid base64 string.");
        }
    }
}