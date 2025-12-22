using Microsoft.AspNetCore.Authorization;

namespace CRM.Infrastructure.Autorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userPermissions = context.User.FindAll("permission").Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

        bool hasAnyPermission = requirement.Permissions.Any(p => userPermissions.Contains(p));

        if (hasAnyPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public List<string> Permissions { get; }

    public PermissionRequirement(IEnumerable<string> permissions)
    {
        Permissions = permissions.Select(p => p.Trim()).ToList();
    }
}