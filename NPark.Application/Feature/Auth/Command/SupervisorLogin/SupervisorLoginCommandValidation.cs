using FluentValidation;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.SupervisorLogin
{
    public sealed class SupervisorLoginCommandValidation : AbstractValidator<SupervisorLoginCommand>
    {
        public SupervisorLoginCommandValidation()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(ErrorMessage.UserName_Required)
                .MaximumLength(100)
                .WithMessage("يجب ألا يزيد اسم المستخدم عن 100 حرف (Username must not exceed 100 characters).");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorMessage.PasswordRequired);

            RuleFor(x => x.GateNumber)
                .GreaterThan(0)
                .WithMessage(ErrorMessage.UserName_Required);

            RuleFor(x => x.GateType)
                .IsInEnum()
                .WithMessage("نوع البوابة غير صالح (Invalid gate type).");
        }
    }
}