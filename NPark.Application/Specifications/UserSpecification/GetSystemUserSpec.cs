using BuildingBlock.Domain.Specification;
using NPark.Application.Feature.UserManagement.Query.GetUsers;
using NPark.Domain.Entities;

namespace NPark.Application.Specifications.UserSpecification
{
    public sealed class GetSystemUserSpec : Specification<User, GetSystemUserQueryResponse>
    {
        public GetSystemUserSpec()
        {
            Include(q => q.Role);
            UseNoTracking();
            Select(q => new GetSystemUserQueryResponse
            {
                Id = q.Id,
                Email = q.Email,
                RoleName = q.Role.NameEn,
                Username = q.Username
            });
        }
    }
}