using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Papercraft.Rendering.SkiaSharp;
using X39.Solutions.PdfTemplate;

namespace X39.Papercraft;

/// <summary>
/// Dependency injection extensions for Papercraft.
/// </summary>
[PublicAPI]
public static class PapercraftServiceCollectionExtensions
{
    /// <summary>
    /// Adds Papercraft with the default SkiaSharp renderer.
    /// </summary>
    public static PapercraftServiceBuilder AddPapercraft(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var builder = services.AddPapercraftCore();
        services.AddPapercraftSkiaSharpRenderer();
        return builder;
    }

    /// <summary>
    /// Adds Papercraft core parsing, control, transformer, and generator services without selecting a renderer.
    /// </summary>
    public static PapercraftServiceBuilder AddPapercraftCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var compatibilityBuilder = new PdfTemplateServiceBuilder(services);
        ServiceCollectionExtensions.AddInfrastructure(services);
        ServiceCollectionExtensions.AddDefaults(compatibilityBuilder);
        return new PapercraftServiceBuilder(services, compatibilityBuilder);
    }

    /// <summary>
    /// Adds the SkiaSharp Papercraft renderer.
    /// </summary>
    public static PapercraftServiceBuilder AddPapercraftSkiaSharpRenderer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var builder = services.AddPapercraftCore();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPapercraftRenderer, SkiaSharpPapercraftRenderer>());
        ServiceCollectionExtensions.AddPapercraftRuntime(services);
        return builder;
    }

    /// <summary>
    /// Adds and configures Papercraft with the default SkiaSharp renderer.
    /// </summary>
    public static PapercraftServiceBuilder AddPapercraft(
        this IServiceCollection services,
        Action<PapercraftServiceBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = services.AddPapercraft();
        configure(builder);
        return builder;
    }
}
