using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.UserSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.UserManagement.Query.GetUsers
{
    public sealed class GetSystemUserQueryHandler : IQueryHandler<GetSystemUserQuery, IReadOnlyList<GetSystemUserQueryResponse>>
    {
        private readonly IGenericRepository<User> _repository;

        public GetSystemUserQueryHandler(IGenericRepository<User> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Result<IReadOnlyList<GetSystemUserQueryResponse>>> Handle(GetSystemUserQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetSystemUserSpec();
            var entities = await _repository.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetSystemUserQueryResponse>>.Ok
                (entities

                );
        }
    }
}