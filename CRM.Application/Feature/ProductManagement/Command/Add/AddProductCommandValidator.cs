using CRM.Domain.Resources;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace CRM.Application.Feature.ProductManagement.Command.Add
{
    internal sealed class AddProductCommandValidator : AbstractValidator<AddProductCommand>
    {
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private const long MaxImageBytes = 2 * 1024 * 1024; // 2 MB

        public AddProductCommandValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty().WithMessage(ErrorMessage.Product_NameEn_Required);

            // Optional: limit description length (remove if you don't want it)
            RuleFor(x => x.DescriptionEn)
                .MaximumLength(2000).WithMessage(ErrorMessage.Product_DescriptionEn_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.DescriptionEn));

            RuleFor(x => x.DescriptionAr)
                .MaximumLength(2000).WithMessage(ErrorMessage.Product_DescriptionAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.DescriptionAr));

            // Image not required, but if provided validate it
            When(x => x.ImagePath is not null, () =>
            {
                RuleFor(x => x.ImagePath!)
                    .Must(BeValidImageType)
                    .WithMessage(ErrorMessage.Product_Image_InvalidType);

                RuleFor(x => x.ImagePath!)
                    .Must(f => f.Length > 0 && f.Length <= MaxImageBytes)
                    .WithMessage(ErrorMessage.Product_Image_MaxSize);
            });
        }

        private static bool BeValidImageType(IFormFile file)
        {
            if (!AllowedContentTypes.Contains(file.ContentType))
                return false;

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
                return false;

            return true;
        }
    }
}