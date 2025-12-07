using Microsoft.EntityFrameworkCore;
using NPark.Application.Shared.Dto;
using NPark.Domain.Entities;

namespace NPark.Infrastructure.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync(
            DateTime? date = null,
            CancellationToken cancellationToken = default);
    }

    public sealed class DashboardService : IDashboardService
    {
        private readonly NParkDBContext _context;

        public DashboardService(NParkDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<DashboardDto> GetDashboardAsync(
            DateTime? date = null,
            CancellationToken cancellationToken = default)
        {
            /*
             * ---------------------------------------------------------
             * 1) NORMALIZE DATE + BUILD [start, end)
             * ---------------------------------------------------------
             * We convert the input date (if any) to a DateOnly that
             * represents the dashboard day (local-based).
             */

            var targetDate = date.HasValue
                ? DateOnly.FromDateTime(date.Value)
                : DateOnly.FromDateTime(DateTime.Now);

            var start = targetDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
            var end = start.AddDays(1);

            /*
             * ---------------------------------------------------------
             * 2) BASE QUERY FOR TODAY'S TICKETS
             * ---------------------------------------------------------
             * All tickets created within the selected day.
             * ASSUMPTION: Ticket has CreatedOnUtc (from base Entity).
             */

            var todayTicketsQuery = _context.Set<Ticket>()
                .Where(t => t.CreatedOnUtc >= start && t.CreatedOnUtc < end);

            var enterTicketsQuery = todayTicketsQuery
                .Where(t => t.ExitGateId == null);

            var exitTicketsQuery = todayTicketsQuery
                .Where(t => t.ExitGateId != null);

            /*
             * ---------------------------------------------------------
             * 2.1) ACTIVE TICKETS & OCCUPANCY (NOT LIMITED TO DATE)
             * ---------------------------------------------------------
             * We calculate:
             * - ActiveTicketsCount: tickets that have not exited and not collected yet.
             * - CurrentVehiclesInside: same as ActiveTicketsCount (cars currently inside).
             * - OccupancyPercentage and EstimatedFreeSlots based on total parking slots.
             *
             * ASSUMPTION:
             * ParkingSystemConfiguration has AllowedParkingSlots (int?).
             */

            var activeTicketsQuery = _context.Set<Ticket>()
                .Where(t => t.ExitGateId == null && !t.IsCollected);

            var parkingConfig = await _context.Set<ParkingSystemConfiguration>()
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var totalParkingSlots = parkingConfig?.AllowedParkingSlots ?? 0;

            var activeTicketsCount = await activeTicketsQuery
                .CountAsync(cancellationToken);

            var currentVehiclesInside = activeTicketsCount;

            decimal occupancyPercentage = 0;
            int estimatedFreeSlots = 0;

            if (totalParkingSlots > 0)
            {
                occupancyPercentage =
                    (decimal)currentVehiclesInside / totalParkingSlots * 100m;

                estimatedFreeSlots =
                    Math.Max(0, totalParkingSlots - currentVehiclesInside);
            }

            /*
             * ---------------------------------------------------------
             * 3) BASIC COUNTS (SEQUENTIAL, BUT ASYNC)
             * ---------------------------------------------------------
             */

            var subscriptionCount = await _context.Set<ParkingMemberships>()
                .CountAsync(cancellationToken);
            var ActiveSubscriptionCount = await _context.Set<ParkingMemberships>()
                .Where(m => m.EndDate >= DateTime.Now)
                .CountAsync(cancellationToken);
            var InactiveSubscriptionCount = await _context.Set<ParkingMemberships>()
                .Where(m => m.EndDate < DateTime.Now)
                .CountAsync(cancellationToken);

            var enterTicketsCount = await enterTicketsQuery
                .CountAsync(cancellationToken);

            var exitTicketsCount = await exitTicketsQuery
                .CountAsync(cancellationToken);

            /*
             * ---------------------------------------------------------
             * 4) MEMBERSHIP INSIGHTS (TODAY)
             * ---------------------------------------------------------
             * We use Ticket.IsSubscriber to distinguish between
             * membership tickets and walk-in tickets.
             */

            var membershipTicketsQuery = todayTicketsQuery
                .Where(t => t.IsSubscriber);

            var membershipTicketsCount = await membershipTicketsQuery
                .CountAsync(cancellationToken);

            var membershipRelatedRevenue = membershipTicketsCount > 0
                ? await membershipTicketsQuery.SumAsync(t => t.TotalPrice, cancellationToken)
                : 0m;

            var walkInTicketsQuery = todayTicketsQuery
                .Where(t => !t.IsSubscriber);

            var walkInTicketsCount = await walkInTicketsQuery
                .CountAsync(cancellationToken);

            /*
             * Memberships expiring in the next 7 days.
             * We use ParkingMemberships.EndDate as the expiry date.
             */

            var now = DateTime.Now;
            var sevenDaysLater = now.AddDays(7);

            var membershipsExpiringNext7Days = await _context.Set<ParkingMemberships>()
                .CountAsync(m =>
                    m.EndDate >= now &&
                    m.EndDate <= sevenDaysLater,
                    cancellationToken);

            /*
             * ---------------------------------------------------------
             * 5) PARKING DURATION (EXITED TICKETS TODAY)
             * ---------------------------------------------------------
             * ASSUMPTION:
             * Ticket has StartDate (enter time) and EndDate (exit time).
             */

            var exitedTicketsForDuration = await exitTicketsQuery
                .Where(t => t.StartDate != default && t.EndDate != null)
                .Select(t => new { t.StartDate, t.EndDate })
                .ToListAsync(cancellationToken);

            int averageDurationMinutes = 0;
            int maxDurationMinutes = 0;

            if (exitedTicketsForDuration.Count > 0)
            {
                var durations = exitedTicketsForDuration
                    .Select(t => (t.EndDate!.Value - t.StartDate).TotalMinutes)
                    .ToList();

                averageDurationMinutes = (int)Math.Round(durations.Average());
                maxDurationMinutes = (int)Math.Round(durations.Max());
            }

            /*
             * Exceed usage: tickets with ExceedPrice > 0
             */

            var exceededTickets = await todayTicketsQuery
                .Where(t => t.ExceedPrice > 0)
                .Select(t => t.ExceedPrice)
                .ToListAsync(cancellationToken);

            var exceededTicketsCount = exceededTickets.Count;
            decimal totalExceedRevenue = exceededTicketsCount > 0
                ? exceededTickets.Sum()
                : 0m;

            decimal averageExceedPerTicket = exceededTicketsCount > 0
                ? totalExceedRevenue / exceededTicketsCount
                : 0m;

            /*
             * ---------------------------------------------------------
             * 6) ENTRY GATES STATISTICS (per GateId)
             * ---------------------------------------------------------
             */

            var entryGateStatsQuery =
                from t in enterTicketsQuery
                group t by t.GateId into g
                select new
                {
                    GateId = g.Key, // GateId is Guid (non-nullable)

                    TicketsCount = g.Count(),
                    TotalPrice = g.Sum(t => t.Price),

                    CollectedPrice = g
                        .Where(t => t.IsCollected)
                        .Sum(t => t.Price),

                    SupervisorName = g
                        .Where(t => t.IsCollected && t.UserCollector != null)
                        .OrderByDescending(t => t.CollectedDate)
                        .Select(t => t.UserCollector!.Name)
                        .FirstOrDefault()
                };

            var entryGates = await
                (from s in entryGateStatsQuery
                 join gate in _context.Set<ParkingGate>()
                     on s.GateId equals gate.Id
                 select new GateTicketsSummaryDto
                 {
                     GateId = gate.Id,
                     GateNumber = gate.GateNumber,
                     TicketsCount = s.TicketsCount,
                     TotalPrice = s.TotalPrice,
                     CollectedPrice = s.CollectedPrice,
                     SupervisorName = s.SupervisorName
                 })
                .OrderBy(x => x.GateNumber)
                .ToListAsync(cancellationToken);

            /*
             * ---------------------------------------------------------
             * 7) EXIT GATES STATISTICS (per ExitGateId)
             * ---------------------------------------------------------
             */

            var exitGateStatsQuery =
                from t in exitTicketsQuery
                where t.ExitGateId != null
                group t by t.ExitGateId into g
                select new
                {
                    GateId = g.Key!.Value, // ExitGateId is Guid?

                    TicketsCount = g.Count(),
                    TotalPrice = g.Sum(t => t.Price),

                    CollectedPrice = g
                        .Where(t => t.IsCollected)
                        .Sum(t => t.Price),

                    SupervisorName = g
                        .Where(t => t.IsCollected && t.UserCollector != null)
                        .OrderByDescending(t => t.CollectedDate)
                        .Select(t => t.UserCollector!.Name)
                        .FirstOrDefault()
                };

            var exitGates = await
                (from s in exitGateStatsQuery
                 join gate in _context.Set<ParkingGate>()
                     on s.GateId equals gate.Id
                 select new GateTicketsSummaryDto
                 {
                     GateId = gate.Id,
                     GateNumber = gate.GateNumber,
                     TicketsCount = s.TicketsCount,
                     TotalPrice = s.TotalPrice,
                     CollectedPrice = s.CollectedPrice,
                     SupervisorName = s.SupervisorName
                 })
                .OrderBy(x => x.GateNumber)
                .ToListAsync(cancellationToken);

            /*
             * ---------------------------------------------------------
             * 8) FINAL AGGREGATIONS (REVENUE)
             * ---------------------------------------------------------
             */

            var totalRevenue =
                entryGates.Sum(x => x.TotalPrice) +
                exitGates.Sum(x => x.TotalPrice);

            var collectedRevenue =
                entryGates.Sum(x => x.CollectedPrice) +
                exitGates.Sum(x => x.CollectedPrice);

            var pendingRevenue = totalRevenue - collectedRevenue;

            // Walk-in revenue = total - membership-related
            var walkInRevenue = totalRevenue - membershipRelatedRevenue;

            /*
             * ---------------------------------------------------------
             * 9) BUSIEST GATE (ENTRY OR EXIT)
             * ---------------------------------------------------------
             */

            int busiestGateNumber = 0;
            int busiestGateTicketsCount = 0;

            var allGateStats = entryGates
                .Select(g => new { g.GateNumber, g.TicketsCount })
                .Concat(
                    exitGates.Select(g => new { g.GateNumber, g.TicketsCount })
                );

            var busiest = allGateStats
                .OrderByDescending(x => x.TicketsCount)
                .ThenBy(x => x.GateNumber)
                .FirstOrDefault();

            if (busiest is not null)
            {
                busiestGateNumber = busiest.GateNumber;
                busiestGateTicketsCount = busiest.TicketsCount;
            }

            /*
             * ---------------------------------------------------------
             * 10) TOP COLLECTOR TODAY
             * ---------------------------------------------------------
             * ASSUMPTION:
             * Ticket has navigation UserCollector with Name.
             */

            var topCollector = await todayTicketsQuery
                .Where(t => t.IsCollected && t.UserCollector != null)
                .GroupBy(t => t.UserCollector!.Name)
                .Select(g => new
                {
                    CollectorName = g.Key,
                    TicketsCount = g.Count(),
                    TotalAmount = g.Sum(t => t.Price)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ThenByDescending(x => x.TicketsCount)
                .FirstOrDefaultAsync(cancellationToken);

            string? topCollectorName = null;
            decimal topCollectorAmount = 0m;
            int topCollectorTicketsCount = 0;

            if (topCollector is not null)
            {
                topCollectorName = topCollector.CollectorName;
                topCollectorAmount = topCollector.TotalAmount;
                topCollectorTicketsCount = topCollector.TicketsCount;
            }

            /*
             * ---------------------------------------------------------
             * 11) BUILD FINAL DASHBOARD DTO
             * ---------------------------------------------------------
             */

            var dto = new DashboardDto
            {
                Date = targetDate,

                SubscriptionCount = subscriptionCount,

                EnterTicketsCount = enterTicketsCount,
                ExitTicketsCount = exitTicketsCount,

                TotalRevenue = totalRevenue,
                CollectedRevenue = collectedRevenue,
                PendingRevenue = pendingRevenue,

                CurrentVehiclesInside = currentVehiclesInside,
                OccupancyPercentage = occupancyPercentage,
                EstimatedFreeSlots = estimatedFreeSlots,
                ActiveTicketsCount = activeTicketsCount,

                MembershipTicketsCount = membershipTicketsCount,
                WalkInTicketsCount = walkInTicketsCount,
                MembershipRelatedRevenue = membershipRelatedRevenue,
                WalkInRevenue = walkInRevenue,
                MembershipsExpiringNext7Days = membershipsExpiringNext7Days,

                AverageDurationMinutes = averageDurationMinutes,
                MaxDurationMinutes = maxDurationMinutes,
                ExceededTicketsCount = exceededTicketsCount,
                TotalExceedRevenue = totalExceedRevenue,
                AverageExceedPerTicket = averageExceedPerTicket,

                BusiestGateNumber = busiestGateNumber,
                BusiestGateTicketsCount = busiestGateTicketsCount,
                TopCollectorName = topCollectorName,
                TopCollectorAmount = topCollectorAmount,
                TopCollectorTicketsCount = topCollectorTicketsCount,

                EntryGates = entryGates,
                ExitGates = exitGates,
                ActiveSubscriptionCount = ActiveSubscriptionCount,
                InactiveSubscriptionCount = InactiveSubscriptionCount
            };

            return dto;
        }
    }
}