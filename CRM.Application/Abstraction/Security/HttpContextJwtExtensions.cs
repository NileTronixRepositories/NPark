using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CRM.Application.Abstraction.Security;

public static class HttpContextJwtExtensions
{
    public static TokenInfoDto ReadToken(this HttpContext http, ITokenReader reader)
    {
        // جرّب أولاً من Authorization header
        var bearer = http.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(bearer))
            return reader.ReadFromBearer(bearer);

        return reader.ReadFromPrincipal(http.User);
    }

    public static Guid? GetUserId(this ClaimsPrincipal user, ITokenReader reader)
        => reader.TryGet<Guid>(user, JwtClaimTypesCustom.UserId).value;

    public static string? GetRole(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Role);

    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal user)
        => user.FindAll(JwtClaimTypesCustom.Permission).Select(x => x.Value);
}