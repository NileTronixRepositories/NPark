using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CRM.Application.Specification.AccountSpecification;
using CRM.Domain.Entities;

namespace CRM.Application.Feature.AccountManagement.Query.GetAll
{
    internal sealed class GetAllAccountQueryHadler : IQueryHandler<GetAllAccountQuery, Pagination<GetAllAccountQueryResponse>>
    {
        private readonly IGenericRepository<Account> _accountRepository;

        public GetAllAccountQueryHadler(IGenericRepository<Account> accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<Result<Pagination<GetAllAccountQueryResponse>>> Handle(GetAllAccountQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetAllAccountSpec(request);
            var response = _accountRepository.GetWithSpec(spec);
            var result = new Pagination<GetAllAccountQueryResponse>(request.PageNumber, request.PageSize, response.count,
                response.data.ToList());
            return Result<Pagination<GetAllAccountQueryResponse>>.Ok(result);
        }
    }
}