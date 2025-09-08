using Catalog.Models;

namespace Basket.ApiClients
{
  internal sealed class CatalogApiClient
  {
    private readonly HttpClient _httpClient;

    public CatalogApiClient(HttpClient httpClient)
    {
      ArgumentNullException.ThrowIfNull(httpClient);

      _httpClient = httpClient;
    }

    public async Task<Product?> GetProductById(int id, CancellationToken ct = default) =>
      await _httpClient.GetFromJsonAsync<Product?>($"/products/{id}", ct).ConfigureAwait(false);
  }
}
