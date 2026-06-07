namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Renderer-neutral font payload used by display commands.
/// </summary>
/// <param name="Family">The requested font family.</param>
public readonly record struct DisplayFont(string Family)
{
    /// <summary>
    /// A renderer-resolved default sans serif font.
    /// </summary>
    public static DisplayFont Default { get; } = new("sans-serif");

    /// <summary>
    /// Font width or letter spacing value.
    /// </summary>
    public ushort LetterSpacing { get; init; }

    /// <summary>
    /// Font weight value.
    /// </summary>
    public ushort Weight { get; init; }

    /// <summary>
    /// Font style.
    /// </summary>
    public DisplayFontStyle Style { get; init; }
}
