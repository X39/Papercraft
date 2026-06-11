using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Rendering.Svg.Services;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Rendering.Svg;

/// <summary>
/// Dependency injection extensions for the SVG Papercraft renderer.
/// </summary>
public static class SvgPapercraftServiceCollectionExtensions
{
    /// <summary>
    /// Adds the SVG Papercraft renderer.
    /// </summary>
    public static IServiceCollection AddPapercraftSvgRenderer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddPapercraftCore();
        services.TryAddSingleton<ITextService, SvgTextService>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPapercraftRenderBackend, SvgRenderBackend>());
        return services;
    }
}
