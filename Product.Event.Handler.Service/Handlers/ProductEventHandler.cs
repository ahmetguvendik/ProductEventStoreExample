using Microsoft.EntityFrameworkCore;
using Product.Event.Handler.Service.Data;
using ProductEntity = Product.Event.Handler.Service.Models.Product;
using Shared.Events;

namespace Product.Event.Handler.Service.Handlers;

public class ProductEventHandler
{
    private readonly IDbContextFactory<ProductDbContext> _dbFactory;

    public ProductEventHandler(IDbContextFactory<ProductDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task Handle(ProductCreatedEvent evt, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        bool existsById = await db.Products.AnyAsync(p => p.Id == evt.Id, ct);
        if (existsById)
        {
            return; // Aynı ID ile kayıt zaten var
        }

        var entity = new ProductEntity
        {
            Id = evt.Id,
            Name = evt.Name,
            Description = evt.Description,
            Stock = evt.Stock,
            Price = evt.Price,
            CreatedAt = evt.CreatedAt == default ? DateTime.UtcNow : evt.CreatedAt,
            IsActive = evt.IsActive
        };

        await db.Products.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task Handle(ProductDeletedEvent evt, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var existing = await db.Products.FirstOrDefaultAsync(p => p.Id == evt.Id, ct);
        if (existing is null)
        {
            return;
        }

        db.Products.Remove(existing);
        await db.SaveChangesAsync(ct);
    }

    public async Task Handle(StockDecreasedEvent evt, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var existing = await db.Products.FirstOrDefaultAsync(p => p.Id == evt.Id, ct);
        if (existing is null)
        {
            return;
        }

        // Stoku güncelle
        existing.Stock = evt.NewStock;
        await db.SaveChangesAsync(ct);

        Console.WriteLine($"[STOCK DECREASED] Product Id={evt.Id}, OldStock={evt.OldStock}, NewStock={evt.NewStock}, Decreased={evt.DecreasedAmount}");
    }

    public async Task Handle(StockIncreasedEvent evt, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var existing = await db.Products.FirstOrDefaultAsync(p => p.Id == evt.Id, ct);
        if (existing is null)
        {
            return;
        }

        // Stoku güncelle
        existing.Stock = evt.NewStock;
        await db.SaveChangesAsync(ct);

        Console.WriteLine($"[STOCK INCREASED] Product Id={evt.Id}, OldStock={evt.OldStock}, NewStock={evt.NewStock}, Increased={evt.IncreasedAmount}");
    }

    public async Task Handle(PriceChangedEvent evt, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var existing = await db.Products.FirstOrDefaultAsync(p => p.Id == evt.Id, ct);
        if (existing is null)
        {
            return;
        }

        // Fiyatı güncelle
        existing.Price = evt.NewPrice;
        await db.SaveChangesAsync(ct);

        Console.WriteLine($"[PRICE CHANGED] Product Id={evt.Id}, OldPrice={evt.OldPrice}, NewPrice={evt.NewPrice}, Difference={evt.PriceDifference}");
    }
}

