using EventStore.Client;

namespace Shared.Services.Abstractions;

public interface IEventStoreService
{
    Task AppendToStreamAsync(string streamName, IEnumerable<EventData> eventData); //event store a ekleme yapacak
    EventData GenerateEventData(object @event); //verdigimiz objeyi eventData a cevireeck 
    Task SubscribeToStreamAsync(string streamName, Func<StreamSubscription, ResolvedEvent, CancellationToken, Task> onEventAppeared); 
}