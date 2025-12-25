using CRM.Application.Shared.Dto;

namespace CRM.Application.Abstraction.Dashboard
{
    public interface IAdminDashboard
    {
        public Task<AdminDashboardDto> AdminDashboard(CancellationToken cancellationToken = default);
    }
}