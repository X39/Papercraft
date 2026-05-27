using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Solutions.PdfTemplate.Abstraction;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Builder for configuring PDF template services.
/// </summary>
[PublicAPI]
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
    public PdfTemplateServiceBuilder AddControl<
        [MeansImplicitUse(
            ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign)]
        TControl>()
        where TControl : IControl
    {
        var registration = ControlRegistry.CreateRegistration<TControl>();
        if (!Services.Any((q) => TryGetControlRegistration(q, out var existing) && existing.Type == registration.Type))
            Services.AddSingleton(registration);
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
        RemoveAll(
            (q) => TryGetControlRegistration(q, out var registration)
                   && registration.Type == typeof(TControl));
        return this;
    }

    /// <summary>
    /// Replaces any controls registered for the same XML namespace and name.
    /// </summary>
    /// <typeparam name="TControl">The replacement control type.</typeparam>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder ReplaceControl<
        [MeansImplicitUse(
            ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign)]
        TControl>()
        where TControl : IControl
    {
        var registration = ControlRegistry.CreateRegistration<TControl>();
        RemoveAll(
            (q) => TryGetControlRegistration(q, out var existing)
                   && NamesEqual(existing.Namespace, registration.Namespace)
                   && NamesEqual(existing.Name, registration.Name));
        return AddControl<TControl>();
    }

    /// <summary>
    /// Removes all control registrations.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder ClearControls()
    {
        RemoveAll((q) => q.ServiceType == typeof(ControlRegistration));
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
        Services.TryAddEnumerable(ServiceDescriptor.Transient<ITransformer, TTransformer>());
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
        RemoveAll((q) => IsServiceDescriptorFor<TTransformer>(q, typeof(ITransformer)));
        return this;
    }

    /// <summary>
    /// Replaces a transformer implementation with another implementation.
    /// </summary>
    /// <typeparam name="TExisting">The transformer type to remove.</typeparam>
    /// <typeparam name="TReplacement">The transformer type to add.</typeparam>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder ReplaceTransformer<TExisting, TReplacement>()
        where TExisting : class, ITransformer
        where TReplacement : class, ITransformer
    {
        RemoveTransformer<TExisting>();
        return AddTransformer<TReplacement>();
    }

    /// <summary>
    /// Removes all transformer registrations.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PdfTemplateServiceBuilder ClearTransformers()
    {
        RemoveAll((q) => q.ServiceType == typeof(ITransformer));
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
        Services.TryAddEnumerable(CreateFunctionDescriptor<TFunction>(lifetime));
        return this;
    }

    private void RemoveAll(Func<ServiceDescriptor, bool> predicate)
    {
        for (var i = Services.Count - 1; i >= 0; i--)
        {
            if (predicate(Services[i]))
                Services.RemoveAt(i);
        }
    }

    private static ServiceDescriptor CreateFunctionDescriptor<TFunction>(ServiceLifetime lifetime)
        where TFunction : class, IFunction
        => lifetime switch
        {
            ServiceLifetime.Singleton => ServiceDescriptor.Singleton<IFunction, TFunction>(),
            ServiceLifetime.Scoped    => ServiceDescriptor.Scoped<IFunction, TFunction>(),
            ServiceLifetime.Transient => ServiceDescriptor.Transient<IFunction, TFunction>(),
            _                         => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null),
        };

    private static bool TryGetControlRegistration(ServiceDescriptor descriptor, out ControlRegistration registration)
    {
        registration = null!;
        if (descriptor.ServiceType != typeof(ControlRegistration)
            || descriptor.ImplementationInstance is not ControlRegistration controlRegistration)
            return false;
        registration = controlRegistration;
        return true;
    }

    private static bool IsServiceDescriptorFor<TImplementation>(ServiceDescriptor descriptor, Type serviceType)
        => descriptor.ServiceType == serviceType
           && (descriptor.ImplementationType == typeof(TImplementation)
               || descriptor.ImplementationInstance?.GetType() == typeof(TImplementation));

    private static bool NamesEqual(string left, string right)
        => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
