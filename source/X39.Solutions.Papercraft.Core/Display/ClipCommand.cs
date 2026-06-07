namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Clips subsequent drawing commands to a rectangle.
/// </summary>
public sealed record ClipCommand(DisplayRectangle Rectangle) : DisplayCommand;