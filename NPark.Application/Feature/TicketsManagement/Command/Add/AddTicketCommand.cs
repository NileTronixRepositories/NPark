using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.TicketsManagement.Command.Add
{
    public sealed record AddTicketCommand : ICommand<AddTicketCommandResponse>
    {
        public string? VehicleNumber { get; set; } = string.Empty;
        public bool IsSubscriber { get; set; } = false;
        public string? CardNumber { get; set; }
    }
}