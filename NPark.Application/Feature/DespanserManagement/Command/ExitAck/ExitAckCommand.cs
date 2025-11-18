using BuildingBlock.Application.Abstraction;

namespace NPark.Application.Feature.DespanserManagement.Command.ExitAck
{
    public class ExitAckCommand : ICommand<string>
    {
        //Gate Number
        public string Received_data { get; set; } = string.Empty;

        //Card Number
        public int Length { get; set; }

        //Status
        public string Type { get; set; } = string.Empty;
    }
}