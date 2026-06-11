using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Rendering.PdfSharp.Services;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp;

/// <summary>
/// Dependency injection extensions for the PDFsharp Papercraft renderer.
/// </summary>
public static class PdfSharpPapercraftServiceCollectionExtensions
{
    /// <summary>
    /// Adds the PDFsharp Papercraft renderer.
    /// </summary>
    public static IServiceCollection AddPapercraftPdfSharpRenderer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddPapercraftCore();
        services.TryAddSingleton<ITextService, PdfSharpTextService>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPapercraftRenderBackend, PdfSharpRenderBackend>());
        return services;
    }
}
