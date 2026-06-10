using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Exceptions;

namespace X39.Solutions.Papercraft.Services;

/// <summary>
/// Caches control activation and parameter binding metadata.
/// </summary>
public sealed class ControlActivationCache
{
    private static readonly object[] EmptyArguments = [];

    private delegate object ObjectFactory(IServiceProvider serviceProvider, object?[] arguments);

    private readonly ConcurrentDictionary<Type, ControlPlan> _controlPlans = new();

    /// <summary>
    /// Creates a control of the specified type.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve constructor dependencies from.</param>
    /// <param name="type">The type of the control to create.</param>
    /// <param name="parameterDictionary">The attributes to set on the control.</param>
    /// <param name="content">The content to set on the control or <see langword="null"/> if no content is provided.</param>
    /// <param name="cultureInfo">The culture to use for parameter conversion.</param>
    /// <returns>The created control.</returns>
    public IControl CreateControl(
        IServiceProvider                    serviceProvider,
        Type                                type,
        IReadOnlyDictionary<string, string> parameterDictionary,
        string?                             content,
        CultureInfo                         cultureInfo)
    {
        var controlPlan = _controlPlans.GetOrAdd(type, static (controlType) => new ControlPlan(controlType));
        var control     = (IControl) controlPlan.Factory(serviceProvider, EmptyArguments);
        if (parameterDictionary.Count is 0 && string.IsNullOrEmpty(content))
            return control;

        SetParametersOfControl(
            serviceProvider,
            type,
            control,
            controlPlan.ParameterPlan.Value,
            parameterDictionary,
            content,
            cultureInfo);
        return control;
    }

    private static void SetParametersOfControl(
        IServiceProvider                    serviceProvider,
        Type                                controlType,
        IControl                            control,
        ParameterPlan                       parameterPlan,
        IReadOnlyDictionary<string, string> parameterDictionary,
        string?                             content,
        CultureInfo                         cultureInfo)
    {
        List<string>? missingParameters = null;
        foreach (var (parameterName, value) in parameterDictionary)
        {
            if (parameterPlan.Bindings.TryGetValue(parameterName, out var binding))
            {
                binding.Set(serviceProvider, control, value, cultureInfo);
                continue;
            }

            (missingParameters ??= []).Add(parameterName.ToUpperInvariant());
        }

        if (missingParameters is { Count: > 0 })
            throw new ControlParameterIsNotExistingException(
                controlType,
                missingParameters.ToArray(),
                parameterPlan.AvailableParameterNames);

        if (string.IsNullOrEmpty(content))
            return;

        if (parameterPlan.ContentBinding is { } contentBinding)
        {
            contentBinding.Set(serviceProvider, control, content, cultureInfo);
            return;
        }

        throw new InvalidOperationException($"The control {controlType.FullName ?? controlType.Name} does not support content.");
    }

    private static ParameterPlan CreateParameterPlan(Type controlType)
    {
        var bindings = new Dictionary<string, ParameterBinding>(StringComparer.OrdinalIgnoreCase);

        foreach (var propertyInfo in controlType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var parameterAttribute = propertyInfo.GetCustomAttribute<ParameterAttribute>();
            if (parameterAttribute is null)
                continue;

            var parameterName = Validators.ParameterName.Get(parameterAttribute, propertyInfo);
            var binding = new ParameterBinding(
                parameterAttribute,
                parameterName,
                CreateSetter(controlType, propertyInfo, parameterAttribute));
            bindings[parameterName] = binding;
        }

        return new ParameterPlan(
            bindings,
            bindings.Values.FirstOrDefault((binding) => binding.Attribute.IsContent),
            bindings.Values.Select((binding) => binding.ParameterName).ToArray());
    }

