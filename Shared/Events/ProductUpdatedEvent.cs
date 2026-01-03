namespace Shared.Events;

public class ProductUpdatedEvent
{
    public string Id { get; set; } = string.Empty; // External unique id (GUID)
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
}

