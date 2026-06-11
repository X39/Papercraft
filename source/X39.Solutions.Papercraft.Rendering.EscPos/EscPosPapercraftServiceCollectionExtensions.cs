using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Solutions.Papercraft;

namespace X39.Solutions.Papercraft.Rendering.EscPos;

/// <summary>
/// Dependency injection extensions for the ESC/POS Papercraft renderer.
/// </summary>
public static class EscPosPapercraftServiceCollectionExtensions
{
    /// <summary>
    /// Adds the ESC/POS Papercraft renderer.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">Optional ESC/POS render options.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddPapercraftEscPosRenderer(
        this IServiceCollection services,
        EscPosRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddPapercraftCore();

        if (options is null)
            services.TryAddSingleton(EscPosRenderOptions.Default);
        else
            services.AddSingleton(options);

        services.TryAddEnumerable(ServiceDescriptor.Transient<IPapercraftRenderBackend, EscPosRenderBackend>());
        return services;
    }
}
