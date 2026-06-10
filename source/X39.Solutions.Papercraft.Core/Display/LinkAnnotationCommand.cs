namespace X39.Solutions.Papercraft.Display;

/// <summary>
/// Adds a clickable URI link annotation over the specified rectangle.
/// </summary>
/// <param name="Uri">The target URI.</param>
/// <param name="Rectangle">The clickable rectangle.</param>
public sealed record LinkAnnotationCommand(string Uri, DisplayRectangle Rectangle) : DisplayCommand;
