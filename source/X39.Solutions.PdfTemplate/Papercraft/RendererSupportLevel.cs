namespace X39.Papercraft;

/// <summary>
/// Describes whether a renderer can support a requested target or feature.
/// </summary>
[PublicAPI]
public enum RendererSupportLevel
{
    /// <summary>
    /// The renderer supports the target or feature.
    /// </summary>
    Supported,

    /// <summary>
    /// The renderer can render the target or feature with a known degradation.
    /// </summary>
    Degraded,

    /// <summary>
    /// The renderer cannot render the target or feature.
    /// </summary>
    Unsupported,
}
