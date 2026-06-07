using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Abstraction;
namespace X39.Solutions.Papercraft;

/// <summary>
/// Builder for configuring Papercraft services.
/// </summary>
public sealed class PapercraftServiceBuilder
{
    internal PapercraftServiceBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Services = services;
    }

    /// <summary>
    /// The service collection being configured.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Adds a control to the service collection.
    /// </summary>
    public PapercraftServiceBuilder AddControl<TControl>()
        where TControl : IControl
    {
        ServiceRegistrationOperations.AddControl<TControl>(Services);
        return this;
    }

    /// <summary>
    /// Removes a control from the service collection.
    /// </summary>
    public PapercraftServiceBuilder RemoveControl<TControl>()
        where TControl : IControl
    {
        ServiceRegistrationOperations.RemoveControl<TControl>(Services);
        return this;
    }

    /// <summary>
    /// Replaces controls registered for the same XML namespace and name.
    /// </summary>
    public PapercraftServiceBuilder ReplaceControl<TControl>()
        where TControl : IControl
    {
        ServiceRegistrationOperations.ReplaceControl<TControl>(Services);
        return this;
    }

    /// <summary>
    /// Removes all control registrations.
    /// </summary>
    public PapercraftServiceBuilder ClearControls()
    {
        ServiceRegistrationOperations.ClearControls(Services);
        return this;
    }

    /// <summary>
    /// Adds a transformer to the service collection.
    /// </summary>
    public PapercraftServiceBuilder AddTransformer<TTransformer>()
        where TTransformer : class, ITransformer
    {
        ServiceRegistrationOperations.AddTransformer<TTransformer>(Services);
        return this;
    }

    /// <summary>
    /// Removes a transformer from the service collection.
    /// </summary>
    public PapercraftServiceBuilder RemoveTransformer<TTransformer>()
        where TTransformer : class, ITransformer
    {
        ServiceRegistrationOperations.RemoveTransformer<TTransformer>(Services);
        return this;
    }

    /// <summary>
    /// Replaces a transformer implementation with another implementation.
    /// </summary>
    public PapercraftServiceBuilder ReplaceTransformer<TExisting, TReplacement>()
        where TExisting : class, ITransformer
        where TReplacement : class, ITransformer
    {
        ServiceRegistrationOperations.ReplaceTransformer<TExisting, TReplacement>(Services);
        return this;
    }

    /// <summary>
    /// Removes all transformer registrations.
    /// </summary>
    public PapercraftServiceBuilder ClearTransformers()
    {
        ServiceRegistrationOperations.ClearTransformers(Services);
        return this;
    }

    /// <summary>
    /// Adds a template function to the service collection.
    /// </summary>
    public PapercraftServiceBuilder AddFunction<TFunction>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TFunction : class, IFunction
    {
        ServiceRegistrationOperations.AddFunction<TFunction>(Services, lifetime);
        return this;
    }
}