    private static Action<IServiceProvider, IControl, string, CultureInfo> CreateSetter(
        Type               controlType,
        PropertyInfo       propertyInfo,
        ParameterAttribute parameterAttribute)
    {
        var propertySetter = propertyInfo.GetSetMethod(true)
                             ?? throw new InvalidOperationException(
                                 $"The property {propertyInfo} of {controlType.FullName ?? controlType.Name} has no setter.");

        var converterType = parameterAttribute.Converter
                            ?? propertyInfo.PropertyType
                                           .GetCustomAttribute<ParameterConverterAttributeBase>()
                                           ?.Converter;
        if (converterType is not null)
            return CreateParameterConverterSetter(
                controlType,
                propertyInfo.PropertyType,
                propertySetter,
                converterType,
                parameterAttribute.Format);

        if (propertyInfo.PropertyType.IsEquivalentTo(typeof(string)))
            return CreateStringSetter(controlType, propertySetter);

        if (TypeDescriptor.GetConverter(propertyInfo.PropertyType) is { } typeConverter
            && typeConverter.CanConvertFrom(typeof(string)))
            return CreateTypeConverterSetter(controlType, propertyInfo.PropertyType, propertySetter, typeConverter);

        if (propertyInfo.PropertyType.GetInterfaces()
                        .FirstOrDefault((q) => q.IsGenericType
                                               && q.GetGenericTypeDefinition() == typeof(IParsable<>)) is { } parsable
            && propertyInfo.PropertyType.IsAssignableTo(parsable))
            return CreateParsableSetter(controlType, propertyInfo.PropertyType, propertySetter);

        throw new InvalidOperationException(
            $"The property {propertyInfo} of {controlType.FullName ?? controlType.Name} has no converter, is not a string and has no {typeof(TypeConverter).FullName ?? typeof(TypeConverter).Name} that can convert from {typeof(string).FullName ?? typeof(string).Name}.");
    }

