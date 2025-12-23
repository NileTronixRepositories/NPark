using BuildingBlock.Domain.Specification;
using CRM.Domain.Entities;

namespace CRM.Application.Specification.AccountSpecification
{
    internal class GetAccountByEmailSpec : Specification<Account>
    {
        public GetAccountByEmailSpec(string email)
        {
            AddCriteria(x => x.Email == email);
            UseNoTracking();
        }
    }
}