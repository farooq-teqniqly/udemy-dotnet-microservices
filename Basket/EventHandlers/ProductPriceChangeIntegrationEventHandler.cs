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
      ArgumentNullException.ThrowIfNull(context);

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
