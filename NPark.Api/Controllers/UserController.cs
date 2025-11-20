using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Application.Feature.UserManagement.Query.GetUsers;

namespace NPark.Api.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ControllerTemplate
    {
        public UserController(ISender sender) : base(sender)
        {
        }

        [HttpGet(nameof(GetUserSystem))]
        public async Task<IActionResult> GetUserSystem([FromQuery] GetSystemUserQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}