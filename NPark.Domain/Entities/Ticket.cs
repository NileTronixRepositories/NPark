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
        public string? VehicleNumber { get; private set; } = null;
        public decimal ExceedPrice { get; private set; } = 0;
        public Guid? CollectedBy { get; private set; }
        public decimal TotalPrice => Price + ExceedPrice;
        public bool IsCollected { get; private set; }
        public DateTime? CollectedDate { get; private set; }
        public byte[] UniqueGuidPart { get; private set; }
        public string UniqueCode => BitConverter.ToString(UniqueGuidPart).Replace("-", "");
        public Guid GateId { get; private set; }
        public Guid UserId { get; private set; }
        public bool IsCashierCollected { get; private set; } = false;
        public User User { get; private set; }
        public ParkingGate ParkingGate { get; private set; }
        public bool IsSubscriber { get; private set; } = false;
        public string? SubscriberNationalId { get; private set; }

        private Ticket()
        { }

        public static Ticket Create(DateTime startDate, decimal price, Guid gateId, Guid userId
            )
        {
            var ticket = new Ticket()
            {
                Id = Guid.NewGuid(),
                StartDate = startDate,
                Price = price,
                GateId = gateId,
                UserId = userId
            };

            // Save the first 4 bytes of the Guid as the unique part
            ticket.UniqueGuidPart = GetGuidUniquePart(ticket.Id);

            return ticket;
        }

        public void SetCollected(Guid Id)
        {
            IsCollected = true;
            CollectedBy = Id;
            CollectedDate = DateTime.Now;
        }

        public void SetVehicleNumber(string vehicleNumber) => VehicleNumber = vehicleNumber;

        public void SetIsCashierCollected() => IsCashierCollected = true;

        public void SetCollectedBy(Guid collectedBy) => CollectedBy = collectedBy;

        private static byte[] GetGuidUniquePart(Guid guid)
        {
            var first8 = guid.ToString("N").AsSpan(0, 8);
            var value = int.Parse(first8, NumberStyles.HexNumber);
            Span<byte> buf = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(buf, value);
            return buf.ToArray();
        }

        public void SetSubscriber(string nationalId, string? vehicleNumber = null)
        {
            Price = 0;
            IsSubscriber = true;
            IsCashierCollected = true;
            VehicleNumber = vehicleNumber;
            SubscriberNationalId = nationalId;
        }
    }
}