using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft.Canvas;

internal sealed class DeferredCanvasImpl : IDeferredCanvas
{
    private readonly List<Action<IImmediateCanvas>> _deferredActions = new();
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
        var callbackIndex = _deferredActions.Count;
        _deferredActions.Add(action);
        _displayList.Add(new BackendDrawCommand("Deferred IImmediateCanvas callback", callbackIndex));
    }

    internal void Render(IImmediateCanvas canvas)
    {
        foreach (var command in _displayList.Commands)
        {
            ReplayCommand(canvas, command);
        }
    }

    public void PushState()
    {
        _stateStack.Push(Translation);
        _displayList.Add(new PushStateCommand());
    }

    public void Clip(Rectangle rectangle)
    {
        _displayList.Add(new ClipCommand(ToDisplay(rectangle)));
    }

    public void DrawRect(Rectangle rectangle, Color color)
    {
        if (color.Alpha is 0)
            return;
        _displayList.Add(new DrawRectangleCommand(ToDisplay(rectangle), ToDisplay(color)));
    }

    public void Translate(Point point)
    {
        if (point is { X: 0, Y: 0 })
            return;
        var translation = Translation + point;
        _stateStack.Pop();
        _stateStack.Push(translation);
        _displayList.Add(new TranslateCommand(ToDisplay(point)));
    }

    public void PopState()
    {
        _stateStack.Pop();
        _displayList.Add(new PopStateCommand());
    }

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
        if (color.Alpha is 0)
            return;
        _displayList.Add(new DrawLineCommand(ToDisplay(color), thickness, startX, startY, endX, endY));
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

    public void DrawImage(byte[] bytes, Rectangle rectangle)
    {
        _displayList.Add(new DrawImageCommand(bytes, ToDisplay(rectangle)));
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
            Decoration       = textStyle.Decoration,
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

    private void ReplayCommand(IImmediateCanvas canvas, DisplayCommand command)
    {
        switch (command)
        {
            case PushStateCommand:
                canvas.PushState();
                break;
            case PopStateCommand:
                canvas.PopState();
                break;
            case TranslateCommand translate:
                canvas.Translate(ToPoint(translate.Offset));
                break;
            case ClipCommand clip:
                canvas.Clip(ToRectangle(clip.Rectangle));
                break;
            case DrawLineCommand line:
                canvas.DrawLine(
                    ToColor(line.Color),
                    line.Thickness,
                    line.StartX,
                    line.StartY,
                    line.EndX,
                    line.EndY);
                break;
            case DrawRectangleCommand rectangle:
                canvas.DrawRect(ToRectangle(rectangle.Rectangle), ToColor(rectangle.Color));
                break;
            case DrawTextCommand text:
                canvas.DrawText(ToTextStyle(text.TextStyle), text.Dpi, text.Text, text.X, text.Y);
                break;
            case DrawImageCommand image:
                canvas.DrawImage(image.Bytes, ToRectangle(image.Rectangle));
                break;
            case LinkAnnotationCommand link:
                canvas.DrawLinkAnnotation(link.Uri, ToRectangle(link.Rectangle));
                break;
            case BackendDrawCommand { CallbackIndex: >= 0 } callback:
                _deferredActions[callback.CallbackIndex](canvas);
                break;
        }
    }

    private static Point ToPoint(DisplayPoint point)
        => new(point.X, point.Y);

    private static Rectangle ToRectangle(DisplayRectangle rectangle)
        => new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);

    private static Color ToColor(DisplayColor color)
        => new(color.Red, color.Green, color.Blue, color.Alpha);

    private static TextStyle ToTextStyle(DisplayTextStyle textStyle)
        => new()
        {
            Foreground = ToColor(textStyle.Foreground),
            FontSize = textStyle.FontSize,
            FontFamily = ToFont(textStyle.FontFamily),
            Scale = textStyle.Scale,
            LineHeight = textStyle.LineHeight,
            Rotation = textStyle.Rotation,
            StrokeThickness = textStyle.StrokeThickness,
            Decoration = textStyle.Decoration,
        };

    private static Font ToFont(DisplayFont font)
        => new(font.Family)
        {
            LetterSpacing = font.LetterSpacing,
            Weight = font.Weight,
            Style = font.Style switch
            {
                DisplayFontStyle.Italic => EFontStyle.Italic,
                DisplayFontStyle.Oblique => EFontStyle.Oblique,
                _ => EFontStyle.Upright,
            },
        };
}
