using SkiaSharp;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp.Abstraction;

/// <summary>
/// Contains SkiaSharp compatibility extensions for <see cref="IDrawableCanvas"/>.
/// </summary>
public static class SkiaSharpCanvasCompatibilityExtensions
{
    /// <summary>
    ///     Draws a SkiaSharp bitmap on the canvas.
    /// </summary>
    /// <param name="canvas">The <see cref="IDrawableCanvas"/> to draw on.</param>
    /// <param name="bitmap">The SkiaSharp bitmap to draw.</param>
    /// <param name="rectangle">The region to draw the bitmap into.</param>
    [Obsolete("Use DrawImage(byte[], Rectangle) for renderer-neutral image drawing. This SkiaSharp-specific overload remains for compatibility.")]
    public static void DrawBitmap(this IDrawableCanvas canvas, SKBitmap bitmap, Rectangle rectangle)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(bitmap);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        if (data is null)
            return;

        canvas.DrawImage(data.ToArray(), rectangle);
    }
}
