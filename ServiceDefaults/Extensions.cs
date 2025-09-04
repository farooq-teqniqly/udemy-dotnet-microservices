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
  public static class Extensions
  {
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
      where TBuilder : IHostApplicationBuilder
    {
      builder
        .Services.AddHealthChecks()
        // Add a default liveness check to ensure app is responsive
        .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

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
      builder.Logging.AddOpenTelemetry(logging =>
      {
        var isDev = builder.Environment.IsDevelopment();
        logging.IncludeFormattedMessage = isDev;
        logging.IncludeScopes = isDev;
      });

      builder
        .Services.AddOpenTelemetry()
        .ConfigureResource(r =>
        {
          r.AddService(
            serviceName: builder.Environment.ApplicationName,
            serviceVersion: typeof(Extensions).Assembly.GetName().Version?.ToString()
          );
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
            .AddAspNetCoreInstrumentation()
            // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
            //.AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation();
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
      app.MapHealthChecks("/health");

      // Only health checks tagged with the "live" tag must pass for app to be considered alive
      app.MapHealthChecks(
        "/alive",
        new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") }
      );

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
