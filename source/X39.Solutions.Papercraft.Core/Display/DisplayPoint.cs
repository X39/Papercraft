namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Renderer-neutral point payload used by display commands.
/// </summary>
/// <param name="X">The X coordinate.</param>
/// <param name="Y">The Y coordinate.</param>
public readonly record struct DisplayPoint(float X, float Y);
