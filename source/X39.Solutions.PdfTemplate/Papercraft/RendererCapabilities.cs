namespace X39.Papercraft;

/// <summary>
/// Describes a renderer backend and the output/features it supports.
/// </summary>
[PublicAPI]
public sealed class RendererCapabilities
{
    /// <summary>
    /// Creates renderer capabilities.
    /// </summary>
    public RendererCapabilities(
        string rendererId,
        string displayName,
        RendererOutputKind outputKinds,
        IEnumerable<string> mediaTypes,
        IReadOnlyDictionary<string, RendererSupportLevel>? featureSupport = null,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rendererId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        RendererId = rendererId;
        DisplayName = displayName;
        OutputKinds = outputKinds;
        MediaTypes = mediaTypes.Select((q) => q.Trim())
                               .Where((q) => !q.IsNullOrWhiteSpace())
                               .Distinct(StringComparer.OrdinalIgnoreCase)
                               .ToArray();
        FeatureSupport = featureSupport ?? new Dictionary<string, RendererSupportLevel>();
        Notes = notes;
    }

    /// <summary>
    /// Stable renderer identifier.
    /// </summary>
    public string RendererId { get; }

    /// <summary>
    /// Human-readable renderer name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Supported output kinds.
    /// </summary>
    public RendererOutputKind OutputKinds { get; }

    /// <summary>
    /// Supported media types.
    /// </summary>
    public IReadOnlyCollection<string> MediaTypes { get; }

    /// <summary>
    /// Renderer feature support map.
    /// </summary>
    public IReadOnlyDictionary<string, RendererSupportLevel> FeatureSupport { get; }

    /// <summary>
    /// Backend-specific notes.
    /// </summary>
    public string? Notes { get; }

    /// <summary>
    /// Checks whether the renderer supports the target output kind and media type.
    /// </summary>
    /// <param name="target">The target to check.</param>
    /// <returns><see langword="true"/> if the renderer supports the target.</returns>
    public bool Supports(RenderTarget target)
        => OutputKinds.HasFlag(target.OutputKind)
           && MediaTypes.Any((q) => string.Equals(q, target.MediaType, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets support for a feature.
    /// </summary>
    /// <param name="feature">The feature name.</param>
    /// <returns>The support level. Unknown features are unsupported.</returns>
    public RendererSupportLevel GetFeatureSupport(string feature)
        => FeatureSupport.TryGetValue(feature, out var supportLevel)
            ? supportLevel
            : RendererSupportLevel.Unsupported;
}
