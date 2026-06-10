namespace X39.Solutions.Papercraft.Data;

/// <summary>
/// Decorations applied to rendered text.
/// </summary>
[Flags]
public enum TextDecoration
{
    /// <summary>
    /// No text decoration.
    /// </summary>
    None = 0,

    /// <summary>
    /// Draw a single underline below the text.
    /// </summary>
    Underline = 1 << 0,

    /// <summary>
    /// Draw a line through the text.
    /// </summary>
    StrikeThrough = 1 << 1,

    /// <summary>
    /// Draw two underline strokes below the text.
    /// </summary>
    DoubleUnderline = 1 << 2,
}
