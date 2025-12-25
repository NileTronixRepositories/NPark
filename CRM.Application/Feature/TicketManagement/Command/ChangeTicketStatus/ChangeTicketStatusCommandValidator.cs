using CRM.Domain.Resources;
using FluentValidation;

namespace CRM.Application.Feature.TicketManagement.Command.ChangeTicketStatus
{
    internal sealed class ChangeTicketStatusCommandValidator
       : AbstractValidator<ChangeTicketStatusCommand>
    {
        public ChangeTicketStatusCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(ErrorMessage.Ticket_Id_Required);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(ErrorMessage.Ticket_Status_Invalid);

            RuleFor(x => x.Severity)
                .IsInEnum().WithMessage(ErrorMessage.Ticket_Severity_Invalid);

            // Description optional, but if provided must not be whitespace + limit length
            When(x => x.Description is not null, () =>
            {
                RuleFor(x => x.Description!)
                    .Must(d => !string.IsNullOrWhiteSpace(d))
                    .WithMessage(ErrorMessage.Ticket_Description_Empty);

                RuleFor(x => x.Description!)
                    .MaximumLength(2000)
                    .WithMessage(ErrorMessage.Ticket_Description_MaxLength);
            });
        }
    }
}