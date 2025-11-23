using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Api.Attribute;
using NPark.Application.Feature.ParkingMembershipsManagement.Command.Add;
using NPark.Application.Feature.ParkingMembershipsManagement.Query.GetAll;

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
    }
}