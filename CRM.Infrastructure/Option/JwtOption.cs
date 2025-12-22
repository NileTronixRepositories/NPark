namespace CRM.Infrastructure.Option
{
    public class JwtOption
    {
        public const string SectionName = "Jwt";
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpiryMinutes { get; set; }
    }
}