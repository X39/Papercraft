namespace X39.Solutions.Papercraft;

/// <summary>
/// Describes a control type registered for use in XML templates.
/// </summary>
/// <param name="Namespace">The XML namespace of the control.</param>
/// <param name="Name">The XML name of the control.</param>
/// <param name="Type">The implementation type of the control.</param>
public sealed record ControlRegistration(string Namespace, string Name, Type Type);