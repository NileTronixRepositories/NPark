using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Command.Add
{
    public sealed record AddParkingMembershipCommand : ICommand
    {
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string NationalId { get; init; } = string.Empty;
        public List<IFormFile>? VehicleImage { get; init; }
        public string VehicleNumber { get; init; } = string.Empty;
        public string CardNumber { get; init; } = string.Empty;
        public Guid PricingSchemeId { get; init; }
    }
}