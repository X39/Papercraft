namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Renderer-neutral rectangle payload used by display commands.
/// </summary>
/// <param name="Left">The left coordinate.</param>
/// <param name="Top">The top coordinate.</param>
/// <param name="Width">The width.</param>
/// <param name="Height">The height.</param>
public readonly record struct DisplayRectangle(float Left, float Top, float Width, float Height)
{
    /// <summary>
    /// The bottom coordinate.
    /// </summary>
    public float Bottom => Top + Height;

    /// <summary>
    /// The right coordinate.
    /// </summary>
    public float Right => Left + Width;
}
