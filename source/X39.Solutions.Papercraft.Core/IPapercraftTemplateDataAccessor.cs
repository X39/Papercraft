using X39.Solutions.Papercraft.Abstraction;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Provides access to request-scoped template data for renderers that expose it.
/// </summary>
public interface IPapercraftTemplateDataAccessor
{
    /// <summary>
    /// The template data used by the current renderer instance.
    /// </summary>
    ITemplateData TemplateData { get; }
}
