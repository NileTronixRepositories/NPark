using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using CRM.Api.Attribute;
using CRM.Application.Feature.AccountManagement.Command.Add;
using CRM.Application.Feature.AccountManagement.Query.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : ControllerTemplate
    {
        public AccountController(ISender sender) : base(sender)
        {
        }

        [Permission("Platform:Centers:Create")]
        [HttpPost(nameof(Add))]
        public async Task<IActionResult> Add([FromBody] AddAccountCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Platform:Centers:Read")]
        [HttpGet(nameof(GetAll))]
        public async Task<IActionResult> GetAll([FromQuery] GetAllAccountQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}