    private static Action<IServiceProvider, IControl, string, CultureInfo> CreateParameterConverterSetter(
        Type       controlType,
        Type       propertyType,
        MethodInfo propertySetter,
        Type       converterType,
        string?    format)
    {
        var interfaceType = converterType.GetInterfaces()
                                         .FirstOrDefault(
                                             (q) => q.IsGenericType
                                                    && q.GetGenericTypeDefinition() == typeof(IParameterConverter<>)
                                                    && propertyType.IsAssignableFrom(q.GetGenericArguments()[0]))
                            ?? throw new InvalidOperationException(
                                $"The converter {converterType} used on {controlType.FullName ?? controlType.Name} does not implement {typeof(IParameterConverter<>).FullName ?? typeof(IParameterConverter<>).Name}.");
        var convertedType = interfaceType.GetGenericArguments()[0];
        return (Action<IServiceProvider, IControl, string, CultureInfo>) typeof(ControlActivationCache)
            .GetMethod(nameof(CreateParameterConverterSetterCore), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(controlType, propertyType, convertedType)
            .Invoke(null, [propertySetter, CreateAttributedFactory<ParameterConverterConstructorAttribute>(converterType), format])!;
    }

    private static Action<IServiceProvider, IControl, string, CultureInfo> CreateParameterConverterSetterCore<TControl, TProperty, TConverted>(
        MethodInfo    propertySetter,
        ObjectFactory converterFactory,
        string?       format)
        where TControl : IControl
    {
        var setter = propertySetter.CreateDelegate<Action<TControl, TProperty>>();
        return (serviceProvider, control, value, cultureInfo) =>
        {
            var converter = (IParameterConverter<TConverted>) converterFactory(serviceProvider, EmptyArguments);
            setter((TControl) control, (TProperty) (object) converter.Convert(value, format, cultureInfo)!);
        };
    }

    private static Action<IServiceProvider, IControl, string, CultureInfo> CreateStringSetter(
        Type       controlType,
        MethodInfo propertySetter)
        => (Action<IServiceProvider, IControl, string, CultureInfo>) typeof(ControlActivationCache)
            .GetMethod(nameof(CreateStringSetterCore), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(controlType)
            .Invoke(null, [propertySetter])!;

    private static Action<IServiceProvider, IControl, string, CultureInfo> CreateStringSetterCore<TControl>(
        MethodInfo propertySetter)
        where TControl : IControl
    {
        var setter = propertySetter.CreateDelegate<Action<TControl, string>>();
        return (_, control, value, _) => setter((TControl) control, value);
    }

    private static Action<IServiceProvider, IControl, string, CultureInfo> CreateTypeConverterSetter(
        Type          controlType,
        Type          propertyType,
        MethodInfo    propertySetter,
        TypeConverter typeConverter)
        => (Action<IServiceProvider, IControl, string, CultureInfo>) typeof(ControlActivationCache)
            .GetMethod(nameof(CreateTypeConverterSetterCore), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(controlType, propertyType)
            .Invoke(null, [propertySetter, typeConverter])!;

    private static Action<IServiceProvider, IControl, string, CultureInfo> CreateTypeConverterSetterCore<TControl, TValue>(
        MethodInfo    propertySetter,
        TypeConverter typeConverter)
        where TControl : IControl
    {
        var setter = propertySetter.CreateDelegate<Action<TControl, TValue>>();
        return (_, control, value, cultureInfo)
            => setter((TControl) control, (TValue) typeConverter.ConvertFromString(null, cultureInfo, value)!);
    }

    private static Action<IServiceProvider, IControl, string, CultureInfo> CreateParsableSetter(
        Type       controlType,
        Type       propertyType,
        MethodInfo propertySetter)
        => (Action<IServiceProvider, IControl, string, CultureInfo>) typeof(ControlActivationCache)
            .GetMethod(nameof(CreateParsableSetterCore), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(controlType, propertyType)
            .Invoke(null, [propertySetter])!;

    private static Action<IServiceProvider, IControl, string, CultureInfo> CreateParsableSetterCore<TControl, TValue>(
        MethodInfo propertySetter)
        where TControl : IControl
        where TValue : IParsable<TValue>
    {
        var setter = propertySetter.CreateDelegate<Action<TControl, TValue>>();
        return (_, control, value, cultureInfo)
            => setter((TControl) control, TValue.Parse(value, cultureInfo));
    }

    private sealed class ControlPlan
    {
        public ControlPlan(Type controlType)
        {
            Factory       = CreateAttributedFactory<ControlConstructorAttribute>(controlType);
            ParameterPlan = new Lazy<ParameterPlan>(() => CreateParameterPlan(controlType), true);
        }

        public ObjectFactory Factory { get; }

        public Lazy<ParameterPlan> ParameterPlan { get; }
    }

    private sealed record ParameterPlan(
        IReadOnlyDictionary<string, ParameterBinding> Bindings,
        ParameterBinding? ContentBinding,
        string[] AvailableParameterNames);

    private sealed record ParameterBinding(
        ParameterAttribute Attribute,
        string ParameterName,
        Action<IServiceProvider, IControl, string, CultureInfo> Set);

    private static readonly MethodInfo ResolveConstructorArgumentMethod = typeof(ControlActivationCache)
        .GetMethod(nameof(ResolveConstructorArgument), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static ObjectFactory CreateAttributedFactory<TConstructorAttribute>(Type implementationType)
        where TConstructorAttribute : Attribute
    {
        var attributedConstructors = implementationType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where((constructor) => constructor.GetCustomAttribute<TConstructorAttribute>() is not null)
            .ToArray();

        return attributedConstructors.Length switch
        {
            0 => CreateDefaultFactory(implementationType),
            1 => CreateConstructorFactory<TConstructorAttribute>(implementationType, attributedConstructors[0]),
            _ => throw new InvalidOperationException(
                $"The type {implementationType.FullName ?? implementationType.Name} has more than one {typeof(TConstructorAttribute).Name}."),
        };
    }

    private static ObjectFactory CreateDefaultFactory(Type implementationType)
    {
        if (implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length is 0)
            throw new InvalidOperationException(
                $"The type {implementationType.FullName ?? implementationType.Name} has no public constructor.");

        var factory = ActivatorUtilities.CreateFactory(implementationType, Type.EmptyTypes);
        return (serviceProvider, arguments) => factory(serviceProvider, arguments);
    }

    private static ObjectFactory CreateConstructorFactory<TConstructorAttribute>(
        Type implementationType,
        ConstructorInfo constructor)
        where TConstructorAttribute : Attribute
    {
        var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");
        var constructorArguments = constructor
            .GetParameters()
            .Select(
                (parameter) => Expression.Convert(
                    Expression.Call(
                        ResolveConstructorArgumentMethod,
                        serviceProviderParameter,
                        Expression.Constant(parameter.ParameterType, typeof(Type)),
                        Expression.Constant(parameter.HasDefaultValue),
                        Expression.Constant(parameter.HasDefaultValue ? parameter.DefaultValue : null, typeof(object)),
                        Expression.Constant(implementationType, typeof(Type)),
                        Expression.Constant(typeof(TConstructorAttribute), typeof(Type))),
                    parameter.ParameterType))
            .ToArray();

        var body = Expression.Convert(Expression.New(constructor, constructorArguments), typeof(object));
        return Expression.Lambda<ObjectFactory>(body, serviceProviderParameter, argumentsParameter)
            .Compile();
    }

    private static object? ResolveConstructorArgument(
        IServiceProvider serviceProvider,
        Type parameterType,
        bool hasDefaultValue,
        object? defaultValue,
        Type implementationType,
        Type constructorAttributeType)
    {
        var service = serviceProvider.GetService(parameterType);
        if (service is not null)
            return service;

        if (hasDefaultValue)
            return defaultValue;

        throw new InvalidOperationException(
            $"Unable to resolve service for type {parameterType.FullName ?? parameterType.Name} while activating {implementationType.FullName ?? implementationType.Name} via {constructorAttributeType.Name}.");
    }
}
