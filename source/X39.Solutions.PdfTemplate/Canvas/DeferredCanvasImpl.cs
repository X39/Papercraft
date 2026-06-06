using SkiaSharp;
using X39.Papercraft.Display;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Canvas;

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
        _displayList.Add(new ClipCommand(rectangle));
        _drawActions.Add((canvas) => canvas.Clip(rectangle));
    }

    public void DrawRect(Rectangle rectangle, Color color)
    {
        if (color.Alpha is 0)
            return;
        _displayList.Add(new DrawRectangleCommand(rectangle, color));
        _drawActions.Add((canvas) => canvas.DrawRect(rectangle, color));
    }

    public void Translate(Point point)
    {
        if (point is { X: 0, Y: 0 })
            return;
        var translation = Translation + point;
        _stateStack.Pop();
        _stateStack.Push(translation);
        _displayList.Add(new TranslateCommand(point));
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
        _displayList.Add(new DrawLineCommand(color, thickness, startX, startY, endX, endY));
        _drawActions.Add((canvas) =>canvas.DrawLine(color, thickness, startX, startY, endX, endY));
    }

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
    {
        if (text.IsNullOrWhiteSpace())
            return;
        _displayList.Add(new DrawTextCommand(textStyle, dpi, text, x, y));
        _drawActions.Add((canvas) => canvas.DrawText(textStyle, dpi, text, x, y));
    }

    public void DrawBitmap(byte[] bytes, Rectangle rectangle)
    {
        _displayList.Add(new DrawImageCommand(bytes, rectangle));
        _drawActions.Add((canvas) => canvas.DrawBitmap(bytes, rectangle));
    }

    public void DrawBitmap(SKBitmap bitmap, Rectangle rectangle)
    {
        _displayList.Add(new BackendDrawCommand("SkiaSharp SKBitmap image"));
        _drawActions.Add((canvas) => canvas.DrawBitmap(bitmap, rectangle));
    }
    
}
