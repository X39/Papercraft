namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Draws a line segment.
/// </summary>
public sealed record DrawLineCommand(
    DisplayColor Color,
    float Thickness,
    float StartX,
    float StartY,
    float EndX,
    float EndY) : DisplayCommand;