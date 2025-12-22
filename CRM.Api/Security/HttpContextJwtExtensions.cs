using CRM.Application.Abstraction.Security;
using System.Security.Claims;

namespace CRM.Api.Security
{
    public static class HttpContextJwtExtensions
    {
        public static TokenInfoDto ReadToken(this HttpContext http, ITokenReader reader)
        {
            // جرّب أولاً من Authorization header
            var bearer = http.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(bearer))
                return reader.ReadFromBearer(bearer);

            // وإلا من الـ User (في حال المصادقة تمت بالفعل عبر middleware)
            return reader.ReadFromPrincipal(http.User);
        }

        public static Guid? GetUserId(this ClaimsPrincipal user, ITokenReader reader)
            => reader.TryGet<Guid>(user, JwtClaimTypesCustom.UserId).value;

        public static string? GetRole(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Role);

        public static IEnumerable<string> GetPermissions(this ClaimsPrincipal user)
            => user.FindAll(JwtClaimTypesCustom.Permission).Select(x => x.Value);
    }
}