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
                .NotEmpty().WithMessage(ErrorMessage.Account_NameEn_Unique); // (أنصح تغيّرها من Unique ل Required)

            RuleFor(x => x.Sites)
                .NotNull().WithMessage(ErrorMessage.Account_Sites_Required)
                .NotEmpty().WithMessage(ErrorMessage.Account_Sites_Empty)
                .Must(HaveUniqueNameEn)
                .WithMessage(ErrorMessage.Site_NameEn_Unique);

            RuleForEach(x => x.Sites)
                .SetValidator(new SiteDtoValidator());
        }

        private static bool HaveUniqueNameEn(List<SiteDto> sites)
        {
            if (sites is null || sites.Count == 0) return true;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var s in sites)
            {
                var key = (s?.NameEn ?? string.Empty).Trim();
                if (key.Length == 0) continue;
                if (!seen.Add(key)) return false;
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

            // Products: not required, but if not null validate items
            When(x => x.Products is not null, () =>
            {
                // اختياري: تمنع إرسال [] فاضية
                RuleFor(x => x.Products!)
                    .NotEmpty().WithMessage(ErrorMessage.Site_Products_Empty);

                RuleForEach(x => x.Products!)
                    .SetValidator(new AddProductDtoValidator());
            });
        }
    }

    public sealed class AddProductDtoValidator : AbstractValidator<AddProductDto>
    {
        public AddProductDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage(ErrorMessage.Product_ProductId_Required);

            RuleFor(x => x.SupportEndDate)
                .Must(d => d != default)
                .WithMessage(ErrorMessage.Product_SupportEndDate_Required);
        }
    }
}