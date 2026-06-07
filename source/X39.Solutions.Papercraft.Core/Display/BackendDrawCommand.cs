namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Represents a backend-specific drawing callback that cannot yet be expressed neutrally.
/// </summary>
public sealed record BackendDrawCommand(string Description) : DisplayCommand;