using CRM.Domain.Resources;
using FluentValidation;

namespace CRM.Application.Feature.AccountManagement.Command.Add
{
    public sealed class AddAccountCommandValidator : AbstractValidator<AddAccountCommand>
    {
        public AddAccountCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ErrorMessage.Login_Email_Required)
                .EmailAddress().WithMessage(ErrorMessage.Login_Email_Invalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ErrorMessage.Login_Password_Required)
                .MinimumLength(8).WithMessage(ErrorMessage.Login_Password_MinLength)
                .MaximumLength(128).WithMessage(ErrorMessage.Login_Password_MaxLength);
            RuleFor(x => x.NameEn)
    .NotEmpty().WithMessage(ErrorMessage.Account_NameEn_Unique);
            When(x => x.Sites != null, () =>
            {
                RuleFor(x => x.Sites)
                    .NotNull().WithMessage(ErrorMessage.Account_Sites_Required)
                    .NotEmpty().WithMessage(ErrorMessage.Account_Sites_Empty)
                .Must(HaveUniqueNameEn)
                .WithMessage(ErrorMessage.Site_NameEn_Unique); ;

                RuleForEach(x => x.Sites)
                    .SetValidator(new SiteDtoValidator());
            });
        }

        private static bool HaveUniqueNameEn(List<SiteDto> sites)
        {
            if (sites is null || sites.Count == 0) return true; // emptiness handled elsewhere

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var s in sites)
            {
                // NameEn required validated in SiteDtoValidator,
                // هنا بس نحمي نفسنا من null/space
                var key = (s?.NameEn ?? string.Empty).Trim();

                if (key.Length == 0) continue;

                if (!seen.Add(key))
                    return false;
            }

            return true;
        }
    }

    public sealed class SiteDtoValidator : AbstractValidator<SiteDto>
    {
        public SiteDtoValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty().WithMessage(ErrorMessage.Site_NameEn_Required);
        }
    }
}