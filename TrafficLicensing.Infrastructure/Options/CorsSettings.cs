namespace TrafficLicensing.Infrastructure.Options
{
    public sealed class CorsSettings
    {
        public bool AllowAnyOrigin { get; set; }
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }
}