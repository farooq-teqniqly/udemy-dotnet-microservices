using Basket.Services;
using MassTransit;
using ServiceDefaults.Messaging.Events;

namespace Basket.EventHandlers
{
  public sealed class ProductPriceChangeIntegrationEventHandler
    : IConsumer<ProductPriceChangeIntegrationEvent>
  {
    private readonly ShoppingBasketService _service;

    public ProductPriceChangeIntegrationEventHandler(ShoppingBasketService service)
    {
      ArgumentNullException.ThrowIfNull(service);
      _service = service;
    }

    public async Task Consume(ConsumeContext<ProductPriceChangeIntegrationEvent> context)
    {
      // context is non-null in MassTransit; optionally guard message fields instead.

      await _service
        .UpdateBasketItemPrices(
          context.Message.ProductId,
          context.Message.Price,
          context.CancellationToken
        )
        .ConfigureAwait(false);
    }
  }
}
