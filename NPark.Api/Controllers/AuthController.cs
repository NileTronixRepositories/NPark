using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Application.Feature.Auth.Command.Login;
using NPark.Application.Feature.Auth.Command.LoginFirstTime;
using NPark.Application.Feature.Auth.Command.LogOut;
using NPark.Application.Feature.Auth.Command.SelectGate;
using NPark.Application.Feature.Auth.Command.SupervisorLogin;

namespace NPark.Api.Controllers
{
    [Route("api/auth")]
    public class AuthController : ControllerTemplate
    {
        public AuthController(ISender sender) : base(sender)
        {
        }

        [HttpPost("loginfirsttime")]
        public async Task<IActionResult> LoginForFirstTime(LoginFirstTimeCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPost(nameof(Logout))]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPost(nameof(CheckGate))]
        public async Task<IActionResult> CheckGate([FromBody] SelectGateCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPost(nameof(SuperVisorLogin))]
        public async Task<IActionResult> SuperVisorLogin([FromBody] SupervisorLoginCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }
    }
}