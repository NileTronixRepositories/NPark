using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using CRM.Application.Feature.TicketManagement.Command.Add;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [Route("api/[controller]")]
    public class TicketController : ControllerTemplate
    {
        public TicketController(ISender sender) : base(sender)
        {
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddTicketCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }
    }
}