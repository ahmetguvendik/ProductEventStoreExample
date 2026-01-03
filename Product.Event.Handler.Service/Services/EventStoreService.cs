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
                Console.WriteLine($"[DEBUG] Received event type: {eventTypeName}");
                
                var eventType = Assembly
                    .Load("Shared")
                    .GetTypes()
                    .FirstOrDefault(t => string.Equals(t.Name, eventTypeName, StringComparison.Ordinal));

                if (eventType is null)
                {
                    Console.WriteLine($"[WARNING] Event type not found: {eventTypeName}");
                    return;
                }

                object? @event = JsonSerializer.Deserialize(resolvedEvent.Event.Data.ToArray(), eventType);
                if (@event is null)
                {
                    Console.WriteLine($"[ERROR] Failed to deserialize event: {eventTypeName}");
                    return;
                }
                
                // Tip kontrolü ile handle et
                if (@event is ProductCreatedEvent created)
                {
                    Console.WriteLine($"[EVENT] ProductCreatedEvent Id={created.Id}, Name={created.Name}");
                    await _productHandler.Handle(created, cancellationToken);
                }
                else if (@event is ProductDeletedEvent deleted)
                {
                    Console.WriteLine($"[EVENT] ProductDeletedEvent Id={deleted.Id}");
                    await _productHandler.Handle(deleted, cancellationToken);
                }
                else if (@event is StockDecreasedEvent stockDecreased)
                {
                    Console.WriteLine($"[EVENT] StockDecreasedEvent Id={stockDecreased.Id}, OldStock={stockDecreased.OldStock}, NewStock={stockDecreased.NewStock}");
                    await _productHandler.Handle(stockDecreased, cancellationToken);
                }
                else if (@event is StockIncreasedEvent stockIncreased)
                {
                    Console.WriteLine($"[EVENT] StockIncreasedEvent Id={stockIncreased.Id}, OldStock={stockIncreased.OldStock}, NewStock={stockIncreased.NewStock}");
                    await _productHandler.Handle(stockIncreased, cancellationToken);
                }
                else if (@event is PriceChangedEvent priceChanged)
                {
                    Console.WriteLine($"[EVENT] PriceChangedEvent Id={priceChanged.Id}, OldPrice={priceChanged.OldPrice}, NewPrice={priceChanged.NewPrice}");
                    await _productHandler.Handle(priceChanged, cancellationToken);
                }
                else
                {
                    Console.WriteLine($"[WARNING] Unhandled event type: {@event.GetType().Name}");
                }
            }
        );
    }
}