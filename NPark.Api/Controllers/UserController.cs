using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Application.Feature.UserManagement.Command.Add;
using NPark.Application.Feature.UserManagement.Command.Update;
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

        [HttpPut(nameof(update))]
        public async Task<IActionResult> update([FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPost(nameof(Create))]
        public async Task<IActionResult> Create([FromBody] AddUserCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }
    }
}