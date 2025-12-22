using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using CRM.Application.Feature.AuthManagement.Command.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerTemplate
    {
        public AuthController(ISender sender) : base(sender)
        {
        }

        [HttpPost(nameof(Login))]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }
    }
}