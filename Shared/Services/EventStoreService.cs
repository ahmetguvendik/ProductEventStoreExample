using System.Text.Json;
using EventStore.Client;
using Shared.Services.Abstractions;

namespace Shared.Services;

public class EventStoreService : IEventStoreService
{
    EventStoreClientSettings GetEventStoreClientSettings(string connectionString = "esdb://localhost:2113?tls=false") =>
        EventStoreClientSettings.Create(connectionString);

    EventStoreClient Client { get => new EventStoreClient(GetEventStoreClientSettings()); }

    public async Task AppendToStreamAsync(string streamName, IEnumerable<EventData> eventData)
    {
       await Client.AppendToStreamAsync(
            streamName: streamName,
            eventData: eventData,
            expectedState: StreamState.Any
        );
    }

    public EventData GenerateEventData(object @event)
    {
        return new EventData(
            eventId: Uuid.NewUuid(),
            type: @event.GetType().Name,
            data: JsonSerializer.SerializeToUtf8Bytes(@event)
        );
    }

    public async Task SubscribeToStreamAsync(string streamName, Func<StreamSubscription, ResolvedEvent, CancellationToken, Task> onEventAppeared)
    {
        await Client.SubscribeToStreamAsync(
            streamName: streamName,
            start: FromStream.Start,
            eventAppeared: onEventAppeared,
            subscriptionDropped: (subscription, reason, arg3) => Console.WriteLine($"SubscriptionDropped: {reason}")
        );
    }
}