var builder = DistributedApplication.CreateBuilder(args);

// Backing services
var postgres = builder.AddPostgres("postgres").WithLifetime(ContainerLifetime.Persistent);

var cache = builder.AddRedis("cache");

var keycloak = builder.AddKeycloak("keycloak").WithLifetime(ContainerLifetime.Persistent);

if (builder.ExecutionContext.IsRunMode)
{
  // Postgres volumes and volumes in general, do not work with Azure Container Apps (ACA).
  // See https://github.com/dotnet/aspire/issues/6671
  postgres.WithDataVolume().WithPgAdmin();
  cache.WithDataVolume().WithLifetime(ContainerLifetime.Persistent).WithRedisInsight();
  keycloak.WithDataVolume();
}

var catalogDb = postgres.AddDatabase("catalogdb");

// Projects
builder.AddProject<Projects.Catalog>("catalog").WithReference(catalogDb).WaitFor(catalogDb);

builder
  .AddProject<Projects.Basket>("basket")
  .WithReference(cache)
  .WaitFor(cache)
  .WithReference(keycloak)
  .WaitFor(keycloak);

await builder.Build().RunAsync().ConfigureAwait(false);
