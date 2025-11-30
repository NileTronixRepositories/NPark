namespace NPark.Application.Abstraction
{
    public interface IRealtimeNotifier
    {
        /// <summary>
        /// Publish a message to a given channel (group/event name) with an arbitrary payload.
        /// </summary>
        /// <param name="channel">Logical channel name (e.g. "tickets:added").</param>
        /// <param name="payload">Any serializable object.</param>
        Task PublishAsync(string channel, object payload, CancellationToken cancellationToken = default);
    }
}