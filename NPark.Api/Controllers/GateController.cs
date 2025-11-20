using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Application.Feature.GateManagement.Query.GetAll;

namespace NPark.Api.Controllers
{
    [Route("api/[controller]")]
    public class GateController : ControllerTemplate
    {
        public GateController(ISender sender) : base(sender)
        {
        }

        [HttpGet(nameof(GetAll))]
        public async Task<IActionResult> GetAll([FromQuery] GetAllGateQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}