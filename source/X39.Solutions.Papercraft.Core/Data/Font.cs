namespace X39.Solutions.Papercraft.Data;

/// <summary>
/// Represents a font
/// </summary>
/// <param name="Family">The font family</param>
public readonly record struct Font(string Family)
{
    /// <summary>
    /// Default font style.
    /// </summary>
    public static Font Default { get; } = new("Arial");

    /// <summary>
    /// The width or letter-spacing of the font
    /// </summary>
    public FontWidth LetterSpacing { get; init; }

    /// <summary>
    /// The weight of the font
    /// </summary>
    public FontWeight Weight { get; init; }

    /// <summary>
    /// The style of the font.
    /// </summary>
    public EFontStyle Style { get; init; }
}
