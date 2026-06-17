using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Services;
using X39.Solutions.Papercraft.Services.PropertyAccessCache;
using X39.Solutions.Papercraft.Services.ResourceResolver;
using X39.Solutions.Papercraft.Transformers;

namespace X39.Solutions.Papercraft;

internal static class ServiceRegistrationOperations
{
    public static void AddCoreServices(IServiceCollection services)
    {
        services.TryAddSingleton<ControlActivationCache>();
        services.TryAddSingleton<ControlRegistry>();
        services.TryAddSingleton<IPropertyAccessCache, PropertyAccessCache>();
        services.TryAddScoped<ITemplateData, TemplateData>();
        services.TryAddScoped<IResourceResolver, DefaultResourceResolver>();
        services.TryAddScoped<IControlFactory, ControlFactory>();
        services.TryAddSingleton<Papercraft>();
        services.TryAddTransient<PapercraftGenerator>();
        services.TryAddTransient<PapercraftRenderer>();
    }

    public static void AddDefaultControls(IServiceCollection services)
    {
        AddControl<Controls.BarChart>(services);
        AddControl<Controls.BlockControl>(services);
        AddControl<Controls.BorderControl>(services);
        AddControl<Controls.ChartControl>(services);
        AddControl<Controls.ChartDataControl>(services);
        AddControl<Controls.CheckboxControl>(services);
        AddControl<Controls.ColumnsControl>(services);
        AddControl<Controls.ImageControl>(services);
        AddControl<Controls.LineChart>(services);
        AddControl<Controls.LineControl>(services);
        AddControl<Controls.HyperlinkControl>(services);
        AddControl<Controls.ListItemControl>(services);
        AddControl<Controls.OrderedListControl>(services);
        AddControl<Controls.PageBreakControl>(services);
        AddControl<Controls.PageNumberControl>(services);
        AddControl<Controls.ParagraphControl>(services);
        AddControl<Controls.PieChart>(services);
        AddControl<Controls.BrControl>(services);
        AddControl<Controls.SignatureControl>(services);
        AddControl<Controls.SpacerControl>(services);
        AddControl<Controls.SpanControl>(services);
        AddControl<Controls.TableCellControl>(services);
        AddControl<Controls.TableControl>(services);
        AddControl<Controls.TableHeaderControl>(services);
        AddControl<Controls.TableRowControl>(services);
        AddControl<Controls.TextControl>(services);
        AddControl<Controls.UnorderedListControl>(services);
    }

    public static void AddDefaultTransformers(IServiceCollection services)
    {
        AddTransformer<ForTransformer>(services);
        AddTransformer<IfTransformer>(services);
        AddTransformer<SwitchTransformer>(services);
        AddTransformer<ForEachTransformer>(services);
        AddTransformer<AlternateTransformer>(services);
        AddTransformer<VariableTransformer>(services);
    }

    public static void AddControl<TControl>(IServiceCollection services)
        where TControl : IControl
    {
        var registration = ControlRegistry.CreateRegistration<TControl>();
        if (!services.Any((q) => TryGetControlRegistration(q, out var existing) && existing.Type == registration.Type))
            services.AddSingleton(registration);
    }

    public static void RemoveControl<TControl>(IServiceCollection services)
        where TControl : IControl
    {
        RemoveAll(
            services,
            (q) => TryGetControlRegistration(q, out var registration)
                   && registration.Type == typeof(TControl));
    }

    public static void ReplaceControl<TControl>(IServiceCollection services)
        where TControl : IControl
    {
        var registration = ControlRegistry.CreateRegistration<TControl>();
        RemoveAll(
            services,
            (q) => TryGetControlRegistration(q, out var existing)
                   && NamesEqual(existing.Namespace, registration.Namespace)
                   && NamesEqual(existing.Name, registration.Name));
        AddControl<TControl>(services);
    }

    public static void ClearControls(IServiceCollection services)
        => RemoveAll(services, (q) => q.ServiceType == typeof(ControlRegistration));

    public static void AddTransformer<TTransformer>(IServiceCollection services)
        where TTransformer : class, ITransformer
        => services.TryAddEnumerable(ServiceDescriptor.Transient<ITransformer, TTransformer>());

    public static void RemoveTransformer<TTransformer>(IServiceCollection services)
        where TTransformer : class, ITransformer
        => RemoveAll(services, (q) => IsServiceDescriptorFor<TTransformer>(q, typeof(ITransformer)));

    public static void ReplaceTransformer<TExisting, TReplacement>(IServiceCollection services)
        where TExisting : class, ITransformer
        where TReplacement : class, ITransformer
    {
        RemoveTransformer<TExisting>(services);
        AddTransformer<TReplacement>(services);
    }

    public static void ClearTransformers(IServiceCollection services)
        => RemoveAll(services, (q) => q.ServiceType == typeof(ITransformer));

    public static void AddFunction<TFunction>(IServiceCollection services, ServiceLifetime lifetime)
        where TFunction : class, IFunction
        => services.TryAddEnumerable(CreateFunctionDescriptor<TFunction>(lifetime));

    private static void RemoveAll(IServiceCollection services, Func<ServiceDescriptor, bool> predicate)
    {
        for (var i = services.Count - 1; i >= 0; i--)
        {
            if (predicate(services[i]))
                services.RemoveAt(i);
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
