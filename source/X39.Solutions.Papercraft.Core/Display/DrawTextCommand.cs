namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Draws text.
/// </summary>
public sealed record DrawTextCommand(
    DisplayTextStyle TextStyle,
    float Dpi,
    string Text,
    float X,
    float Y) : DisplayCommand;