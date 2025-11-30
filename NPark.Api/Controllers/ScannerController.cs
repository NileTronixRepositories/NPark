using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NPark.Application.Feature.ScannerManagement.Command;

namespace NPark.Api.Controllers
{
    [Route("api/[controller]")]
    public class ScannerController : ControllerTemplate
    {
        public ScannerController(ISender sender) : base(sender)
        {
        }

        [HttpPost(nameof(GetScannerInfo))]
        public async Task<IActionResult> GetScannerInfo([FromForm] GetScannerInfoCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }
    }
}