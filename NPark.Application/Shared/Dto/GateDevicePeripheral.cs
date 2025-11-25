using NPark.Domain.Enums;

namespace NPark.Application.Shared.Dto
{
    public sealed record GateDevicePeripheral
    {
        public string? PcIp { get; set; } = string.Empty;
        public string? LprIp { get; set; } = string.Empty;
        public bool HasPc { get; set; }
        public bool HasLpr { get; set; }
        public int GateNumber { get; set; }
        public GateType GateType { get; set; }
    }
}