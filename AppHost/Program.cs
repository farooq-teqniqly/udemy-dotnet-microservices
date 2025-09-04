var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Catalog>("catalog");

await builder.Build().RunAsync().ConfigureAwait(false);
