using Basket.Endpoints;
using Basket.Services;
using ServiceDefaults;

namespace Basket;

internal static class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.AddServiceDefaults();

    builder.AddRedisDistributedCache(connectionName: "cache");
    builder.Services.AddScoped<ShoppingBasketService>();

    builder.Services.AddAuthorization();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.MapOpenApi();
    }

    if (!app.Environment.IsDevelopment())
    {
      app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.MapDefaultEndpoints();
    app.MapShoppingBasketEndpoints();

    await app.RunAsync().ConfigureAwait(false);
  }
}
