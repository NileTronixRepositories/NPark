using Microsoft.AspNetCore.Mvc;
using NPark.Infrastructure.Services;

namespace NPark.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetDashboardAsync(DateTime.Now, cancellationToken);
            return Ok(result);
        }
    }
}