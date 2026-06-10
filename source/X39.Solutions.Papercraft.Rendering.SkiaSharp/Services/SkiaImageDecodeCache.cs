using SkiaSharp;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;

internal sealed class SkiaImageDecodeCache : IDisposable
{
    private readonly Dictionary<byte[], SKBitmap> _bitmaps = new(ReferenceEqualityComparer.Instance);

    public SKBitmap? GetOrDecode(byte[] bytes)
    {
        if (_bitmaps.TryGetValue(bytes, out var cached))
            return cached;

        using var stream = new MemoryStream(bytes);
        var bitmap = SKBitmap.Decode(stream);
        if (bitmap is null)
            return null;

        _bitmaps.Add(bytes, bitmap);
        return bitmap;
    }

    public void Dispose()
    {
        foreach (var bitmap in _bitmaps.Values)
        {
            bitmap.Dispose();
        }

        _bitmaps.Clear();
    }
}
