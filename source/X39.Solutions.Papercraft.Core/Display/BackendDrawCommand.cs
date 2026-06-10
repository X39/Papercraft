namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Represents a backend-specific drawing callback that cannot yet be expressed neutrally.
/// </summary>
public sealed record BackendDrawCommand(string Description) : DisplayCommand
{
    internal BackendDrawCommand(string description, int callbackIndex)
        : this(description)
    {
        CallbackIndex = callbackIndex;
    }

    internal int CallbackIndex { get; init; } = -1;
}
