using Catalog.Data;
using Catalog.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Services
{
  internal sealed class ProductService
  {
    private readonly ProductDbContext _dbContext;

    public ProductService(ProductDbContext dbContext)
    {
      _dbContext = dbContext;
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

      updatedProduct.Name = inputProduct.Name;
      updatedProduct.Description = inputProduct.Description;
      updatedProduct.ImageFilename = inputProduct.ImageFilename;
      updatedProduct.Price = inputProduct.Price;

      await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
  }
}
