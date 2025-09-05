using Catalog.Data;
using Catalog.Endpoints;
using Catalog.Services;
using ServiceDefaults;

namespace Catalog;

internal static class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.AddServiceDefaults();

    builder.AddNpgsqlDbContext<ProductDbContext>(connectionName: "catalogdb");
    builder.Services.AddScoped<ProductService>();

    builder.Services.AddAuthorization();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.MapDefaultEndpoints();

    app.MapProductEndpoints();

    if (app.Environment.IsDevelopment())
    {
      var forceMigration = app.Configuration.GetValue<bool>("Database:ForceMigration");

      await app.UseMigrationAsync(forceMigration, app.Lifetime.ApplicationStopping)
        .ConfigureAwait(false);
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.MapOpenApi();
    }

    if (!app.Environment.IsDevelopment())
    {
      app.UseHsts();
      app.UseHttpsRedirection();
    }

    app.UseAuthorization();

    await app.RunAsync().ConfigureAwait(false);
  }
}
