using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Services.PropertyAccessCache;
using X39.Solutions.PdfTemplate.Services.ResourceResolver;
using X39.Solutions.PdfTemplate.Services.TextService;

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
    /// <remarks>
    /// This method adds the following services:
    /// <list type="bullet">
    ///     <item><see cref="SkPaintCache"/></item>
    ///     <item><see cref="ControlExpressionCache"/></item>
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
    public static void AddPdfTemplateServices(this IServiceCollection services)
    {
        services.TryAddSingleton<SkPaintCache>();
        services.TryAddSingleton<ControlExpressionCache>();
        services.TryAddSingleton<ControlRegistry>();
        services.TryAddSingleton<ITextService, TextService>();
        services.TryAddSingleton<IPropertyAccessCache, PropertyAccessCache>();
        services.TryAddScoped<ITemplateData, TemplateData>();
        services.TryAddScoped<IResourceResolver, DefaultResourceResolver>();
        services.TryAddScoped<IControlFactory, ControlFactory>();
        services.TryAddTransient<Generator>();
    }

    /// <summary>
    /// Adds a control to the service collection, making it available for use in templates.
    /// </summary>
    /// <param name="services">The service collection to add the control to.</param>
    /// <typeparam name="TControl">The type of the control to add.</typeparam>
    /// <returns>The <paramref name="services"/> passed to allow chaining.</returns>
    public static IServiceCollection AddPdfTemplateControl<
        [MeansImplicitUse(
            ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign)]
        TControl>(this IServiceCollection services)
        where TControl : IControl
    {
        services.AddSingleton(ControlRegistry.CreateRegistration<TControl>());
        return services;
    }

    /// <summary>
    /// Adds a transformer to the service collection, making it available for use in templates.
    /// </summary>
    /// <param name="services">The service collection to add the transformer to.</param>
    /// <typeparam name="TTransformer">The type of the transformer to add.</typeparam>
    /// <returns>The <paramref name="services"/> passed to allow chaining.</returns>
    public static IServiceCollection AddPdfTemplateTransformer<TTransformer>(this IServiceCollection services)
        where TTransformer : class, ITransformer
    {
        services.AddTransient<ITransformer, TTransformer>();
        return services;
    }
}
