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

    public async Task<IActionResult> Details(int id)
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

        ProductCreatedEvent pce = new ProductCreatedEvent
        {
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

    public async Task<IActionResult> Edit(int id)
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
    public async Task<IActionResult> Edit(int id, ProductUpdateViewModel updated)
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

        var evt = new ProductUpdatedEvent
        {
            Id = updated.Id,
            Name = updated.Name,
            Description = updated.Description,
            Price = updated.Price,
            Stock = updated.Stock,
            IsActive = true
        };

        await _eventStore.AppendToStreamAsync("product-stream", new[]
        {
            _eventStore.GenerateEventData(evt)
        });

        TempData["Success"] = "Ürün güncelleme isteği alındı ve kuyruğa yazıldı.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
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
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await Find(id);
        if (product is null)
        {
            return NotFound("Ürün bulunamadı.");
        }

        var evt = new ProductDeletedEvent
        {
            Id = id
        };

        await _eventStore.AppendToStreamAsync("product-stream", new[]
        {
            _eventStore.GenerateEventData(evt)
        });

        TempData["Success"] = "Ürün silme isteği alındı ve kuyruğa yazıldı.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Models.Product?> Find(int id, bool track = true)
    {
        var query = track ? _db.Products : _db.Products.AsNoTracking();
        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

}

