using CRM.Application.Abstraction.Dashboard;
using CRM.Application.Shared.Dto;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Service.Dashboard
{
    internal class AdminDashboardService : IAdminDashboard
    {
        private CRMDBContext _context;

        public AdminDashboardService(CRMDBContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> AdminDashboard(CancellationToken cancellationToken = default)
        {
            var totalClients = await _context.Set<Account>().CountAsync(cancellationToken);
            var totalPendingTickets = await _context.Set<Ticket>().Where(t => t.Status == Domain.Enums.TicketStatus.Pending)
                .CountAsync(cancellationToken);
            var totalAssignedTickets = await _context.Set<Ticket>().Where(t => t.Status == Domain.Enums.TicketStatus.Assigned)
                .CountAsync(cancellationToken);
            var totalClosedTickets = await _context.Set<Ticket>().Where(t => t.Status == Domain.Enums.TicketStatus.Closed)
                .CountAsync(cancellationToken);

            return new AdminDashboardDto
            {
                TotalClients = totalClients,
                TotalPendingTickets = totalPendingTickets,
                TotalAssignedTickets = totalAssignedTickets,
                TotalClosedTickets = totalClosedTickets
            };
        }
    }
}