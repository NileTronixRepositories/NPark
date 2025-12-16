using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TrafficLicensing.Application.Feature.ArchiveManagement.Command.ActionTaken;
using TrafficLicensing.Application.Feature.ArchiveManagement.Command.Add;
using TrafficLicensing.Application.Feature.ArchiveManagement.Query.GetAll;
using TrafficLicensing.Application.Feature.ArchiveManagement.Query.GetToday;

namespace TrafficLicensing.Api.Controllers
{
    [Route("api/[controller]")]
    public class ArchiveRequestsController : ControllerTemplate
    {
        public ArchiveRequestsController(ISender sender) : base(sender)
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddArchiveRequestCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetAllArchiveRequestsQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPost(nameof(ActionTaken))]
        public async Task<IActionResult> ActionTaken([FromBody] ActionTakenOnArchiveCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpGet(nameof(GetToday))]
        public async Task<IActionResult> GetToday([FromQuery] GetArchiveTodayQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}