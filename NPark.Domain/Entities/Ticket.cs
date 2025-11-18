using BuildingBlock.Domain.EntitiesHelper;
using System.Buffers.Binary;
using System.Globalization;

namespace NPark.Domain.Entities
{
    public sealed class Ticket : Entity<Guid>
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public decimal Price { get; private set; }
        public decimal ExceedPrice { get; private set; } = 0;
        public decimal TotalPrice => Price + ExceedPrice;
        public bool IsCollected { get; private set; }
        public byte[] UniqueGuidPart { get; private set; }
        public string UniqueCode => BitConverter.ToString(UniqueGuidPart).Replace("-", "");

        private Ticket()
        { }

        public static Ticket Create(DateTime startDate, DateTime endDate, decimal price)
        {
            var ticket = new Ticket()
            {
                Id = Guid.NewGuid(),
                StartDate = startDate,
                EndDate = endDate,
                Price = price,
            };

            // Save the first 4 bytes of the Guid as the unique part
            ticket.UniqueGuidPart = GetGuidUniquePart(ticket.Id);

            return ticket;
        }

        public void SetCollected() => IsCollected = true;

        private static byte[] GetGuidUniquePart(Guid guid)
        {
            var first8 = guid.ToString("N").AsSpan(0, 8);
            var value = int.Parse(first8, NumberStyles.HexNumber);
            Span<byte> buf = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(buf, value);
            return buf.ToArray();
        }
    }
}