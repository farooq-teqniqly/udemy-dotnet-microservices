using System.Text.Json;
using Basket.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.Services
{
  internal sealed class ShoppingBasketService
  {
    private readonly IDistributedCache _cache;

    private static readonly DistributedCacheEntryOptions options = new()
    {
      SlidingExpiration = TimeSpan.FromHours(1),
    };

    private static string GetKey(string username) => $"basket:{username.Trim().ToUpperInvariant()}";

    public ShoppingBasketService(IDistributedCache cache)
    {
      ArgumentNullException.ThrowIfNull(cache);
      _cache = cache;
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
      if (string.IsNullOrEmpty(json)) return null;
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

      await _cache
        .SetStringAsync(
          GetKey(shoppingBasket.Username),
          JsonSerializer.Serialize(shoppingBasket),
          options,
          ct
        )
        .ConfigureAwait(false);
    }
  }
}
