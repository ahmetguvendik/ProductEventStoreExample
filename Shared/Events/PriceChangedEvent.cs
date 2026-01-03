namespace Shared.Events;

public class PriceChangedEvent
{
    public string Id { get; set; } = string.Empty;
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal PriceDifference { get; set; }
}

