using CRM.Application.Shared.Dto;

namespace CRM.Application.Abstraction
{
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message, CancellationToken ct = default);
    }
}