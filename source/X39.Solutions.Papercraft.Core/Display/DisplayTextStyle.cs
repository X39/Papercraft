namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Renderer-neutral text styling payload used by display commands.
/// </summary>
public readonly record struct DisplayTextStyle()
{
    /// <summary>
    /// The foreground color.
    /// </summary>
    public DisplayColor Foreground { get; init; } = DisplayColor.Black;

    /// <summary>
    /// The font size in points.
    /// </summary>
    public float FontSize { get; init; } = 12F;

    /// <summary>
    /// The requested font family.
    /// </summary>
    public DisplayFont FontFamily { get; init; } = DisplayFont.Default;

    /// <summary>
    /// The text scale.
    /// </summary>
    public float Scale { get; init; } = 1F;

    /// <summary>
    /// The line height multiplier.
    /// </summary>
    public float LineHeight { get; init; } = 1F;

    /// <summary>
    /// The text rotation in degrees.
    /// </summary>
    public float Rotation { get; init; }

    /// <summary>
    /// The foreground stroke thickness.
    /// </summary>
    public float StrokeThickness { get; init; } = 1F;
}
