using Catalog.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Data
{
  internal sealed class ProductDbContext : DbContext
  {
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
      : base(options) { }

    public DbSet<Product> Products => Set<Product>();
  }
}
