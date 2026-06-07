using System.Globalization;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.ResourceResolver;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class ImageControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(200, 100);

    [Fact]
    public async Task RenderDrawsImageIntoArrangedSize()
    {
        using var control = new ImageControl(new FixedImageResourceResolver(CreatePngBytes()))
        {
            Source = "logo",
            Width = new Length(40F, ELengthUnit.Pixel),
            Height = new Length(20F, ELengthUnit.Pixel),
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        var canvas = new DeferredCanvasMock {ActualPageSize = PageSize, PageSize = PageSize};

        await control.InitializeControlAsync(null);
        Assert.Equal(new Size(40, 20), control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture));
        Assert.Equal(new Size(40, 20), control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture));
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        canvas.AssertState();
        canvas.AssertClip(new Rectangle(0, 0, 40, 20));
        canvas.AssertDrawImage(new Rectangle(0, 0, 40, 20));
    }

    [Fact]
    public async Task MeasureUsesPngDimensionsWhenWidthAndHeightAreAuto()
    {
        using var control = new ImageControl(new FixedImageResourceResolver(CreatePngBytes()))
        {
            Source = "logo",
        };

        await control.InitializeControlAsync(null);

        Assert.Equal(new Size(4, 2), control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture));
    }

    private static byte[] CreatePngBytes()
        => Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAQAAAACCAIAAADwyuo0AAAAJUlEQVR4AQEaAOX/AAAA/wAA/wAA/wAA/wAAAP8AAP8AAP8AAP9fugf5AHw6kwAAAABJRU5ErkJggg==");

    private sealed class FixedImageResourceResolver(byte[] bytes) : IResourceResolver
    {
        public ValueTask<byte[]> ResolveImageAsync(
            string source,
            object? context,
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult(bytes);
    }
}
