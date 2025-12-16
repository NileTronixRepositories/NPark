using FluentValidation;
using TrafficLicensing.Domain.Enum;

namespace TrafficLicensing.Application.Feature.ArchiveManagement.Command.Add
{
    internal class AddArchiveRequestCommandValidator : AbstractValidator<AddArchiveRequestCommand>
    {
        public AddArchiveRequestCommandValidator()
        {
            RuleFor(x => x.PlateNumber)
                .NotEmpty()
                .WithMessage("رقم اللوحة مطلوب. | Plate number is required.");

            RuleFor(x => x.Action)
                .IsInEnum()
                .WithMessage("نوع الإجراء غير صحيح. | Action must be a valid value.");

            When(x => x.Action == ArchiveType.Other, () =>
            {
                RuleFor(x => x.Note)
                    .NotEmpty()
                    .WithMessage("الملاحظة مطلوبة عندما يكون الإجراء (Other). | Note is required when Action is Other.");
            });
        }
    }
}