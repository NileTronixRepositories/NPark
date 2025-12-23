using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using CRM.Application.Specification.ProductSpecification;
using CRM.Domain.Entities;

namespace CRM.Application.Feature.ProductManagement.Query.GetProducts
{
    internal sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IReadOnlyList<GetProductsResponse>>
    {
        private readonly IGenericRepository<Product> _productRepository;

        public GetProductsQueryHandler(IGenericRepository<Product> repository)
        {
            _productRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Result<IReadOnlyList<GetProductsResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetProductsSpec();
            var result = await _productRepository.ListWithSpecAsync(spec, cancellationToken);
            return Result<IReadOnlyList<GetProductsResponse>>.Ok(result);
        }
    }
}