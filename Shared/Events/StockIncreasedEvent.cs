namespace Shared.Events;

public class StockIncreasedEvent
{
    public string Id { get; set; } = string.Empty;
    public int OldStock { get; set; }
    public int NewStock { get; set; }
    public int IncreasedAmount { get; set; }
}

