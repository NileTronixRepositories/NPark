using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.RoleManagement.Query.GetRoles;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.RoleSpecification
{
    public sealed class GetRolesSpec : Specification<Role, GetRolesQueryResponse>
    {
        public GetRolesSpec()
        {
            UseNoTracking();
            Select(x => new GetRolesQueryResponse
            {
                RoleId = x.Id,

                RoleName = x.NameEn
            });
        }
    }
}