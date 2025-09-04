var builder = DistributedApplication.CreateBuilder(args);

// Backing services
var postgres = builder
  .AddPostgres("postgres")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

if (!builder.ExecutionContext.IsPublishMode)
{
  postgres.WithPgAdmin();
}

var catalogDb = postgres.AddDatabase("catalogdb");

// Projects
builder.AddProject<Projects.Catalog>("catalog").WithReference(catalogDb).WaitFor(catalogDb);

await builder.Build().RunAsync().ConfigureAwait(false);
