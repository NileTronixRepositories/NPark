using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using CRM.Api.Attribute;
using CRM.Application.Feature.TicketManagement.Command.Add;
using CRM.Application.Feature.TicketManagement.Command.ChangeTicketStatus;
using CRM.Application.Feature.TicketManagement.Query.GetAllSuperAdmin;
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

        [Permission("Platform:Tickets:Update")]
        [HttpPost(nameof(UpdateStatus))]
        public async Task<IActionResult> UpdateStatus([FromBody] ChangeTicketStatusCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpGet(nameof(GetAllForSuperAdmin))]
        public async Task<IActionResult> GetAllForSuperAdmin([FromQuery] GetAllSuperAdminQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}