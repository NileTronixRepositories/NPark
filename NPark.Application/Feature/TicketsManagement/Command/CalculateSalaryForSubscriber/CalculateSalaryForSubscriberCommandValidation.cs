using FluentValidation;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalaryForSubscriber
{
    public sealed class CalculateSalaryForSubscriberCommandValidation
        : AbstractValidator<CalculateSalaryForSubscriberCommand>
    {
        public CalculateSalaryForSubscriberCommandValidation()
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .WithMessage(ErrorMessage.IsRequired)
                .MaximumLength(50)
                .WithMessage("يجب ألا يزيد رقم الكارت عن 50 حرفًا (Card number must not exceed 50 characters).");
        }
    }
}