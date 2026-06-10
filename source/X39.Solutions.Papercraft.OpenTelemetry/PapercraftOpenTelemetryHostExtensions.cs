using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace X39.Solutions.Papercraft.OpenTelemetry;

/// <summary>
/// OpenTelemetry registration helpers for Papercraft renderer tracing.
/// </summary>
public static class PapercraftOpenTelemetryHostExtensions
{
    /// <summary>
    /// Registers Papercraft's activity source with OpenTelemetry tracing.
    /// </summary>
    public static IServiceCollection AddPapercraftOpenTelemetry(this IServiceCollection services)
        => AddPapercraftOpenTelemetry(services, null);

    /// <summary>
    /// Registers Papercraft's activity source with OpenTelemetry tracing and allows callers to add exporters or processors.
    /// </summary>
    public static IServiceCollection AddPapercraftOpenTelemetry(
        this IServiceCollection services,
        Action<TracerProviderBuilder>? configureTracing)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOpenTelemetry()
                .WithTracing(
                    (tracing) =>
                    {
                        tracing.AddSource(PapercraftInstrumentation.ActivitySourceName);
                        configureTracing?.Invoke(tracing);
                    });
        return services;
    }

    /// <summary>
    /// Registers Papercraft's activity source with OpenTelemetry tracing on an application builder.
    /// </summary>
    public static IHostApplicationBuilder AddPapercraftOpenTelemetry(this IHostApplicationBuilder builder)
        => AddPapercraftOpenTelemetry(builder, null);

    /// <summary>
    /// Registers Papercraft's activity source with OpenTelemetry tracing on an application builder.
    /// </summary>
    public static IHostApplicationBuilder AddPapercraftOpenTelemetry(
        this IHostApplicationBuilder builder,
        Action<TracerProviderBuilder>? configureTracing)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddPapercraftOpenTelemetry(configureTracing);
        return builder;
    }

    /// <summary>
    /// Registers Papercraft's activity source with OpenTelemetry tracing on a host builder.
    /// </summary>
    public static IHostBuilder AddPapercraftOpenTelemetry(this IHostBuilder hostBuilder)
        => AddPapercraftOpenTelemetry(hostBuilder, null);

    /// <summary>
    /// Registers Papercraft's activity source with OpenTelemetry tracing on a host builder.
    /// </summary>
    public static IHostBuilder AddPapercraftOpenTelemetry(
        this IHostBuilder hostBuilder,
        Action<TracerProviderBuilder>? configureTracing)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);
        return hostBuilder.ConfigureServices(
            (_, services) => services.AddPapercraftOpenTelemetry(configureTracing));
    }
}
