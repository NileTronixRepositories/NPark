using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Application.Feature.RoleManagement.Query.GetRoles;

namespace NPark.Api.Controllers
{
    [Route("api/[controller]")]
    public class RoleController : ControllerTemplate

    {
        public RoleController(ISender sender) : base(sender)
        { }

        [HttpGet(nameof(GetAll))]
        public async Task<IActionResult> GetAll([FromQuery] GetRolesQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}