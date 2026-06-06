namespace X39.Papercraft;

/// <summary>
/// The result of validating a document against a renderer and output target.
/// </summary>
[PublicAPI]
public sealed class RenderValidationResult
{
    /// <summary>
    /// Creates a validation result.
    /// </summary>
    /// <param name="diagnostics">The diagnostics produced by validation.</param>
    public RenderValidationResult(IEnumerable<RenderDiagnostic> diagnostics)
    {
        Diagnostics = diagnostics.ToArray();
    }

    /// <summary>
    /// A validation result without diagnostics.
    /// </summary>
    public static RenderValidationResult Supported { get; } = new(Array.Empty<RenderDiagnostic>());

    /// <summary>
    /// Diagnostics produced by validation.
    /// </summary>
    public IReadOnlyList<RenderDiagnostic> Diagnostics { get; }

    /// <summary>
    /// The aggregate support level.
    /// </summary>
    public RendererSupportLevel SupportLevel
    {
        get
        {
            if (Diagnostics.Any((q) => q.Level is RendererSupportLevel.Unsupported))
                return RendererSupportLevel.Unsupported;
            return Diagnostics.Any((q) => q.Level is RendererSupportLevel.Degraded)
                ? RendererSupportLevel.Degraded
                : RendererSupportLevel.Supported;
        }
    }

    /// <summary>
    /// Indicates whether rendering can proceed without unsupported diagnostics.
    /// </summary>
    public bool IsSupported => SupportLevel is not RendererSupportLevel.Unsupported;

    /// <summary>
    /// Throws a <see cref="RenderValidationException"/> if unsupported diagnostics exist.
    /// </summary>
    public void ThrowIfUnsupported()
    {
        if (!IsSupported)
            throw new RenderValidationException(this);
    }

    /// <summary>
    /// Throws a <see cref="RenderValidationException"/> if degraded diagnostics should be treated as failures.
    /// </summary>
    /// <param name="strict">Whether degraded diagnostics should fail rendering.</param>
    public void ThrowIfUnsupportedOrStrictDegraded(bool strict)
    {
        if (SupportLevel is RendererSupportLevel.Unsupported
            || (strict && SupportLevel is RendererSupportLevel.Degraded))
            throw new RenderValidationException(this);
    }
}
