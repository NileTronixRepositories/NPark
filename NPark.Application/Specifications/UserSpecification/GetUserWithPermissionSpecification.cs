using BuildingBlock.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.UserSpecification
{
    public class GetUserWithPermissionSpecification : Specification<User>
    {
        public GetUserWithPermissionSpecification(Guid id)
        {
            AddCriteria(q => q.Id == id);
            AddInclude(q => q.Include(r => r.Role)
            .ThenInclude(r => r.GetPermissions).ThenInclude(r => r.Permission));
            UseSingleQuery();
            UseNoTracking();
        }
    }
}