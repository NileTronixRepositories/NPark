using BuildingBlock.Domain.Primitive;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BuildingBlock.Infrastracture.Interceptors
{
    /// <summary>
    /// Interceptor عام لنشر الـ Domain Events لأي Aggregate
    /// يطبّق IHasDomainEvents بعد نجاح SaveChanges.
    /// </summary>
    public sealed class DomainEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IPublisher _publisher;
        private readonly ILogger<DomainEventsInterceptor> _logger;

        public DomainEventsInterceptor(
            IPublisher publisher,
            ILogger<DomainEventsInterceptor> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

        // بعد SaveChanges Async
        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null && result > 0)
            {
                await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
            }

            return result;
        }

        // بعد SaveChanges Sync
        public override int SavedChanges(
            SaveChangesCompletedEventData eventData,
            int result)
        {
            if (eventData.Context is not null && result > 0)
            {
                DispatchDomainEventsAsync(eventData.Context, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }

            return result;
        }

        private async Task DispatchDomainEventsAsync(
            DbContext context,
            CancellationToken cancellationToken)
        {
            // 1) لِمّ كل الكيانات اللي عندها Domain Events
            var entitiesWithEvents = context.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            if (entitiesWithEvents.Count == 0)
                return;

            // 2) لِمّ كل الـ Events في لستة واحدة
            var allEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // 3) امسح الـ Events من الـ Aggregates (علشان ما تتكررش)
            foreach (var entity in entitiesWithEvents)
            {
                entity.ClearDomainEvents();
            }

            // 4) انشر كل Event عن طريق MediatR
            foreach (var domainEvent in allEvents)
            {
                try
                {
                    _logger.LogDebug(
                        "Publishing domain event {EventType}",
                        domainEvent.GetType().Name);

                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to publish domain event {EventType}",
                        domainEvent.GetType().Name);

                    // هنا ممكن إمّا:
                    // - تسيب الاستثناء يطلع (علشان ما نبقاش في حالة نص محفوظة / نص لا)
                    // - أو تكمل (fire-and-forget) حسب السياسة اللي تحبها
                    // دلوقتي هنكمّل
                }
            }
        }
    }
}