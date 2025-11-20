using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Application.Feature.TicketsManagement.Command.Add;
using NPark.Application.Feature.TicketsManagement.Query.GetTicketForEntryGate;
using NPark.Application.Feature.TicketsManagement.Query.GetTicketForExitGate;

namespace NPark.Api.Controllers
{
    [Route("api/[controller]")]
    public class TicketController : ControllerTemplate
    {
        public TicketController(ISender sender) : base(sender)
        {
        }

        [HttpPost(nameof(AddTicket))]
        public async Task<IActionResult> AddTicket(AddTicketCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return File(result.Value, "image/png", "ticket-qr.png");
        }

        [HttpGet(nameof(GetTicketForEntry))]
        public async Task<IActionResult> GetTicketForEntry([FromQuery] GetTicketForEntryGateQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpGet(nameof(GetTicketForExit))]
        public async Task<IActionResult> GetTicketForExit([FromQuery] GetTicketForExitGateQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}