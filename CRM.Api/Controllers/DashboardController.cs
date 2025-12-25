using BuildingBlock.Api.ControllerTemplate;
using MediatR;

namespace CRM.Api.Controllers
{
    public class DashboardController : ControllerTemplate
    {
        public DashboardController(ISender sender) : base(sender)
        {
        }
    }
}