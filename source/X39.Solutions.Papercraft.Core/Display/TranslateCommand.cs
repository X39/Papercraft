namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Translates subsequent drawing commands.
/// </summary>
public sealed record TranslateCommand(DisplayPoint Offset) : DisplayCommand;