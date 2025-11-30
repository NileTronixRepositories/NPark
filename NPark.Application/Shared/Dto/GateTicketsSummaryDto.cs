namespace NPark.Application.Shared.Dto
{
    public sealed record GateTicketsSummaryDto
    {
        /// <summary>Database identifier of the gate.</summary>
        public Guid GateId { get; init; }

        /// <summary>Human-readable gate number (e.g. 1, 2, 3).</summary>
        public int GateNumber { get; init; }

        /// <summary>Total number of tickets processed by this gate for the selected day.</summary>
        public int TicketsCount { get; init; }

        /// <summary>Total ticket price (sum of Price) for this gate.</summary>
        public decimal TotalPrice { get; init; }

        /// <summary>Total collected amount (only tickets with IsCollected = true).</summary>
        public decimal CollectedPrice { get; init; }

        /// <summary>
        /// Name of the last supervisor (collector) who collected a ticket on this gate,
        /// based on CollectedDate. Can be null if no collected tickets.
        /// </summary>
        public string? SupervisorName { get; init; }
    }

    /// <summary>
    /// Top-level dashboard statistics for a specific day.
    /// </summary>
    public sealed record DashboardDto
    {
        /// <summary>The date the statistics are calculated for.</summary>
        public DateOnly Date { get; init; }

        /// <summary>Total number of active memberships in the system.</summary>
        public int SubscriptionCount { get; init; }

        /// <summary>Total number of entry tickets issued this day.</summary>
        public int EnterTicketsCount { get; init; }

        /// <summary>Total number of tickets that reached an exit gate this day.</summary>
        public int ExitTicketsCount { get; init; }

        /// <summary>Total revenue (sum of ticket prices for the day).</summary>
        public decimal TotalRevenue { get; init; }

        /// <summary>Total collected revenue (only collected tickets).</summary>
        public decimal CollectedRevenue { get; init; }

        /// <summary>Revenue that is still pending collection (Total - Collected).</summary>
        public decimal PendingRevenue { get; init; }

        // == High-level KPIs ==

        /// <summary>Number of vehicles currently inside the parking (based on active tickets).</summary>
        public int CurrentVehiclesInside { get; init; }

        /// <summary>Parking occupancy percentage = CurrentVehiclesInside / TotalParkingSlots * 100.</summary>
        public decimal OccupancyPercentage { get; init; }

        /// <summary>Estimated free slots = TotalParkingSlots - CurrentVehiclesInside.</summary>
        public int EstimatedFreeSlots { get; init; }

        /// <summary>Number of active tickets that have not exited and not collected yet.</summary>
        public int ActiveTicketsCount { get; init; }

        // == 2) Membership Insights ==

        /// <summary>Number of tickets linked to a membership today (Ticket.IsSubscriber = true).</summary>
        public int MembershipTicketsCount { get; init; }

        /// <summary>Number of tickets without membership (walk-in) today.</summary>
        public int WalkInTicketsCount { get; init; }

        /// <summary>Total revenue associated with membership tickets today.</summary>
        public decimal MembershipRelatedRevenue { get; init; }

        /// <summary>Total revenue associated with walk-in tickets today.</summary>
        public decimal WalkInRevenue { get; init; }

        /// <summary>Number of memberships expiring in the next 7 days.</summary>
        public int MembershipsExpiringNext7Days { get; init; }

        // == Pricing & Exceed Time ==

        /// <summary>Average parking duration (in minutes) for tickets that exited today.</summary>
        public int AverageDurationMinutes { get; init; }

        /// <summary>Maximum parking duration (in minutes) for tickets that exited today.</summary>
        public int MaxDurationMinutes { get; init; }

        /// <summary>Number of tickets that have ExceedPrice > 0 today.</summary>
        public int ExceededTicketsCount { get; init; }

        /// <summary>Total exceed charges collected today.</summary>
        public decimal TotalExceedRevenue { get; init; }

        /// <summary>Average exceed charge per exceeded ticket today.</summary>
        public decimal AverageExceedPerTicket { get; init; }

        // == Gate & Cashier Performance ==

        /// <summary>Gate number with the highest tickets count today (entry or exit).</summary>
        public int BusiestGateNumber { get; init; }

        /// <summary>Number of tickets processed by the busiest gate today.</summary>
        public int BusiestGateTicketsCount { get; init; }

        /// <summary>Name of the top collector (cashier) today.</summary>
        public string? TopCollectorName { get; init; }

        /// <summary>Total amount collected by the top collector today.</summary>
        public decimal TopCollectorAmount { get; init; }

        /// <summary>Number of tickets collected by the top collector today.</summary>
        public int TopCollectorTicketsCount { get; init; }

        /// <summary>Entry gates per-gate statistics.</summary>
        public IReadOnlyList<GateTicketsSummaryDto> EntryGates { get; init; }
            = Array.Empty<GateTicketsSummaryDto>();

        /// <summary>Exit gates per-gate statistics.</summary>
        public IReadOnlyList<GateTicketsSummaryDto> ExitGates { get; init; }
            = Array.Empty<GateTicketsSummaryDto>();
    }
}