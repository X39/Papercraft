using X39.Solutions.PdfTemplate;

namespace X39.Papercraft;

/// <summary>
/// Options for a Papercraft render request.
/// </summary>
[PublicAPI]
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
    /// Optional renderer id. If unset, Papercraft selects the first renderer supporting the target.
    /// </summary>
    public string? RendererId { get; init; }

    /// <summary>
    /// Treat degraded validation diagnostics as render-blocking diagnostics.
    /// </summary>
    public bool TreatDegradedAsUnsupported { get; init; }
}
