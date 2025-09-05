using Catalog.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Data;

internal static class DataSeeder
{
  internal static Product[] Products =>
    [
      new()
      {
        Id = 1,
        Name = "Laptop",
        Description = "High performance laptop",
        Price = 999.99m,
        ImageFilename = "laptop.jpg",
      },
      new()
      {
        Id = 2,
        Name = "Smartphone",
        Description = "Latest model smartphone",
        Price = 799.99m,
        ImageFilename = "smartphone.jpg",
      },
      new()
      {
        Id = 3,
        Name = "Headphones",
        Description = "Noise-cancelling headphones",
        Price = 199.99m,
        ImageFilename = "headphones.jpg",
      },
      new()
      {
        Id = 4,
        Name = "Smartwatch",
        Description = "Feature-packed smartwatch",
        Price = 249.99m,
        ImageFilename = "smartwatch.jpg",
      },
      new()
      {
        Id = 5,
        Name = "Tablet",
        Description = "Lightweight and powerful tablet",
        Price = 499.99m,
        ImageFilename = "tablet.jpg",
      },
      new()
      {
        Id = 6,
        Name = "Camera",
        Description = "High resolution digital camera",
        Price = 599.99m,
        ImageFilename = "camera.jpg",
      },
      new()
      {
        Id = 7,
        Name = "Monitor",
        Description = "4K Ultra HD monitor",
        Price = 299.99m,
        ImageFilename = "monitor.jpg",
      },
      new()
      {
        Id = 8,
        Name = "Keyboard",
        Description = "Mechanical keyboard",
        Price = 89.99m,
        ImageFilename = "keyboard.jpg",
      },
      new()
      {
        Id = 9,
        Name = "Mouse",
        Description = "Wireless ergonomic mouse",
        Price = 49.99m,
        ImageFilename = "mouse.jpg",
      },
      new()
      {
        Id = 10,
        Name = "Printer",
        Description = "All-in-one printer",
        Price = 149.99m,
        ImageFilename = "printer.jpg",
      },
    ];

  internal static async Task SeedAsync(
    ProductDbContext dbContext,
    bool forceMigration = false,
    CancellationToken ct = default
  )
  {
    ArgumentNullException.ThrowIfNull(dbContext);

    if (forceMigration)
    {
      await dbContext
        .Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""Products"" RESTART IDENTITY CASCADE", ct)
        .ConfigureAwait(false);
    }
    else if (await dbContext.Products.AsNoTracking().AnyAsync(ct).ConfigureAwait(false))
    {
      return;
    }

    await dbContext.AddRangeAsync(Products, ct).ConfigureAwait(false);
  }
}
