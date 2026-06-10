using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft.Canvas;

internal sealed class DisplayCanvasImpl : IImmediateCanvas
{
    private readonly DisplayList _displayList = new();

    public DisplayList DisplayList => _displayList;

    public ushort PageNumber { get; init; }

    public ushort TotalPages { get; init; }

    public void PushState()
        => _displayList.Add(new PushStateCommand());

    public void Clip(Rectangle rectangle)
        => _displayList.Add(new ClipCommand(ToDisplay(rectangle)));

    public void PopState()
        => _displayList.Add(new PopStateCommand());

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
        if (color.Alpha is 0)
            return;
        _displayList.Add(new DrawLineCommand(ToDisplay(color), thickness, startX, startY, endX, endY));
    }

    public void Translate(Point point)
    {
        if (point is { X: 0, Y: 0 })
            return;
        _displayList.Add(new TranslateCommand(ToDisplay(point)));
    }

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        _displayList.Add(new DrawTextCommand(ToDisplay(textStyle), dpi, text, x, y));
    }

    public void DrawLinkAnnotation(string uri, Rectangle rectangle)
    {
        if (string.IsNullOrWhiteSpace(uri) || rectangle is { Width: <= 0F } or { Height: <= 0F })
            return;
        _displayList.Add(new LinkAnnotationCommand(uri, ToDisplay(rectangle)));
    }

    public void DrawRect(Rectangle rectangle, Color color)
    {
        if (color.Alpha is 0)
            return;
        _displayList.Add(new DrawRectangleCommand(ToDisplay(rectangle), ToDisplay(color)));
    }

    public void DrawImage(byte[] bytes, Rectangle rectangle)
        => _displayList.Add(new DrawImageCommand(bytes, ToDisplay(rectangle)));

    public void DrawImage(ReadOnlyMemory<byte> bytes, Rectangle rectangle)
        => DrawImage(bytes.ToArray(), rectangle);

    public void DrawBitmap(byte[] bytes, Rectangle rectangle)
        => DrawImage(bytes, rectangle);

    private static DisplayPoint ToDisplay(Point point)
        => new(point.X, point.Y);

    private static DisplayRectangle ToDisplay(Rectangle rectangle)
        => new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);

    private static DisplayColor ToDisplay(Color color)
        => new(color.Red, color.Green, color.Blue, color.Alpha);

    private static DisplayTextStyle ToDisplay(TextStyle textStyle)
        => new()
        {
            Foreground = ToDisplay(textStyle.Foreground),
            FontSize = textStyle.FontSize,
            FontFamily = ToDisplay(textStyle.FontFamily),
            Scale = textStyle.Scale,
            LineHeight = textStyle.LineHeight,
            Rotation = textStyle.Rotation,
            StrokeThickness = textStyle.StrokeThickness,
            Decoration = textStyle.Decoration,
        };

    private static DisplayFont ToDisplay(Font font)
        => new(font.Family)
        {
            LetterSpacing = font.LetterSpacing,
            Weight = font.Weight,
            Style = font.Style switch
            {
                EFontStyle.Italic => DisplayFontStyle.Italic,
                EFontStyle.Oblique => DisplayFontStyle.Oblique,
                _ => DisplayFontStyle.Upright,
            },
        };
}
