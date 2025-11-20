namespace NPark.Application.Shared.Dto
{
    public sealed record UserTokenDto
    {
        public string Token { get; init; } = string.Empty;
        public string RoleName { get; init; } = string.Empty;
        public List<string> Permissions { get; init; } = new();
        public GateDevicePeripheral? GateDevicePeripheral { get; set; } = new();
    }
}