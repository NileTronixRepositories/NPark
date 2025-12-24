using CRM.Domain.Resources;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace CRM.Application.Feature.TicketManagement.Command.Add
{
    internal sealed class AddTicketCommandValidator : AbstractValidator<AddTicketCommand>
    {
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "application/pdf"
        };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".pdf"
        };

        private const long MaxAttachmentBytes = 5 * 1024 * 1024; // 5 MB

        public AddTicketCommandValidator()
        {
            RuleFor(x => x.Description)
                 .NotEmpty().WithMessage(ErrorMessage.Ticket_Description_Required);

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage(ErrorMessage.Ticket_Subject_Required);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ErrorMessage.Login_Email_Required)
                .EmailAddress().WithMessage(ErrorMessage.Login_Email_Invalid);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(ErrorMessage.Ticket_Phone_Required)
                    .Matches(@"^(?:\+20|0020|0)?1[0125]\d{8}$")
                    .WithMessage(ErrorMessage.Ticket_Phone_Invalid)
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.Severity)
                .IsInEnum().WithMessage(ErrorMessage.Ticket_Severity_Invalid);

            RuleFor(x => x.SiteId)
                .NotEmpty().WithMessage(ErrorMessage.Ticket_SiteId_Required);

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage(ErrorMessage.Ticket_ProductId_Required);

            // Validation for Attachments
            When(x => x.Attachments != null && x.Attachments.Any(), () =>
            {
                RuleForEach(x => x.Attachments)
                    .Must(BeValidAttachmentType)
                    .WithMessage(ErrorMessage.Product_Image_InvalidType);

                RuleForEach(x => x.Attachments)
                    .Must(f => f.Length <= MaxAttachmentBytes)
                    .WithMessage(ErrorMessage.Product_Image_MaxSize);
            });
        }

        // Validate attachment file type
        private static bool BeValidAttachmentType(IFormFile file)
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