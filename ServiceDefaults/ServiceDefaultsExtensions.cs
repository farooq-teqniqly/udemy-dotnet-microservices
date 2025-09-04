using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ServiceDefaults
{
  // Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
  // This project should be referenced by each service project in your solution.
  // To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
  public static class ServiceDefaultsExtensions
  {
    private const string AliveEndpoint = "/alive";
    private const string HealthEndpoint = "/health";
    private const string LiveTag = "live";

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
      where TBuilder : IHostApplicationBuilder
    {
      builder
        .Services.AddHealthChecks()
        // Add a default liveness check to ensure app is responsive
        .AddCheck("self", () => HealthCheckResult.Healthy(), [LiveTag]);

      return builder;
    }

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
      where TBuilder : IHostApplicationBuilder
    {
      builder.ConfigureOpenTelemetry();

      builder.AddDefaultHealthChecks();

      builder.Services.AddServiceDiscovery();

      builder.Services.ConfigureHttpClientDefaults(http =>
      {
        // Turn on resilience by default
        http.AddStandardResilienceHandler();

        // Turn on service discovery by default
        http.AddServiceDiscovery();
      });

      return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
      where TBuilder : IHostApplicationBuilder
    {
      var serviceName = builder.Environment.ApplicationName;

      var serviceVersion = typeof(ServiceDefaultsExtensions).Assembly.GetName().Version?.ToString();

      builder.Logging.AddOpenTelemetry(logging =>
      {
        var isDev = builder.Environment.IsDevelopment();
        logging.IncludeFormattedMessage = isDev;
        logging.IncludeScopes = isDev;
        logging.ParseStateValues = isDev;

        logging.SetResourceBuilder(
          ResourceBuilder
            .CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        );
      });

      builder
        .Services.AddOpenTelemetry()
        .ConfigureResource(r =>
        {
          r.AddService(serviceName: serviceName, serviceVersion: serviceVersion);
        })
        .WithMetrics(metrics =>
        {
          metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
        })
        .WithTracing(tracing =>
        {
          tracing
            .AddSource(builder.Environment.ApplicationName)
            .AddAspNetCoreInstrumentation(o =>
            {
              o.RecordException = true;
              o.Filter = ctx =>
                ctx.Request.Path.ToString() is not (AliveEndpoint or HealthEndpoint);
            })
            // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
            //.AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation(o =>
            {
              o.FilterHttpRequestMessage = req =>
                req?.RequestUri?.AbsolutePath is not (AliveEndpoint or HealthEndpoint);
            });
        });

      builder.AddOpenTelemetryExporters();

      return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
      ArgumentNullException.ThrowIfNull(app);

      var cfg = app.Services.GetRequiredService<IConfiguration>();
      var exposeInNonDev = cfg.GetValue<bool>("HealthChecks:ExposeEndpoints");

      if (!app.Environment.IsDevelopment() && !exposeInNonDev)
      {
        return app;
      }

      // All health checks must pass for app to be considered ready to accept traffic after starting
      app.MapHealthChecks(HealthEndpoint).DisableHttpMetrics();

      // Only health checks tagged with the "live" tag must pass for app to be considered alive
      app.MapHealthChecks(
          AliveEndpoint,
          new HealthCheckOptions
          {
            Predicate = r => r.Tags.Contains(LiveTag),
            AllowCachingResponses = false,
          }
        )
        .DisableHttpMetrics();

      return app;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
      where TBuilder : IHostApplicationBuilder
    {
      var useOtlpExporter = !string.IsNullOrWhiteSpace(
        builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
      );

      if (!useOtlpExporter)
      {
        return builder;
      }

      // Traces & metrics
      builder.Services.AddOpenTelemetry().UseOtlpExporter();

      // Logs
      builder.Services.Configure<OpenTelemetryLoggerOptions>(o => o.AddOtlpExporter());

      return builder;
    }
  }
}
