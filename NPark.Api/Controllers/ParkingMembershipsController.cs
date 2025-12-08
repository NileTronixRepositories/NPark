using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Api.Attribute;
using NPark.Application.Feature.ParkingMembershipsManagement.Command.Add;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetActiveMembership;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetAll;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetInactiveMembership;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetMembershipExpireNextSevenDays;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetSummaryById;

namespace NPark.Api.Controllers
{
    [Route("api/[controller]")]
    public class ParkingMembershipsController : ControllerTemplate
    {
        public ParkingMembershipsController(ISender sender) : base(sender)
        {
        }

        [Permission("Create")]
        [HttpPost(nameof(Add))]
        public async Task<IActionResult> Add([FromForm] AddParkingMembershipCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Read")]
        [HttpGet(nameof(GetAll))]
        public async Task<IActionResult> GetAll([FromQuery] GetAllParkingMembershipQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpGet(nameof(GetCardSummary))]
        public async Task<IActionResult> GetCardSummary([FromQuery] GetCardSummaryByIdQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpGet(nameof(GetActiveMembership))]
        public async Task<IActionResult> GetActiveMembership([FromQuery] GetActiveMembershipQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpGet(nameof(GetInactiveMembership))]
        public async Task<IActionResult> GetInactiveMembership([FromQuery] GetInactiveMembershipQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpGet(nameof(GetMembershipExpireNextSevenDays))]
        public async Task<IActionResult> GetMembershipExpireNextSevenDays([FromQuery] GetMembershipExpireNextSevenDaysQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}