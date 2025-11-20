using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Specifications.RoleSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.RoleManagement.Query.GetRoles
{
    public sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, IReadOnlyList<GetRolesQueryResponse>>
    {
        private readonly IGenericRepository<Role> _repository;

        public GetRolesQueryHandler(IGenericRepository<Role> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Result<IReadOnlyList<GetRolesQueryResponse>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetRolesSpec();
            var entities = await _repository.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetRolesQueryResponse>>.Ok
                 (entities);
        }
    }
}