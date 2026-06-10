using SkiaSharp;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp.Canvas;

internal sealed class ImmediateCanvasImpl : IImmediateCanvas
{
    private readonly SkPaintCache _paintCache;
    private readonly SKCanvas     _canvas;

    public ImmediateCanvasImpl(SKCanvas canvas, SkPaintCache paintCache)
    {
        _canvas     = canvas;
        _paintCache = paintCache;
    }

    public void PushState()
        => _canvas.Save();

    public void Clip(Rectangle rectangle)
        => _canvas.ClipRect(rectangle.ToSkRect());

    public void PopState()
        => _canvas.Restore();

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
        => _canvas.DrawLine(startX, startY, endX, endY, _paintCache.Get(color, thickness));

    public void Translate(Point point)
        => _canvas.Translate(point.X, point.Y);

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
        => SkiaTextDecorationRenderer.DrawText(_canvas, _paintCache, textStyle, dpi, text, x, y);

    public void DrawRect(Rectangle rectangle, Color color)
        => _canvas.DrawRect(rectangle.ToSkRect(), _paintCache.Get(color));

    public void DrawImage(byte[] bytes, Rectangle rectangle)
    {
        using var stream = new MemoryStream(bytes);
        using var bitmap = SKBitmap.Decode(stream);
        if (bitmap is null)
            return;
        _canvas.DrawBitmap(bitmap, rectangle.ToSkRect());
    }

    public void DrawImage(ReadOnlyMemory<byte> bytes, Rectangle rectangle)
        => DrawImage(bytes.ToArray(), rectangle);

    public void DrawBitmap(byte[] bytes, Rectangle rectangle)
        => DrawImage(bytes, rectangle);

    [Obsolete("Use DrawImage(byte[], Rectangle) for renderer-neutral image drawing. This SkiaSharp-specific overload remains for compatibility.")]
    public void DrawBitmap(SKBitmap bitmap, Rectangle arrangementInner)
        => _canvas.DrawBitmap(bitmap, arrangementInner.ToSkRect());

    public ushort PageNumber { get; internal set; }
    public ushort TotalPages { get; internal set; }
}
