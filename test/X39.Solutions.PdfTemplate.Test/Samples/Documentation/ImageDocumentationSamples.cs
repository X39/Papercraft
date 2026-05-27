using SkiaSharp;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class ImageDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Image_FromTemplateData()
        => RenderDocumentationSampleAsync(
            "image-from-template-data",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <image
                        source="@LogoImage"
                        width="32mm"
                        height="18mm"
                        horizontalAlignment="left"
                        verticalAlignment="top"/>
                </body>
            </template>
            """,
            configureGenerator: (generator) =>
                generator.TemplateData.SetVariable("LogoImage", CreateLogoDataUri()));

    private static string CreateLogoDataUri()
    {
        using var bitmap = new SKBitmap(160, 90);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(new SKColor(219, 234, 254));

        using var bluePaint = new SKPaint { Color = new SKColor(37, 99, 235), IsAntialias = true };
        using var greenPaint = new SKPaint { Color = new SKColor(22, 163, 74), IsAntialias = true };
        using var darkPaint = new SKPaint { Color = new SKColor(15, 23, 42), IsAntialias = true };

        canvas.DrawRect(new SKRect(0, 0, 160, 22), darkPaint);
        canvas.DrawCircle(44, 54, 22, bluePaint);
        canvas.DrawRoundRect(new SKRect(78, 36, 138, 72), 8, 8, greenPaint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return $"data:image/png;base64,{Convert.ToBase64String(data.ToArray())}";
    }
}
