using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Services;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Creates control instances for template nodes.
/// </summary>
public sealed class ControlFactory : IControlFactory
{
    private readonly IServiceProvider       _serviceProvider;
    private readonly ControlActivationCache _controlActivationCache;
    private readonly ControlRegistry        _controlRegistry;

    /// <summary>
    /// Creates a new <see cref="ControlFactory"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve constructor dependencies from.</param>
    /// <param name="controlActivationCache">The cache used for control activation and parameter binding.</param>
    /// <param name="controlRegistry">The registry containing available controls.</param>
    public ControlFactory(
        IServiceProvider serviceProvider,
        ControlActivationCache controlActivationCache,
        ControlRegistry controlRegistry)
    {
        _serviceProvider        = serviceProvider;
        _controlActivationCache = controlActivationCache;
        _controlRegistry        = controlRegistry;
    }

    /// <summary>
    /// Creates a control instance and applies XML parameters and content.
    /// </summary>
    /// <param name="namespace">The XML namespace of the control.</param>
    /// <param name="name">The XML name of the control.</param>
    /// <param name="parameterDictionary">The XML attributes to apply.</param>
    /// <param name="content">The XML text content to apply, if any.</param>
    /// <param name="cultureInfo">The culture to use for parameter conversion.</param>
    /// <returns>The created control.</returns>
    public IControl Create(
        string @namespace,
        string name,
        IReadOnlyDictionary<string, string> parameterDictionary,
        string? content,
        CultureInfo cultureInfo)
    {
        var type = _controlRegistry.GetControlType(@namespace, name);
        return _controlActivationCache.CreateControl(
            _serviceProvider,
            type,
            parameterDictionary,
            content,
            cultureInfo);
    }
}
