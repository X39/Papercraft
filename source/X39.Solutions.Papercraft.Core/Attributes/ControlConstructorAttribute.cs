using X39.Solutions.Papercraft.Abstraction;

namespace X39.Solutions.Papercraft.Attributes;

/// <summary>
/// Attribute to explicitly mark a constructor for use when creating a <see cref="IControl"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class ControlConstructorAttribute : Attribute;
