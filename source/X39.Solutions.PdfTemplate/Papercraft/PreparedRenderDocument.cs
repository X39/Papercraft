using System.Xml;

namespace X39.Papercraft;

/// <summary>
/// A template document prepared for validation and rendering.
/// </summary>
[PublicAPI]
public sealed class PreparedRenderDocument
{
    /// <summary>
    /// Creates a prepared document.
    /// </summary>
    public PreparedRenderDocument(string templateXml, CultureInfo cultureInfo, PapercraftRenderOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateXml);
        ArgumentNullException.ThrowIfNull(cultureInfo);
        ArgumentNullException.ThrowIfNull(options);
        TemplateXml = templateXml;
        CultureInfo = cultureInfo;
        Options = options;
    }

    /// <summary>
    /// The prepared template XML.
    /// </summary>
    public string TemplateXml { get; }

    /// <summary>
    /// The culture used for parsing, measuring, and rendering.
    /// </summary>
    public CultureInfo CultureInfo { get; }

    /// <summary>
    /// The render options for the request.
    /// </summary>
    public PapercraftRenderOptions Options { get; }

    /// <summary>
    /// Creates a fresh XML reader over the prepared template.
    /// </summary>
    /// <returns>A new XML reader.</returns>
    public XmlReader CreateReader()
        => XmlReader.Create(new StringReader(TemplateXml));
}
