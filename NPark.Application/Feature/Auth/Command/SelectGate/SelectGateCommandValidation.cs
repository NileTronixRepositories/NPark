using FluentValidation;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.SelectGate
{
    public class SelectGateCommandValidation : AbstractValidator<SelectGateCommand>
    {
        public SelectGateCommandValidation()
        {
            RuleFor(x => x.GateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.IsRequired);
        }
    }
}