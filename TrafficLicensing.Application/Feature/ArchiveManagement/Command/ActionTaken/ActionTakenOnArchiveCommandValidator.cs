using FluentValidation;
using TrafficLicensing.Domain.Enum;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Command.ActionTaken
{
    internal class ActionTakenOnArchiveCommandValidator : AbstractValidator<ActionTakenOnArchiveCommand>
    {
        public ActionTakenOnArchiveCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("المعرّف مطلوب. | Id is required.");

            RuleFor(x => x.ActionTaken)
                .IsInEnum()
                .WithMessage("قيمة الإجراء غير صحيحة. | Action must be a valid value.");

            When(x => x.ActionTaken == ArchiveAction.Rejected, () =>
            {
                RuleFor(x => x.RejectReason)
                    .NotEmpty()
                    .WithMessage("سبب الرفض مطلوب عند اختيار (Rejected). | Reject reason is required when Action is Rejected.");
            });
        }
    }
}