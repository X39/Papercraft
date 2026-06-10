using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;

namespace X39.Solutions.PdfTemplate.Test.Rendering;

public sealed class SkiaTextDecorationRendererTests : IDisposable
{
    private const float Dpi = 90F;
    private readonly SkPaintCache _paintCache = new();

    public void Dispose()
    {
        _paintCache.Dispose();
    }

    [Fact]
    public void DoubleUnderlineCreatesTwoUnderlineLines()
    {
        var textStyle = new TextStyle
        {
            Foreground = Colors.Red,
            Decoration = TextDecoration.DoubleUnderline,
        };
        var paint = _paintCache.Get(textStyle, Dpi);

        var lines = SkiaTextDecorationRenderer.GetDecorationLines(textStyle, paint, 50F, 2F, 20F);

        Assert.Equal(2, lines.Count);
        Assert.All(lines, (line) => Assert.Equal(Colors.Red, line.Color));
        Assert.All(lines, (line) => Assert.Equal((2F, 52F), (line.StartX, line.EndX)));
        Assert.True(lines[1].StartY > lines[0].StartY);
    }

    [Fact]
    public void UnderlineAndStrikeThroughCreateIndependentLines()
    {
        var textStyle = new TextStyle
        {
            Foreground = Colors.Blue,
            Decoration = TextDecoration.Underline | TextDecoration.StrikeThrough,
        };
        var paint = _paintCache.Get(textStyle, Dpi);

        var lines = SkiaTextDecorationRenderer.GetDecorationLines(textStyle, paint, 40F, 1F, 18F);

        Assert.Equal(2, lines.Count);
        Assert.Contains(lines, (line) => line.StartY > 18F);
        Assert.Contains(lines, (line) => line.StartY < 18F);
    }
}
