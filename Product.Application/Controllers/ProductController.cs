using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Product.Application.Data;
using Product.Application.Models;
using Shared.Events;
using Shared.Services.Abstractions;

namespace Product.Application.Controllers;

public class ProductController : Controller
{
    private readonly ProductDbContext _db;
    private readonly IMapper _mapper;
    private readonly IEventStoreService _eventStore;
    

    public ProductController(ProductDbContext db, IMapper mapper, IEventStoreService eventStore)
    {
        _db = db;
        _mapper = mapper;
        _eventStore = eventStore;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _db.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var vm = _mapper.Map<List<ProductViewModel>>(model);

        return View(vm);
    }

    public async Task<IActionResult> Details(string id)
    {
        var product = await Find(id, track: false);
        if (product is null)
        {
            return NotFound("Ürün bulunamadı.");
        }

        var vm = _mapper.Map<ProductViewModel>(product);
        return View(vm);
    }

    public IActionResult Create()
    {
        return View(new ProductCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        var newExternalId = Guid.NewGuid().ToString();

        ProductCreatedEvent pce = new ProductCreatedEvent
        {
            Id = newExternalId,
            Name = product.Name,
            Description = product.Description,
            Stock = product.Stock,
            Price = product.Price,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _eventStore.AppendToStreamAsync("product-stream", new[]
        {
            _eventStore.GenerateEventData(pce)
        });

        TempData["Success"] = "Ürün başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var product = await Find(id, track: false);
        if (product is null)
        {
            return NotFound("Ürün bulunamadı.");
        }

        var vm = _mapper.Map<ProductUpdateViewModel>(product);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, ProductUpdateViewModel updated)
    {
        if (id != updated.Id)
        {
            return BadRequest("Geçersiz ürün bilgisi.");
        }

        if (!ModelState.IsValid)
        {
            return View(updated);
        }

        var existing = await Find(id);
        if (existing is null)
        {
            return NotFound("Ürün bulunamadı.");
        }

        var eventsToPublish = new List<object>();

        // Stok değişikliklerini kontrol et
        if (updated.Stock != existing.Stock)
        {
            if (updated.Stock < existing.Stock)
            {
                // Stok azaldı
                var stockDecreasedEvent = new StockDecreasedEvent
                {
                    Id = existing.Id,
                    OldStock = existing.Stock,
                    NewStock = updated.Stock,
                    DecreasedAmount = existing.Stock - updated.Stock
                };
                eventsToPublish.Add(stockDecreasedEvent);
            }
            else
            {
                // Stok arttı
                var stockIncreasedEvent = new StockIncreasedEvent
                {
                    Id = existing.Id,
                    OldStock = existing.Stock,
                    NewStock = updated.Stock,
                    IncreasedAmount = updated.Stock - existing.Stock
                };
                eventsToPublish.Add(stockIncreasedEvent);
            }
        }

        // Fiyat değişikliğini kontrol et
        if (updated.Price != existing.Price)
        {
            var priceChangedEvent = new PriceChangedEvent
            {
                Id = existing.Id,
                OldPrice = existing.Price,
                NewPrice = updated.Price,
                PriceDifference = updated.Price - existing.Price
            };
            eventsToPublish.Add(priceChangedEvent);
        }

        // Tüm event'leri yayınla
        var eventDataList = eventsToPublish.Select(e => _eventStore.GenerateEventData(e)).ToArray();
        await _eventStore.AppendToStreamAsync("product-stream", eventDataList);
        TempData["Success"] = "Ürün güncelleme isteği alındı ve kuyruğa yazıldı.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
        var product = await Find(id, track: false);
        if (product is null)
        {
            return NotFound("Ürün bulunamadı.");
        }

        var vm = _mapper.Map<ProductViewModel>(product);
        return View(vm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var product = await Find(id);
        if (product is null)
        {
            return NotFound("Ürün bulunamadı.");
        }

        var evt = new ProductDeletedEvent
        {
            Id = product.Id
        };

        await _eventStore.AppendToStreamAsync("product-stream", new[]
        {
            _eventStore.GenerateEventData(evt)
        });

        TempData["Success"] = "Ürün silme isteği alındı ve kuyruğa yazıldı.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Models.Product?> Find(string id, bool track = true)
    {
        var query = track ? _db.Products : _db.Products.AsNoTracking();
        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

}

