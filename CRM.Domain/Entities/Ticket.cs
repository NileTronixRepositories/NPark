using BuildingBlock.Domain.EntitiesHelper;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    public sealed class Ticket : Entity<Guid>
    {
        public string Description { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;
        public string Subject { get; private set; } = string.Empty;
        public string? PhoneNumber { get; private set; } = string.Empty;
        public TicketStatus Status { get; private set; }
        public TicketSeverity Severity { get; private set; }
        public Guid SiteId { get; private set; }
        public Site Site { get; private set; }
        public Guid ProductId { get; private set; }
        public Product Product { get; private set; }

        private Ticket()
        { }

        public static Ticket Create(string description, string email, string subject, string? phoneNumber, TicketSeverity severity,
            Guid site, Guid product) => new Ticket()
            {
                Description = description,
                Email = email,
                Status = TicketStatus.Pending,
                Severity = severity,
                Subject = subject,
                PhoneNumber = phoneNumber,
                SiteId = site,
                ProductId = product
            };
    }
}