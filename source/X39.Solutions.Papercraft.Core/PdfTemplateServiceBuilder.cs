using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Abstraction;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Builder for configuring PDF template services.
/// </summary>
public sealed class PdfTemplateServiceBuilder
{
    /// <summary>
    /// Creates a new <see cref="PdfTemplateServiceBuilder"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public PdfTemplateServiceBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Services = services;
    }

    /// <summary>
    /// The service collection being configured.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Adds a control to the service collection, making it available for use in templates.
    /// </summary>
    /// <typeparam name="TControl">The type of the control to add.</typeparam>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder AddControl<TControl>()
        where TControl : IControl
    {
        ServiceRegistrationOperations.AddControl<TControl>(Services);
        return this;
    }

    /// <summary>
    /// Removes a control from the service collection.
    /// </summary>
    /// <typeparam name="TControl">The type of the control to remove.</typeparam>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder RemoveControl<TControl>()
        where TControl : IControl
    {
        ServiceRegistrationOperations.RemoveControl<TControl>(Services);
        return this;
    }

    /// <summary>
    /// Replaces any controls registered for the same XML namespace and name.
    /// </summary>
    /// <typeparam name="TControl">The replacement control type.</typeparam>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder ReplaceControl<TControl>()
        where TControl : IControl
    {
        ServiceRegistrationOperations.ReplaceControl<TControl>(Services);
        return this;
    }

    /// <summary>
    /// Removes all control registrations.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder ClearControls()
    {
        ServiceRegistrationOperations.ClearControls(Services);
        return this;
    }

    /// <summary>
    /// Adds a transformer to the service collection, making it available for use in templates.
    /// </summary>
    /// <typeparam name="TTransformer">The type of the transformer to add.</typeparam>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder AddTransformer<TTransformer>()
        where TTransformer : class, ITransformer
    {
        ServiceRegistrationOperations.AddTransformer<TTransformer>(Services);
        return this;
    }

    /// <summary>
    /// Removes a transformer from the service collection.
    /// </summary>
    /// <typeparam name="TTransformer">The type of the transformer to remove.</typeparam>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder RemoveTransformer<TTransformer>()
        where TTransformer : class, ITransformer
    {
        ServiceRegistrationOperations.RemoveTransformer<TTransformer>(Services);
        return this;
    }

    /// <summary>
    /// Replaces a transformer implementation with another implementation.
    /// </summary>
    /// <typeparam name="TExisting">The transformer type to remove.</typeparam>
    /// <typeparam name="TReplacement">The type of the replacement transformer.</typeparam>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder ReplaceTransformer<TExisting, TReplacement>()
        where TExisting : class, ITransformer
        where TReplacement : class, ITransformer
    {
        ServiceRegistrationOperations.ReplaceTransformer<TExisting, TReplacement>(Services);
        return this;
    }

    /// <summary>
    /// Removes all transformer registrations.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder ClearTransformers()
    {
        ServiceRegistrationOperations.ClearTransformers(Services);
        return this;
    }

    /// <summary>
    /// Adds a function to the service collection, making it available for use in templates.
    /// </summary>
    /// <param name="lifetime">The service lifetime for the function.</param>
    /// <typeparam name="TFunction">The type of the function to add.</typeparam>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder AddFunction<TFunction>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TFunction : class, IFunction
    {
        ServiceRegistrationOperations.AddFunction<TFunction>(Services, lifetime);
        return this;
    }
}
