using System.ComponentModel.DataAnnotations;

namespace Product.Application.Models;

public class ProductUpdateViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olmalıdır.")]
    public string? Description { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Fiyat 0 ile 1.000.000 arasında olmalıdır.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Range(0, 100_000, ErrorMessage = "Stok 0 ile 100.000 arasında olmalıdır.")]
    public int Stock { get; set; }
}

