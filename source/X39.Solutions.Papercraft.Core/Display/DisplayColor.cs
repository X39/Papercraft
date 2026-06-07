namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Renderer-neutral color payload used by display commands.
/// </summary>
/// <param name="Red">The red component.</param>
/// <param name="Green">The green component.</param>
/// <param name="Blue">The blue component.</param>
/// <param name="Alpha">The alpha component.</param>
public readonly record struct DisplayColor(byte Red, byte Green, byte Blue, byte Alpha = 255)
{
    /// <summary>
    /// Opaque black.
    /// </summary>
    public static DisplayColor Black { get; } = new(0, 0, 0);
}
