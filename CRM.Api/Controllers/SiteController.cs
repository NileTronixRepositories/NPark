using BuildingBlock.Api;
using BuildingBlock.Api.ControllerTemplate;
using CRM.Api.Attribute;
using CRM.Application.Feature.SiteManagement.Query.GetAll;
using CRM.Application.Feature.SiteManagement.Query.GetSites;
using CRM.Application.Feature.SiteManagement.Query.GetSuperAdminSites;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [Route("api/[controller]")]
    public class SiteController : ControllerTemplate
    {
        public SiteController(ISender sender) : base(sender)
        {
        }

        [Permission("Account:Site:Read")]
        [HttpGet(nameof(GetSites))]
        public async Task<IActionResult> GetSites([FromQuery] GetSitesQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Account:Site:Read")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllSitesQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [Permission("Platform:Site:Read")]
        [HttpGet(nameof(GetSuperAdminSites))]
        public async Task<IActionResult> GetSuperAdminSites([FromQuery] GetSuperAdminSitesQuery query, CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }
    }
}