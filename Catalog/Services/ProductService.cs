using Catalog.Data;
using Catalog.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Messaging.Events;

namespace Catalog.Services
{
  internal sealed class ProductService
  {
    private readonly IBus _bus;
    private readonly ProductDbContext _dbContext;

    public ProductService(ProductDbContext dbContext, IBus bus)
    {
      ArgumentNullException.ThrowIfNull(dbContext);
      ArgumentNullException.ThrowIfNull(bus);

      _dbContext = dbContext;
      _bus = bus;
    }

    internal async Task CreateProductAsync(Product product, CancellationToken ct = default)
    {
      ArgumentNullException.ThrowIfNull(product);

      _dbContext.Products.Add(product);
      await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    internal async Task DeleteProductAsync(Product deletedProduct, CancellationToken ct = default)
    {
      ArgumentNullException.ThrowIfNull(deletedProduct);

      _dbContext.Products.Remove(deletedProduct);
      await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    internal async Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default) =>
      await _dbContext.Products.FindAsync([id], ct).ConfigureAwait(false);

    internal async Task<IEnumerable<Product>> GetProductsAsync(CancellationToken ct = default) =>
      await _dbContext.Products.ToListAsync(ct).ConfigureAwait(false);

    internal async Task UpdateProductAsync(
      Product updatedProduct,
      Product inputProduct,
      CancellationToken ct = default
    )
    {
      ArgumentNullException.ThrowIfNull(updatedProduct);
      ArgumentNullException.ThrowIfNull(inputProduct);

      if (updatedProduct.Price != inputProduct.Price)
      {
        var @event = new ProductPriceChangeIntegrationEvent
        {
          ProductId = updatedProduct.Id,
          Name = inputProduct.Name,
          Description = inputProduct.Description,
          Price = inputProduct.Price,
          ImageFilename = inputProduct.ImageFilename,
        };

        await _bus.Publish(@event, ct).ConfigureAwait(false);
      }

      updatedProduct.Name = inputProduct.Name;
      updatedProduct.Description = inputProduct.Description;
      updatedProduct.ImageFilename = inputProduct.ImageFilename;

      await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
  }
}
