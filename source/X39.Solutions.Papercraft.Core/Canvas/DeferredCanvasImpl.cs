using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft.Canvas;

internal sealed class DeferredCanvasImpl : IDeferredCanvas
{
    private readonly List<Action<IImmediateCanvas>> _drawActions = new();
    private readonly DisplayList                    _displayList = new();

    // We want to allow ~20 levels of nesting without a reallocation.
    private const    int          DefaultStackCapacity = 20 + 1;
    private readonly Stack<Point> _stateStack          = new(DefaultStackCapacity);
    public           Point        Translation                            => _stateStack.Count is not 0 ? _stateStack.Peek() : new Point();
    public Size ActualPageSize { get; set; }
    public Size PageSize { get; set; }
    internal DisplayList DisplayList => _displayList;

    public void Defer(Action<IImmediateCanvas> action)
    {
        _displayList.Add(new BackendDrawCommand("Deferred IImmediateCanvas callback"));
        _drawActions.Add(action);
    }

    internal void Render(IImmediateCanvas canvas)
    {
        foreach (var action in _drawActions)
        {
            action(canvas);
        }
    }

    public void PushState()
    {
        _stateStack.Push(Translation);
        _displayList.Add(new PushStateCommand());
        _drawActions.Add((canvas) => canvas.PushState());
    }

    public void Clip(Rectangle rectangle)
    {
        _displayList.Add(new ClipCommand(ToDisplay(rectangle)));
        _drawActions.Add((canvas) => canvas.Clip(rectangle));
    }

    public void DrawRect(Rectangle rectangle, Color color)
    {
        if (color.Alpha is 0)
            return;
        _displayList.Add(new DrawRectangleCommand(ToDisplay(rectangle), ToDisplay(color)));
        _drawActions.Add((canvas) => canvas.DrawRect(rectangle, color));
    }

    public void Translate(Point point)
    {
        if (point is { X: 0, Y: 0 })
            return;
        var translation = Translation + point;
        _stateStack.Pop();
        _stateStack.Push(translation);
        _displayList.Add(new TranslateCommand(ToDisplay(point)));
        _drawActions.Add((canvas) => canvas.Translate(point));
    }

    public void PopState()
    {
        _stateStack.Pop();
        _displayList.Add(new PopStateCommand());
        _drawActions.Add((canvas) => canvas.PopState());
    }

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
        if (color.Alpha is 0)
            return;
        _displayList.Add(new DrawLineCommand(ToDisplay(color), thickness, startX, startY, endX, endY));
        _drawActions.Add((canvas) =>canvas.DrawLine(color, thickness, startX, startY, endX, endY));
    }

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        _displayList.Add(new DrawTextCommand(ToDisplay(textStyle), dpi, text, x, y));
        _drawActions.Add((canvas) => canvas.DrawText(textStyle, dpi, text, x, y));
    }

    public void DrawImage(byte[] bytes, Rectangle rectangle)
    {
        _displayList.Add(new DrawImageCommand(bytes, ToDisplay(rectangle)));
        _drawActions.Add((canvas) => canvas.DrawImage(bytes, rectangle));
    }

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
            Foreground       = ToDisplay(textStyle.Foreground),
            FontSize         = textStyle.FontSize,
            FontFamily       = ToDisplay(textStyle.FontFamily),
            Scale            = textStyle.Scale,
            LineHeight       = textStyle.LineHeight,
            Rotation         = textStyle.Rotation,
            StrokeThickness  = textStyle.StrokeThickness,
        };

    private static DisplayFont ToDisplay(Font font)
        => new(font.Family)
        {
            LetterSpacing = font.LetterSpacing,
            Weight        = font.Weight,
            Style         = font.Style switch
            {
                EFontStyle.Italic  => DisplayFontStyle.Italic,
                EFontStyle.Oblique => DisplayFontStyle.Oblique,
                _                  => DisplayFontStyle.Upright,
            },
        };
}
