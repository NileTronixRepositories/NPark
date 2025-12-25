using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using CRM.Application.Abstraction.Dashboard;
using CRM.Application.Shared.Dto;

namespace CRM.Application.Feature.DashboardManagement.Query.AdminDashboard
{
    internal sealed class AdminDashboardQueryHandler : IQueryHandler<AdminDashboardQuery, AdminDashboardDto>
    {
        private IAdminDashboard _adminDashboardService;

        public AdminDashboardQueryHandler(IAdminDashboard adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        public async Task<Result<AdminDashboardDto>> Handle(AdminDashboardQuery request, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.AdminDashboard(cancellationToken);
            return Result<AdminDashboardDto>.Ok(result);
        }
    }
}