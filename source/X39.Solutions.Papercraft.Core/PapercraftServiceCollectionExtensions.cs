using Microsoft.Extensions.DependencyInjection;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Dependency injection extensions for renderer-neutral Papercraft services.
/// </summary>
public static class PapercraftServiceCollectionExtensions
{
    /// <summary>
    /// Adds Papercraft core parsing, control, transformer, and generator services without selecting a renderer.
    /// </summary>
    public static PapercraftServiceBuilder AddPapercraftCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        ServiceRegistrationOperations.AddCoreServices(services);
        ServiceRegistrationOperations.AddDefaultControls(services);
        ServiceRegistrationOperations.AddDefaultTransformers(services);
        return new PapercraftServiceBuilder(services);
    }
}
