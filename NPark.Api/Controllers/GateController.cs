using BuildingBlock.Api.ControllerTemplate;
using MediatR;

namespace NPark.Api.Controllers
{
    public class GateController : ControllerTemplate
    {
        public GateController(ISender sender) : base(sender)
        {
        }
    }
}