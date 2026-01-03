namespace Shared.Events;

public class StockDecreasedEvent
{
    public string Id { get; set; } = string.Empty;
    public int OldStock { get; set; }
    public int NewStock { get; set; }
    public int DecreasedAmount { get; set; }
}

