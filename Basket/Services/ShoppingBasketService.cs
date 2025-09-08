using System.Text.Json;
using Basket.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.Services
{
  public sealed class ShoppingBasketService
  {
    private static readonly DistributedCacheEntryOptions options = new()
    {
      SlidingExpiration = TimeSpan.FromHours(1),
    };
    private readonly IDistributedCache _cache;

    public ShoppingBasketService(IDistributedCache cache)
    {
      ArgumentNullException.ThrowIfNull(cache);
      _cache = cache;
    }

    public async Task UpdateBasketItemPrices(
      int productId,
      decimal price,
      CancellationToken ct = default
    )
    {
      const string username = "farooq";
      var basket = await GetBasketAsync(username, ct).ConfigureAwait(false);
      var item = basket?.Items.FirstOrDefault(i => i.ProductId == productId);

      if (item is null)
      {
        return;
      }

      item.Price = price;

      await _cache
        .SetStringAsync(GetKey(username), JsonSerializer.Serialize(basket), ct)
        .ConfigureAwait(false);
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

      var key = GetKey(shoppingBasket.Username);
      await _cache
        .SetStringAsync(key, JsonSerializer.Serialize(shoppingBasket), options, ct)
        .ConfigureAwait(false);
    }

    private static string GetKey(string username) => $"basket:{username.Trim().ToUpperInvariant()}";
  }
}
