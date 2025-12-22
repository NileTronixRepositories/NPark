using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Autorization;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string POLICY_PREFIX = "PERMISSION_";
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(POLICY_PREFIX))
        {
            var permissionSegment = policyName.Substring(POLICY_PREFIX.Length);
            var permissions = permissionSegment.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permissions))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}