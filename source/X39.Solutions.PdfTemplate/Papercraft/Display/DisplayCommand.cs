using X39.Solutions.PdfTemplate.Data;

namespace X39.Papercraft.Display;

/// <summary>
/// Base type for renderer-neutral display-list commands.
/// </summary>
[PublicAPI]
public abstract record DisplayCommand;

/// <summary>
/// Saves the current drawing state.
/// </summary>
[PublicAPI]
public sealed record PushStateCommand : DisplayCommand;

/// <summary>
/// Restores the previous drawing state.
/// </summary>
[PublicAPI]
public sealed record PopStateCommand : DisplayCommand;

/// <summary>
/// Translates subsequent drawing commands.
/// </summary>
[PublicAPI]
public sealed record TranslateCommand(Point Offset) : DisplayCommand;

/// <summary>
/// Clips subsequent drawing commands to a rectangle.
/// </summary>
[PublicAPI]
public sealed record ClipCommand(Rectangle Rectangle) : DisplayCommand;

/// <summary>
/// Draws a line segment.
/// </summary>
[PublicAPI]
public sealed record DrawLineCommand(
    Color Color,
    float Thickness,
    float StartX,
    float StartY,
    float EndX,
    float EndY) : DisplayCommand;

/// <summary>
/// Draws text.
/// </summary>
[PublicAPI]
public sealed record DrawTextCommand(
    TextStyle TextStyle,
    float Dpi,
    string Text,
    float X,
    float Y) : DisplayCommand;

/// <summary>
/// Draws a filled rectangle.
/// </summary>
[PublicAPI]
public sealed record DrawRectangleCommand(Rectangle Rectangle, Color Color) : DisplayCommand;

/// <summary>
/// Draws encoded image bytes.
/// </summary>
[PublicAPI]
public sealed record DrawImageCommand(byte[] Bytes, Rectangle Rectangle) : DisplayCommand;

/// <summary>
/// Represents a backend-specific drawing callback that cannot yet be expressed neutrally.
/// </summary>
[PublicAPI]
public sealed record BackendDrawCommand(string Description) : DisplayCommand;
