namespace Shared.Events;

public class ProductDeletedEvent
{
    public string Id { get; set; } = string.Empty; // External unique id (GUID)
}

