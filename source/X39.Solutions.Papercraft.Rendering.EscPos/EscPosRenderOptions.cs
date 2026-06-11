using System.Text;

namespace X39.Solutions.Papercraft.Rendering.EscPos;

/// <summary>
/// Configures first-pass ESC/POS command emission.
/// </summary>
public sealed record EscPosRenderOptions
{
    /// <summary>
    /// Default ESC/POS render options.
    /// </summary>
    public static EscPosRenderOptions Default { get; } = new();

    /// <summary>
    /// Text encoding used for ESC/POS text payloads.
    /// </summary>
    public Encoding TextEncoding { get; init; } = Encoding.Latin1;

    /// <summary>
    /// Whether rendering starts with ESC @ printer initialization.
    /// </summary>
    public bool InitializePrinter { get; init; } = true;

    /// <summary>
    /// Assumed printer text columns for simple rule-line approximation.
    /// </summary>
    public int CharactersPerLine { get; init; } = 42;

    /// <summary>
    /// Line feeds emitted between generated pages.
    /// </summary>
    public int PageFeedLines { get; init; } = 1;

    /// <summary>
    /// Line feeds emitted after the final page.
    /// </summary>
    public int DocumentEndFeedLines { get; init; }

    /// <summary>
    /// Whether to append a full-cut command after rendering.
    /// </summary>
    public bool CutPaper { get; init; }
}
