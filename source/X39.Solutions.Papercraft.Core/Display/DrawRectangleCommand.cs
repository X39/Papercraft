namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Draws a filled rectangle.
/// </summary>
public sealed record DrawRectangleCommand(DisplayRectangle Rectangle, DisplayColor Color) : DisplayCommand;