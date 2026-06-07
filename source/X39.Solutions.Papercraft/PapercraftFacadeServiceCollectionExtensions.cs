using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Dependency injection extensions for the default Papercraft facade.
/// </summary>
public static class PapercraftFacadeServiceCollectionExtensions
{
    /// <summary>
    /// Adds Papercraft with the default SkiaSharp renderer.
    /// </summary>
    public static PapercraftServiceBuilder AddPapercraft(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddPapercraftSkiaSharpRenderer();
        return new PapercraftServiceBuilder(services);
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
