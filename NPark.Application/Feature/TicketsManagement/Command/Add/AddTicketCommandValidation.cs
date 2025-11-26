using FluentValidation;

namespace NPark.Application.Feature.TicketsManagement.Command.Add
{
    public sealed class AddTicketCommandValidation : AbstractValidator<AddTicketCommand>
    {
        public AddTicketCommandValidation()
        {
            // ---------------------------
            // Common rules
            // ---------------------------

            // VehicleNumber: optional, but if provided must be valid.
            RuleFor(x => x.VehicleNumber)
                .MaximumLength(50)
                .WithMessage("Vehicle number must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.VehicleNumber));

            // ---------------------------
            // Subscriber rules
            // ---------------------------
            When(x => x.IsSubscriber, () =>
            {
                RuleFor(x => x.CardNumber)
                    .NotEmpty().WithMessage("Card number is required for subscriber tickets.")
                    .MaximumLength(50).WithMessage("Card number must not exceed 50 characters.");
            });

            // ---------------------------
            // Non-subscriber rules
            // ---------------------------
            When(x => !x.IsSubscriber, () =>
            {
                // Optional: you can enforce that CardNumber should be empty for non-subscriber.
                RuleFor(x => x.CardNumber)
                    .Empty()
                    .WithMessage("Card number must be empty for non-subscriber tickets.");
            });
        }
    }
}