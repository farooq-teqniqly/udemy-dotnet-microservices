using System.Text.Json;
using Basket.ApiClients;
using Basket.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.Services
{
  internal sealed class ShoppingBasketService
  {
    private readonly IDistributedCache _cache;
    private readonly CatalogApiClient _catalogApiClient;

    private static readonly DistributedCacheEntryOptions options = new()
    {
      SlidingExpiration = TimeSpan.FromHours(1),
    };

    private static string GetKey(string username) => $"basket:{username.Trim().ToUpperInvariant()}";

    public ShoppingBasketService(IDistributedCache cache, CatalogApiClient catalogApiClient)
    {
      ArgumentNullException.ThrowIfNull(cache);
      ArgumentNullException.ThrowIfNull(catalogApiClient);

      _cache = cache;
      _catalogApiClient = catalogApiClient;
    }

    internal async Task DeleteBasketAsync(string username, CancellationToken ct = default)
    {
      ArgumentException.ThrowIfNullOrEmpty(username);

      await _cache.RemoveAsync(GetKey(username), ct).ConfigureAwait(false);
    }

    internal async Task<ShoppingBasket?> GetBasketAsync(
      string username,
      CancellationToken ct = default
    )
    {
      ArgumentException.ThrowIfNullOrEmpty(username);

      var json = await _cache.GetStringAsync(GetKey(username), ct).ConfigureAwait(false);
      if (string.IsNullOrEmpty(json))
        return null;
      try
      {
        return JsonSerializer.Deserialize<ShoppingBasket>(json);
      }
      catch (JsonException)
      {
        await _cache.RemoveAsync(GetKey(username), ct).ConfigureAwait(false);
        return null;
      }
    }

    internal async Task UpdateBasketAsync(
      ShoppingBasket shoppingBasket,
      CancellationToken ct = default
    )
    {
      ArgumentNullException.ThrowIfNull(shoppingBasket);
      ArgumentException.ThrowIfNullOrEmpty(shoppingBasket.Username);

      foreach (var item in shoppingBasket.Items)
      {
        var product = await _catalogApiClient
          .GetProductById(item.ProductId, ct)
          .ConfigureAwait(false);

        if (product is null)
        {
          throw new InvalidOperationException(
            $"Product with id {item.ProductId} was added to the basket, but was not found in the catalog."
          );
        }

        item.Price = product.Price;
        item.Name = product.Name;
      }

      var key = GetKey(shoppingBasket.Username);

      await _cache
        .SetStringAsync(key, JsonSerializer.Serialize(shoppingBasket), options, ct)
        .ConfigureAwait(false);
    }
  }
}
