using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Canvas;

internal sealed class DryRunDeferredCanvas : IDeferredCanvas
{
    private readonly Stack<Point> _stateStack = new();

    private DryRunDeferredCanvas(Size actualPageSize, Size pageSize, Point translation)
    {
        ActualPageSize = actualPageSize;
        PageSize = pageSize;
        _stateStack.Push(translation);
    }

    public Size ActualPageSize { get; }

    public Size PageSize { get; }

    public Point Translation => _stateStack.Peek();

    public static DryRunDeferredCanvas From(IDeferredCanvas canvas)
        => new(canvas.ActualPageSize, canvas.PageSize, canvas.Translation);

    public void PushState()
        => _stateStack.Push(Translation);

    public void Clip(Rectangle rectangle)
    {
    }

    public void PopState()
        => _stateStack.Pop();

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
    }

    public void Translate(Point point)
    {
        var current = _stateStack.Pop();
        _stateStack.Push(current + point);
    }

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
    {
    }

    public void DrawRect(Rectangle rectangle, Color color)
    {
    }

    public void DrawImage(byte[] image, Rectangle rectangle)
    {
    }

    public void DrawImage(ReadOnlyMemory<byte> image, Rectangle rectangle)
    {
    }

    public void DrawBitmap(byte[] bitmap, Rectangle rectangle)
    {
    }

    public void Defer(Action<IImmediateCanvas> action)
    {
    }
}
