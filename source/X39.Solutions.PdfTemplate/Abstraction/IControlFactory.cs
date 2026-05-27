namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// Creates control instances for template nodes.
/// </summary>
public interface IControlFactory
{
    /// <summary>
    /// Creates a control instance and applies XML parameters and content.
    /// </summary>
    /// <param name="namespace">The XML namespace of the control.</param>
    /// <param name="name">The XML name of the control.</param>
    /// <param name="parameterDictionary">The XML attributes to apply.</param>
    /// <param name="content">The XML text content to apply, if any.</param>
    /// <param name="cultureInfo">The culture to use for parameter conversion.</param>
    /// <returns>The created control.</returns>
    IControl Create(
        string @namespace,
        string name,
        IReadOnlyDictionary<string, string> parameterDictionary,
        string? content,
        CultureInfo cultureInfo);
}
