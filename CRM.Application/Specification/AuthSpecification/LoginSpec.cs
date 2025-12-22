using BuildingBlock.Domain.Specification;
using CRM.Application.Feature.AuthManagement.Command.Login;
using CRM.Domain.Entities;

namespace CRM.Application.Specification.AuthSpecification
{
    internal sealed class LoginSpec : Specification<SuperAdmin>
    {
        public LoginSpec(LoginCommand request)
        {
            AddCriteria(x => x.Email == request.Email);
            UseNoTracking();
        }
    }
}