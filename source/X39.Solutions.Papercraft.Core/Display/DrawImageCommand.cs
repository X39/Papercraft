namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Draws encoded image bytes.
/// </summary>
public sealed record DrawImageCommand(byte[] Bytes, DisplayRectangle Rectangle) : DisplayCommand;