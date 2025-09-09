var builder = DistributedApplication.CreateBuilder(args);

// Backing services
var postgres = builder
  .AddPostgres("postgres")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

var cache = builder.AddRedis("cache");

var keycloak = builder
  .AddKeycloak("keycloak")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

if (builder.ExecutionContext.IsRunMode)
{
  postgres.WithPgAdmin();
  cache.WithDataVolume().WithLifetime(ContainerLifetime.Persistent).WithRedisInsight();
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
