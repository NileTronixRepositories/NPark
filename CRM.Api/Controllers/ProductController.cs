using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using CRM.Api.Attribute;
using CRM.Application.Feature.ProductManagement.Command.Add;
using CRM.Application.Feature.ProductManagement.Query.GetAll;
using CRM.Application.Feature.ProductManagement.Query.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : ControllerTemplate
    {
        public ProductController(ISender sender) : base(sender)
        {
        }

        [Permission("Platform:Products:Create")]
        [HttpPost(nameof(Add))]
        public async Task<IActionResult> Add([FromForm] AddProductCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Platform:Products:Read")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllProductQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Platform:Products:Read")]
        [HttpGet(nameof(GetProducts))]
        public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}