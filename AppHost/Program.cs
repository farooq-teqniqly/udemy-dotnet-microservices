var builder = DistributedApplication.CreateBuilder(args);

// Backing services
var postgres = builder
  .AddPostgres("postgres")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

var cache = builder.AddRedis("cache");

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

if (builder.ExecutionContext.IsRunMode)
{
  postgres.WithPgAdmin();
  cache.WithDataVolume().WithLifetime(ContainerLifetime.Persistent).WithRedisInsight();
  rabbitmq.WithManagementPlugin().WithDataVolume().WithLifetime(ContainerLifetime.Persistent);
}

var catalogDb = postgres.AddDatabase("catalogdb");

// Projects
builder
  .AddProject<Projects.Catalog>("catalog")
  .WithReference(catalogDb)
  .WaitFor(catalogDb)
  .WithReference(rabbitmq)
  .WaitFor(rabbitmq);

builder
  .AddProject<Projects.Basket>("basket")
  .WithReference(cache)
  .WaitFor(cache)
  .WithReference(rabbitmq)
  .WaitFor(rabbitmq);

await builder.Build().RunAsync().ConfigureAwait(false);
