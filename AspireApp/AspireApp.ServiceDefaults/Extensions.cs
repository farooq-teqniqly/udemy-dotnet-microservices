using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AspireApp.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
public static class Extensions
{
  private const string HealthEndpointPath = "/health";
  private const string AlivenessEndpointPath = "/alive";

  public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
    where TBuilder : IHostApplicationBuilder
  {
    builder.ConfigureOpenTelemetry();

    builder.AddDefaultHealthChecks();

    builder.Services.AddServiceDiscovery();

    builder.Services.ConfigureHttpClientDefaults(http =>
    {
      // Resilience for all HttpClient instances
      http.AddStandardResilienceHandler();

      // Service discovery for "http://catalog" style logical URLs
      http.AddServiceDiscovery();
    });

    return builder;
  }

  public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
    where TBuilder : IHostApplicationBuilder
  {
    // 1) W3C propagation across hops
    Sdk.SetDefaultTextMapPropagator(
      new CompositeTextMapPropagator([new TraceContextPropagator(), new BaggagePropagator()])
    );

    // 2) Put TraceId/SpanId/Baggage into logs automatically
    builder.Services.Configure<LoggerFactoryOptions>(o =>
      o.ActivityTrackingOptions =
        ActivityTrackingOptions.TraceId
        | ActivityTrackingOptions.SpanId
        | ActivityTrackingOptions.ParentId
        | ActivityTrackingOptions.Baggage
    );

    // Optional: export logs via OTLP too
    builder.Logging.AddOpenTelemetry(o =>
    {
      o.IncludeFormattedMessage = true;
      o.IncludeScopes = true;
    });

    // 3) Metrics + Tracing (HTTP in/out, DB)
    builder
      .Services.AddOpenTelemetry()
      .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
      .WithMetrics(m =>
        m.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation()
      )
      .WithTracing(t =>
        t.AddAspNetCoreInstrumentation(o =>
          {
            o.RecordException = true;
            // Keep traces clean: skip health endpoints
            o.Filter = ctx =>
              !ctx.Request.Path.StartsWithSegments(
                HealthEndpointPath,
                StringComparison.InvariantCultureIgnoreCase
              )
              && !ctx.Request.Path.StartsWithSegments(
                AlivenessEndpointPath,
                StringComparison.InvariantCultureIgnoreCase
              );
          })
          .AddHttpClientInstrumentation(o =>
          {
            o.RecordException = true; // also injects traceparent/baggage automatically
          })
          // Database instrumentation (child spans under current request)
          .AddEntityFrameworkCoreInstrumentation(ef =>
          {
            // Useful in dev; disable in prod if too verbose
            ef.SetDbStatementForText = true;
          })
          .AddNpgsql() // If you use Npgsql directly; harmless otherwise
      );

    // 4) Exporters (enabled when endpoint is present)
    var hasOtlp = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

    if (hasOtlp)
    {
      builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }

    return builder;
  }

  public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
    where TBuilder : IHostApplicationBuilder
  {
    builder
      .Services.AddHealthChecks()
      .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

    return builder;
  }

  public static WebApplication MapDefaultEndpoints(this WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);

    // Enrich the current Activity (inbound server span) + logging scope for every request.
    app.Use(
      async (ctx, next) =>
      {
        var activity = Activity.Current;
        if (activity is not null)
        {
          activity.SetTag("enduser.id", ctx.User?.Identity?.Name);
          activity.SetTag("http.client_ip", ctx.Connection.RemoteIpAddress?.ToString());
        }

        // Ensure trace/span IDs are present in structured logs even if a formatter ignores ActivityTrackingOptions
        var loggerFactory = ctx.RequestServices.GetRequiredService<ILoggerFactory>();
        using (
          loggerFactory
            .CreateLogger("Tracing")
            .BeginScope(
              new Dictionary<string, object?>
              {
                ["traceId"] = activity?.TraceId.ToString(),
                ["spanId"] = activity?.SpanId.ToString(),
              }
            )
        )
        {
          await next().ConfigureAwait(false);
        }

        // Add response details after pipeline executes
        activity?.SetTag("http.response_content_length", ctx.Response.ContentLength);
      }
    );

    // Health endpoints (enable carefully outside dev)
    if (app.Environment.IsDevelopment())
    {
      app.MapHealthChecks(HealthEndpointPath);
      app.MapHealthChecks(
        AlivenessEndpointPath,
        new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") }
      );
    }

    return app;
  }
}
