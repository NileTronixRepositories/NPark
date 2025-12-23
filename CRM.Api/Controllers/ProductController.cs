using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using CRM.Api.Attribute;
using CRM.Application.Feature.ProductManagement.Command.Add;
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
    }
}