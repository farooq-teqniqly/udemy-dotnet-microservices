﻿namespace ServiceDefaults.Messaging.Events;

public sealed record ProductPriceChangeIntegrationEvent : IntegrationEvent
{
  public required int ProductId { get; init; }
  public required string Name { get; init; }
  public required string Description { get; init; }
  public required decimal Price { get; init; }
  public required string ImageFilename { get; init; }
}
