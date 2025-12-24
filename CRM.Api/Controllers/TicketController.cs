using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using CRM.Api.Attribute;
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

        [Permission("Account:Tickets:Create")]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] AddTicketCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }
    }
}