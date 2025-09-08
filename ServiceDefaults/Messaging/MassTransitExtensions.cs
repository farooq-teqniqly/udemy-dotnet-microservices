using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceDefaults.Messaging
{
  public static class MassTransitExtensions
  {
    public static IServiceCollection AddMassTransitWithAssemblies(
      this IServiceCollection services,
      params Assembly[] assemblies
    )
    {
      ArgumentNullException.ThrowIfNull(services);
      ArgumentNullException.ThrowIfNull(assemblies);

      services.AddMassTransit(massTransitConfig =>
      {
        massTransitConfig.SetKebabCaseEndpointNameFormatter();
        massTransitConfig.SetInMemorySagaRepositoryProvider();
        massTransitConfig.AddConsumers(assemblies);
        massTransitConfig.AddSagaStateMachines(assemblies);
        massTransitConfig.AddSagas(assemblies);
        massTransitConfig.AddActivities(assemblies);

        massTransitConfig.UsingRabbitMq(
          (context, rabbitConfig) =>
          {
            var configuration = context.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("rabbitmq");

            rabbitConfig.Host(connectionString);
            rabbitConfig.ConfigureEndpoints(context);
          }
        );
      });

      return services;
    }
  }
}
