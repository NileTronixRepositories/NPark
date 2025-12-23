using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CRM.Application.Specification.ProductSpecification;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Feature.ProductManagement.Query.GetAll
{
    internal sealed class GetAllProductQueryHandler : IQueryHandler<GetAllProductQuery, Pagination<GetAllProductQueryResponse>>
    {
        private readonly IGenericRepository<Product> _productRepository;

        public GetAllProductQueryHandler(IGenericRepository<Product> productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<Result<Pagination<GetAllProductQueryResponse>>> Handle(GetAllProductQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetAllProductSpec(request);
            (var data, var count) = _productRepository.GetWithSpec(spec);

            var result = new Pagination<GetAllProductQueryResponse>(request.PageNumber, request.PageSize, count,
                await data.ToListAsync(cancellationToken));
            return Result<Pagination<GetAllProductQueryResponse>>.Ok(result);
        }
    }
}