using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services.TextService;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp;

/// <summary>
/// Dependency injection extensions for the SkiaSharp Papercraft renderer.
/// </summary>
public static class SkiaSharpPapercraftServiceCollectionExtensions
{
    /// <summary>
    /// Adds the SkiaSharp Papercraft renderer and the legacy runtime services it needs.
    /// </summary>
    public static IServiceCollection AddPapercraftSkiaSharpRenderer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddPapercraftCore();
        AddSkiaSharpRuntime(services);
        return services;
    }

    internal static void AddSkiaSharpRuntime(IServiceCollection services)
    {
        services.TryAddSingleton<SkPaintCache>();
        services.TryAddSingleton<ITextService, TextService>();
        services.TryAddSingleton<SkiaSharpDisplayListRenderer>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPapercraftRenderBackend, SkiaSharpRenderBackend>());
    }
}
