using System.Globalization;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services.ResourceResolver;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class ImageControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(200, 100);

    [Fact]
    public async Task RenderDrawsBitmapIntoArrangedSize()
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
        canvas.AssertDrawBitmap(new Rectangle(0, 0, 40, 20));
    }

    private static byte[] CreatePngBytes()
    {
        using var bitmap = new SKBitmap(4, 2);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Blue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private sealed class FixedImageResourceResolver(byte[] bytes) : IResourceResolver
    {
        public ValueTask<byte[]> ResolveImageAsync(
            string source,
            object? context,
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult(bytes);
    }
}
