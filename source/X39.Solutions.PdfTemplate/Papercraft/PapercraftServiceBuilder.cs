using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate;
using X39.Solutions.PdfTemplate.Abstraction;

namespace X39.Papercraft;

/// <summary>
/// Builder for configuring Papercraft services.
/// </summary>
[PublicAPI]
public sealed class PapercraftServiceBuilder
{
    private readonly PdfTemplateServiceBuilder _compatibilityBuilder;

    internal PapercraftServiceBuilder(IServiceCollection services, PdfTemplateServiceBuilder compatibilityBuilder)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(compatibilityBuilder);
        Services = services;
        _compatibilityBuilder = compatibilityBuilder;
    }

    /// <summary>
    /// The service collection being configured.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Adds a control to the service collection.
    /// </summary>
    public PapercraftServiceBuilder AddControl<
        [MeansImplicitUse(
            ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign)]
        TControl>()
        where TControl : IControl
    {
        _compatibilityBuilder.AddControl<TControl>();
        return this;
    }

    /// <summary>
    /// Removes a control from the service collection.
    /// </summary>
    public PapercraftServiceBuilder RemoveControl<TControl>()
        where TControl : IControl
    {
        _compatibilityBuilder.RemoveControl<TControl>();
        return this;
    }

    /// <summary>
    /// Replaces controls registered for the same XML namespace and name.
    /// </summary>
    public PapercraftServiceBuilder ReplaceControl<
        [MeansImplicitUse(
            ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign)]
        TControl>()
        where TControl : IControl
    {
        _compatibilityBuilder.ReplaceControl<TControl>();
        return this;
    }

    /// <summary>
    /// Removes all control registrations.
    /// </summary>
    public PapercraftServiceBuilder ClearControls()
    {
        _compatibilityBuilder.ClearControls();
        return this;
    }

    /// <summary>
    /// Adds a transformer to the service collection.
    /// </summary>
    public PapercraftServiceBuilder AddTransformer<TTransformer>()
        where TTransformer : class, ITransformer
    {
        _compatibilityBuilder.AddTransformer<TTransformer>();
        return this;
    }

    /// <summary>
    /// Removes a transformer from the service collection.
    /// </summary>
    public PapercraftServiceBuilder RemoveTransformer<TTransformer>()
        where TTransformer : class, ITransformer
    {
        _compatibilityBuilder.RemoveTransformer<TTransformer>();
        return this;
    }

    /// <summary>
    /// Replaces a transformer implementation with another implementation.
    /// </summary>
    public PapercraftServiceBuilder ReplaceTransformer<TExisting, TReplacement>()
        where TExisting : class, ITransformer
        where TReplacement : class, ITransformer
    {
        _compatibilityBuilder.ReplaceTransformer<TExisting, TReplacement>();
        return this;
    }

    /// <summary>
    /// Removes all transformer registrations.
    /// </summary>
    public PapercraftServiceBuilder ClearTransformers()
    {
        _compatibilityBuilder.ClearTransformers();
        return this;
    }

    /// <summary>
    /// Adds a template function to the service collection.
    /// </summary>
    public PapercraftServiceBuilder AddFunction<TFunction>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TFunction : class, IFunction
    {
        _compatibilityBuilder.AddFunction<TFunction>(lifetime);
        return this;
    }
}
