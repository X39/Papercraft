using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services.TextService;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Rendering;

public sealed class SkiaTextServiceTests : IDisposable
{
    private const float Dpi = 90F;
    private readonly SkPaintCache _paintCache = new();

    public void Dispose()
    {
        _paintCache.Dispose();
    }

    [Theory]
    [InlineData(0.5F)]
    [InlineData(2F)]
    public void MeasureReturnsZeroForEmptyTextWithNonDefaultLineHeight(float lineHeight)
    {
        var textService = new TextService(_paintCache);
        var textStyle = new TextStyle
        {
            LineHeight = lineHeight,
        };

        var measured = textService.Measure(textStyle, Dpi, ReadOnlySpan<char>.Empty, 100F);

        Assert.Equal(Size.Zero, measured);
    }

    [Fact]
    public void LayoutAndDrawAreNoOpsForEmptyText()
    {
        var textService = new TextService(_paintCache);
        var textStyle = new TextStyle
        {
            LineHeight = 2F,
        };
        var canvas = new DeferredCanvasMock
        {
            ActualPageSize = new Size(100F, 100F),
            PageSize = new Size(100F, 100F),
        };

        var layout = textService.Layout(textStyle, Dpi, ReadOnlySpan<char>.Empty, 100F);
        textService.Draw(canvas, textStyle, Dpi, ReadOnlySpan<char>.Empty, 100F);

        Assert.Empty(layout);
        Assert.Equal(0, canvas.DrawTextCount);
        canvas.AssertState();
    }
}
