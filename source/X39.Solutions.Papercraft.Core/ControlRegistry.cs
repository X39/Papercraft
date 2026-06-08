using System.Reflection;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Provides access to the control registrations configured through dependency injection.
/// </summary>
public sealed class ControlRegistry
{
    private readonly Dictionary<(string @namespace, string name), Type> _controls;

    /// <summary>
    /// Creates a new <see cref="ControlRegistry"/>.
    /// </summary>
    /// <param name="registrations">The registered controls.</param>
    public ControlRegistry(IEnumerable<ControlRegistration> registrations)
    {
        _controls = new Dictionary<(string @namespace, string name), Type>();
        foreach (var registration in registrations)
        {
            var key = Key(registration.Namespace, registration.Name);
            if (_controls.TryGetValue(key, out var existingType))
                throw new InvalidOperationException(
                    $"The control {registration.Namespace}:{registration.Name} is registered more than once. Existing type: {existingType.FullName ?? existingType.Name}.");
            _controls.Add(key, registration.Type);
        }
    }

    /// <summary>
    /// Gets the implementation type for the provided XML control name.
    /// </summary>
    /// <param name="namespace">The XML namespace of the control.</param>
    /// <param name="name">The XML name of the control.</param>
    /// <returns>The registered control implementation type.</returns>
    public Type GetControlType(string @namespace, string name)
    {
        var key = Key(@namespace, name);
        if (_controls.TryGetValue(key, out var type))
            return type;

        if (NamesEqual(@namespace, Constants.LegacyControlsNamespace))
        {
            var fallbackKey = Key(Constants.ControlsNamespace, name);
            if (_controls.TryGetValue(fallbackKey, out type))
                return type;
        }

        throw new InvalidOperationException($"The control {@namespace}:{name} does not exist.");
    }

    /// <summary>
    /// Creates a registration for a control type using its <see cref="ControlAttribute"/>.
    /// </summary>
    /// <typeparam name="TControl">The type of the control to register.</typeparam>
    /// <returns>The control registration.</returns>
    public static ControlRegistration CreateRegistration<TControl>()
        where TControl : IControl
    {
        var type = typeof(TControl);

        if (type.IsGenericType)
            throw new InvalidOperationException(
                $"The type {type.FullName} is a generic type and cannot be used as a control.");
        var attribute = type.GetCustomAttribute<ControlAttribute>();
        if (attribute is null)
            throw new InvalidOperationException(
                $"The type {type.FullName} does not have a {nameof(ControlAttribute)}.");
        var name = attribute.Name;
        if (string.IsNullOrEmpty(name))
        {
            const string controlSuffix = "Control";
            name = type.Name;

            if (name.EndsWith(controlSuffix, StringComparison.Ordinal))
                name = name[..^controlSuffix.Length];
        }

        return new ControlRegistration(attribute.Namespace, name, type);
    }

    private static (string @namespace, string name) Key(string @namespace, string name)
        => (@namespace.ToUpper(CultureInfo.InvariantCulture), name.ToUpper(CultureInfo.InvariantCulture));

    private static bool NamesEqual(string left, string right)
        => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
