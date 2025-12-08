namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetMembershipExpireNextSevenDays
{
    public sealed record GetMembershipExpireNextSevenDaysQueryResponse
    {
        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string PricingSchemeName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime EndDate { get; set; }
    }
}