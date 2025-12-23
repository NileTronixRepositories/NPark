using CRM.Domain.Resources;
using FluentValidation;

namespace CRM.Application.Feature.TicketManagement.Command.Add
{
    internal sealed class AddTicketCommandValidator : AbstractValidator<AddTicketCommand>
    {
        public AddTicketCommandValidator()
        {
            RuleFor(x => x.Description)
                 .NotEmpty().WithMessage(ErrorMessage.Ticket_Description_Required);

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage(ErrorMessage.Ticket_Subject_Required);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ErrorMessage.Login_Email_Required)
                .EmailAddress().WithMessage(ErrorMessage.Login_Email_Invalid);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(ErrorMessage.Ticket_Phone_Required)
                    .Matches(@"^(?:\+20|0020|0)?1[0125]\d{8}$")
                    .WithMessage(ErrorMessage.Ticket_Phone_Invalid)
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.Severity)
                .IsInEnum().WithMessage(ErrorMessage.Ticket_Severity_Invalid);

            RuleFor(x => x.SiteId)
                .NotEmpty().WithMessage(ErrorMessage.Ticket_SiteId_Required);
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage(ErrorMessage.Ticket_ProductId_Required);
        }
    }
}