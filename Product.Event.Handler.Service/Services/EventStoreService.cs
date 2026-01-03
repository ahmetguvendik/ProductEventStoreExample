using System.Reflection;
using System.Text.Json;
using System.Linq;
using Product.Event.Handler.Service.Handlers;
using Shared.Events;
using Shared.Services.Abstractions;

namespace Product.Event.Handler.Service.Services;

public class EventStoreService : BackgroundService
{
    private readonly IEventStoreService _eventStoreService;
    private readonly ProductEventHandler _productHandler;

    public EventStoreService(IEventStoreService eventStoreService, ProductEventHandler productHandler)
    {
        _eventStoreService = eventStoreService;
        _productHandler = productHandler;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _eventStoreService.SubscribeToStreamAsync(
            streamName: "product-stream", async (subscription, resolvedEvent, cancellationToken) =>
            {
                string eventTypeName = resolvedEvent.Event.EventType;
                var eventType = Assembly
                    .Load("Shared")
                    .GetTypes()
                    .FirstOrDefault(t => string.Equals(t.Name, eventTypeName, StringComparison.Ordinal));

                if (eventType is null)
                {
                    // Tür bulunamadı; ileride log eklenebilir
                    return;
                }

                object? @event = JsonSerializer.Deserialize(resolvedEvent.Event.Data.ToArray(), eventType);
                switch (@event)
                {
                    case ProductCreatedEvent created:
                        Console.WriteLine($"[EVENT] ProductCreatedEvent Id={created.Id}, Name={created.Name}");
                        await _productHandler.Handle(created, cancellationToken);
                        break;
                    case ProductUpdatedEvent updated:
                        Console.WriteLine($"[EVENT] ProductUpdatedEvent Id={updated.Id}, Name={updated.Name}");
                        await _productHandler.Handle(updated, cancellationToken);
                        break;
                    case ProductDeletedEvent deleted:
                        Console.WriteLine($"[EVENT] ProductDeletedEvent Id={deleted.Id}");
                        await _productHandler.Handle(deleted, cancellationToken);
                        break;
                    default:
                        // Bilinmeyen event tipi; log eklenebilir.
                        break;
                }
            }
        );
    }
}