using SkiaSharp;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;

internal static class SkiaSharpConversions
{
    public static SKColor ToSkColor(this Color color)
        => new(color.Red, color.Green, color.Blue, color.Alpha);

    public static SKRect ToSkRect(this Rectangle rectangle)
        => new(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
}
