using BuildingBlock.Domain.Specification;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.TicketSpecification
{
    public sealed class TicketByUniquePartSpecification : Specification<Ticket>
    {
        public TicketByUniquePartSpecification(byte[] uniquePart)
        {
            if (uniquePart is null || uniquePart.Length != 4)
                throw new ArgumentException("uniquePart must be exactly 4 bytes.");

            AddCriteria(t => t.UniqueGuidPart == uniquePart);
        }
    }
}