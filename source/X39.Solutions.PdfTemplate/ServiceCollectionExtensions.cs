using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;
using X39.Solutions.Papercraft.Services;
using X39.Solutions.Papercraft.Services.PropertyAccessCache;
using X39.Solutions.Papercraft.Services.ResourceResolver;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Contains extension methods for the service collection.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required for the generator to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>A builder for configuring PDF template services.</returns>
    /// <remarks>
    /// This method adds the following services:
    /// <list type="bullet">
    ///     <item><see cref="SkPaintCache"/></item>
    ///     <item><see cref="ControlActivationCache"/></item>
    ///     <item><see cref="ITextService"/></item>
    ///     <item><see cref="IPropertyAccessCache"/></item>
    ///     <item><see cref="ITemplateData"/></item>
    ///     <item><see cref="IResourceResolver"/></item>
    ///     <item><see cref="ControlRegistry"/></item>
    ///     <item><see cref="IControlFactory"/></item>
    ///     <item><see cref="Generator"/></item>
    /// </list>
    ///
    /// If you want to use your own implementation of <see cref="IResourceResolver"/>, you can add it after this method.
    /// See <a href="https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-registration-methods">MSDN Page</a>
    /// for more information about how to work with dependency injection in .NET.
    /// </remarks>
    public static PdfTemplateServiceBuilder AddPdfTemplateService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddPapercraftSkiaSharpRenderer();
        services.TryAddTransient<Generator>();
        return new PdfTemplateServiceBuilder(services);
    }

    /// <summary>
    /// Adds and configures the services required for the generator to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configure">The builder configuration callback.</param>
    /// <returns>A builder for configuring PDF template services.</returns>
    public static PdfTemplateServiceBuilder AddPdfTemplateService(
        this IServiceCollection services,
        Action<PdfTemplateServiceBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = services.AddPdfTemplateService();
        configure(builder);
        return builder;
    }
}
