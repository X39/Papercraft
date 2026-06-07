namespace X39.Solutions.Papercraft;

/// <summary>
/// Options for a Papercraft render request.
/// </summary>
public sealed record PapercraftRenderOptions
{
    /// <summary>
    /// Default Papercraft render options.
    /// </summary>
    public static PapercraftRenderOptions Default { get; } = new();

    /// <summary>
    /// Existing document layout and PDF metadata options.
    /// </summary>
    public DocumentOptions DocumentOptions { get; init; } = DocumentOptions.Default;

    /// <summary>
    /// Optional backend id. If unset, Papercraft selects the first backend supporting the target.
    /// </summary>
    public string? BackendId { get; init; }

    /// <summary>
    /// Treat degraded validation diagnostics as render-blocking diagnostics.
    /// </summary>
    public bool TreatDegradedAsUnsupported { get; init; }
}
