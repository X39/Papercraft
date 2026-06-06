namespace X39.Papercraft.Display;

/// <summary>
/// A renderer-neutral list of drawing commands.
/// </summary>
[PublicAPI]
public sealed class DisplayList
{
    private readonly List<DisplayCommand> _commands = new();

    /// <summary>
    /// Recorded commands.
    /// </summary>
    public IReadOnlyList<DisplayCommand> Commands => _commands;

    /// <summary>
    /// Adds a command to the display list.
    /// </summary>
    /// <param name="command">The command to add.</param>
    public void Add(DisplayCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _commands.Add(command);
    }
}
