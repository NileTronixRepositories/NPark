using BuildingBlock.Domain.EntitiesHelper;

namespace NPark.Domain.Entities
{
    public class ParkingMemberships : Entity<Guid>
    {
        private List<ParkingMembershipsAttachment> _attachments = new List<ParkingMembershipsAttachment>();
        public string Name { get; private set; } = string.Empty;

        public string Phone { get; private set; } = string.Empty;
        public string NationalId { get; private set; } = string.Empty;

        public string VehicleNumber { get; private set; } = string.Empty;
        public string CardNumber { get; private set; } = string.Empty;
        public Guid PricingSchemeId { get; private set; }
        public PricingScheme PricingScheme { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime EndDate { get; private set; }
        public IReadOnlyList<ParkingMembershipsAttachment> Attachments => _attachments;

        private ParkingMemberships()
        { }

        public static ParkingMemberships Create(string name, string phone, string nationalId, string vehicleNumber, string cardNumber, Guid pricingSchemeId, DateTime createdAt, DateTime endDate) => new ParkingMemberships()
        {
            Name = name,
            Phone = phone,
            NationalId = nationalId,
            VehicleNumber = vehicleNumber,
            CardNumber = cardNumber,
            PricingSchemeId = pricingSchemeId,
            CreatedAt = createdAt,
            EndDate = endDate
        };

        public void UpdateEndDate(DateTime endDate) => EndDate = endDate;

        public void AddAttachment(string filePath) => _attachments.Add(ParkingMembershipsAttachment.Create(this.Id, filePath));

        public void RemoveAttachment(Guid attachmentId)
        {
            var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId);
            if (attachment != null)
            {
                _attachments.Remove(attachment);
            }
        }

        public void RemoveAllAttachments() => _attachments.Clear();

        public void UpdateVehicleNumber(string vehicleNumber) => VehicleNumber = vehicleNumber;

        public void UpdateCardNumber(string cardNumber) => CardNumber = cardNumber;

        public void UpdatePricingSchemeId(Guid pricingSchemeId) => PricingSchemeId = pricingSchemeId;

        public void UpdateName(string name) => Name = name;

        public void UpdatePhone(string phone) => Phone = phone;

        public void UpdateNationalId(string nationalId) => NationalId = nationalId;

        public void UpdateCreatedAt(DateTime createdAt) => CreatedAt = createdAt;
    }
}