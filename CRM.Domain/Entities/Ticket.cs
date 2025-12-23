using BuildingBlock.Domain.EntitiesHelper;

namespace CRM.Domain.Entities
{
    public sealed class Ticket : Entity<Guid>
    {
        public string Description { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;
    }
}