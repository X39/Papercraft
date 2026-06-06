using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Papercraft;
using X39.Papercraft.Rendering.SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Services.PropertyAccessCache;
using X39.Solutions.PdfTemplate.Services.ResourceResolver;
using X39.Solutions.PdfTemplate.Services.TextService;
using X39.Solutions.PdfTemplate.Transformers;

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
        var builder = new PdfTemplateServiceBuilder(services);
        AddInfrastructure(services);
        AddDefaults(builder);
        AddPapercraftRuntime(services);
        AddPapercraftSkiaSharpRenderer(services);
        return builder;
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

    internal static void AddInfrastructure(IServiceCollection services)
    {
        services.TryAddSingleton<SkPaintCache>();
        services.TryAddSingleton<ControlActivationCache>();
        services.TryAddSingleton<ControlRegistry>();
        services.TryAddSingleton<ITextService, TextService>();
        services.TryAddSingleton<IPropertyAccessCache, PropertyAccessCache>();
        services.TryAddScoped<ITemplateData, TemplateData>();
        services.TryAddScoped<IResourceResolver, DefaultResourceResolver>();
        services.TryAddScoped<IControlFactory, ControlFactory>();
        services.TryAddTransient<Generator>();
    }

    internal static void AddDefaults(PdfTemplateServiceBuilder builder)
    {
        builder.AddControl<Controls.BarChart>();
        builder.AddControl<Controls.BorderControl>();
        builder.AddControl<Controls.ChartControl>();
        builder.AddControl<Controls.ChartDataControl>();
        builder.AddControl<Controls.ImageControl>();
        builder.AddControl<Controls.LineChart>();
        builder.AddControl<Controls.LineControl>();
        builder.AddControl<Controls.PageNumberControl>();
        builder.AddControl<Controls.PieChart>();
        builder.AddControl<Controls.TableCellControl>();
        builder.AddControl<Controls.TableControl>();
        builder.AddControl<Controls.TableHeaderControl>();
        builder.AddControl<Controls.TableRowControl>();
        builder.AddControl<Controls.TextControl>();

        builder.AddTransformer<ForTransformer>();
        builder.AddTransformer<IfTransformer>();
        builder.AddTransformer<SwitchTransformer>();
        builder.AddTransformer<ForEachTransformer>();
        builder.AddTransformer<AlternateTransformer>();
        builder.AddTransformer<VariableTransformer>();
    }

    internal static void AddPapercraftRuntime(IServiceCollection services)
    {
        services.TryAddTransient<PapercraftGenerator>();
    }

    internal static void AddPapercraftSkiaSharpRenderer(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPapercraftRenderer, SkiaSharpPapercraftRenderer>());
    }
}
