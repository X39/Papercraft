using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Canvas;

internal sealed class RelativeDeferredCanvas : IDeferredCanvas
{
    private readonly IDeferredCanvas _inner;
    private readonly Point _origin;

    public RelativeDeferredCanvas(IDeferredCanvas inner, Point origin, Size pageSize)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
        _origin = origin;
        ActualPageSize = pageSize;
        PageSize = pageSize;
    }

    public Size ActualPageSize { get; }

    public Size PageSize { get; }

    public Point Translation => _inner.Translation - _origin;

    public void PushState() => _inner.PushState();

    public void Clip(Rectangle rectangle) => _inner.Clip(rectangle);

    public void PopState() => _inner.PopState();

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
        => _inner.DrawLine(color, thickness, startX, startY, endX, endY);

    public void Translate(Point point) => _inner.Translate(point);

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
        => _inner.DrawText(textStyle, dpi, text, x, y);

    public void DrawLinkAnnotation(string uri, Rectangle rectangle)
        => _inner.DrawLinkAnnotation(uri, rectangle);

    public void DrawRect(Rectangle rectangle, Color color)
        => _inner.DrawRect(rectangle, color);

    public void DrawImage(byte[] bytes, Rectangle rectangle)
        => _inner.DrawImage(bytes, rectangle);

    public void DrawImage(ReadOnlyMemory<byte> bytes, Rectangle rectangle)
        => _inner.DrawImage(bytes, rectangle);

    public void DrawBitmap(byte[] bytes, Rectangle rectangle)
        => _inner.DrawBitmap(bytes, rectangle);

    public void Defer(Action<IImmediateCanvas> action)
        => _inner.Defer(action);
}
