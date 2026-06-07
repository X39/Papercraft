namespace X39.Solutions.Papercraft;

/// <summary>
/// Stable diagnostic codes emitted by Papercraft validation.
/// </summary>
public static class RenderDiagnosticCodes
{
    /// <summary>
    /// The selected renderer does not support the requested output kind.
    /// </summary>
    public const string UnsupportedOutputKind = "PAPERCRAFT001";

    /// <summary>
    /// The selected renderer does not support the requested media type.
    /// </summary>
    public const string UnsupportedMediaType = "PAPERCRAFT002";

    /// <summary>
    /// The selected renderer does not support a feature used by the template.
    /// </summary>
    public const string UnsupportedFeature = "PAPERCRAFT003";

    /// <summary>
    /// The selected renderer supports a template feature only with degraded output.
    /// </summary>
    public const string DegradedFeature = "PAPERCRAFT004";
}
