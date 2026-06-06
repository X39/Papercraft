using X39.Solutions.PdfTemplate.Abstraction;

namespace X39.Papercraft;

/// <summary>
/// Provides access to request-scoped template data for compatibility with the existing generator UX.
/// </summary>
[PublicAPI]
public interface IPapercraftTemplateDataAccessor
{
    /// <summary>
    /// The template data used by the current renderer instance.
    /// </summary>
    ITemplateData TemplateData { get; }
}
