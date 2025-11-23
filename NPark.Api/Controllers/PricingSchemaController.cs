using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Api.Attribute;
using NPark.Application.Feature.PricingSchemaManagement.Command.Add;
using NPark.Application.Feature.PricingSchemaManagement.Command.AddOrderSchema;
using NPark.Application.Feature.PricingSchemaManagement.Command.Delete;
using NPark.Application.Feature.PricingSchemaManagement.Command.Update;
using NPark.Application.Feature.PricingSchemaManagement.Query.GetAll;
using NPark.Application.Feature.PricingSchemaManagement.Query.GetOrderSchema;
using NPark.Application.Feature.PricingSchemaManagement.Query.GetPricingSchema;
using NPark.Application.Feature.PricingSchemaManagement.Query.GetRepeatedPricingSchema;

namespace NPark.Api.Controllers
{
    [Route("api/[controller]")]
    public class PricingSchemaController : ControllerTemplate
    {
        public PricingSchemaController(ISender sender) : base(sender)
        {
        }

        [Permission("Create")]
        [HttpPost(nameof(Add))]
        public async Task<IActionResult> Add([FromBody] AddPricingSchemaCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Update")]
        [HttpPut(nameof(Update))]
        public async Task<IActionResult> Update([FromBody] UpdatePricingSchemaCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Read")]
        [HttpGet(nameof(GetAll))]
        public async Task<IActionResult> GetAll([FromQuery] GetAllPricingSchemaQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Read")]
        [HttpGet(nameof(GetWithoudPagination))]
        public async Task<IActionResult> GetWithoudPagination([FromQuery] GetPricingSchemaQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Delete")]
        [HttpDelete(nameof(Delete))]
        public async Task<IActionResult> Delete([FromQuery] DeletePricingSchemaCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Read")]
        [HttpGet(nameof(GetRepeated))]
        public async Task<IActionResult> GetRepeated([FromQuery] GetRepeatedPricingSchemaQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Create")]
        [HttpPost(nameof(AddOrder))]
        public async Task<IActionResult> AddOrder([FromBody] AddOrderSchemaCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Read")]
        [HttpGet(nameof(GetOrder))]
        public async Task<IActionResult> GetOrder([FromQuery] GetOrderSchemaQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}