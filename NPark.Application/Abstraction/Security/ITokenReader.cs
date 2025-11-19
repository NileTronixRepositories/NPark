using System.Security.Claims;

namespace NPark.Application.Abstraction.Security
{
    public interface ITokenReader
    {
        TokenInfoDto ReadFromBearer(string bearerToken);      // "Bearer x.y.z" أو "x.y.z"

        TokenInfoDto ReadFromPrincipal(ClaimsPrincipal principal);

        bool TryGetClaim(ClaimsPrincipal principal, string claimType, out string? value);

        (bool ok, T? value) TryGet<T>(ClaimsPrincipal principal, string claimType);
    }

    public sealed record TokenInfoDto
    {
        public string RawToken { get; init; } = string.Empty;

        // From payload (custom)
        public Guid? UserId { get; init; }
        public string? UserName { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
        public Guid? GateId { get; init; }
        public string? Role { get; init; }
        public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();

        // Standard JWT
        public string? Issuer { get; init; }
        public string? Audience { get; init; }
        public DateTime? IssuedAtUtc { get; init; }
        public DateTime? ExpiresAtUtc { get; init; }
        public string? Subject { get; init; }
        public string? JwtId { get; init; }
    }

    public static class JwtClaimTypesCustom
    {
        // نفس الأسماء المستخدمة في JwtProvider.Generate
        public const string UserId = "userId";

        public const string UserName = "userName";
        public const string Email = "email";
        public const string PhoneNumber = "phoneNumber";
        public const string GateId = "gateId";
        public const string Permission = "permission";
    }
}