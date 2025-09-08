namespace ServiceDefaults.Messaging.Events;

public sealed record ProductPriceChangeIntegrationEvent : IntegrationEvent
{
  public int ProductId { get; set; }
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public decimal Price { get; set; }
  public string ImageFilename { get; set; } = null!;
}
