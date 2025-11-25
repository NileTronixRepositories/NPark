namespace NPark.Application.Feature.ParkingMembershipsManagement.Query.GetSummaryById
{
    public sealed record GetCardSummaryByIdQueryResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
    }
}