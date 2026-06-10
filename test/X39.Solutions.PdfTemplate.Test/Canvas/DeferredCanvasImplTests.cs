using SkiaSharp;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Abstraction;

namespace X39.Solutions.PdfTemplate.Test.Canvas;

public sealed class DeferredCanvasImplTests
{
    [Fact]
    public void DrawImageRecordsRendererNeutralDisplayCommand()
    {
        var canvas = new DeferredCanvasImpl();
        var bytes = new byte[] { 1, 2, 3 };
        var rectangle = new Rectangle(1, 2, 3, 4);

        canvas.DrawImage(bytes, rectangle);

        var command = Assert.IsType<DrawImageCommand>(Assert.Single(canvas.DisplayList.Commands));
        Assert.Same(bytes, command.Bytes);
        Assert.Equal(new DisplayRectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height), command.Rectangle);
    }

    [Fact]
    public void DrawBitmapByteArrayRecordsRendererNeutralDisplayCommand()
    {
        var canvas = new DeferredCanvasImpl();
        var bytes = new byte[] { 1, 2, 3 };
        var rectangle = new Rectangle(1, 2, 3, 4);

        canvas.DrawBitmap(bytes, rectangle);

        var command = Assert.IsType<DrawImageCommand>(Assert.Single(canvas.DisplayList.Commands));
        Assert.Same(bytes, command.Bytes);
        Assert.Equal(new DisplayRectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height), command.Rectangle);
    }

    [Fact]
    public void SkiaSharpDrawBitmapExtensionRecordsRendererNeutralDisplayCommand()
    {
        IDrawableCanvas canvas = new DeferredCanvasImpl();
        using var bitmap = new SKBitmap(2, 1);
        bitmap.Erase(SKColors.Red);
        var rectangle = new Rectangle(1, 2, 3, 4);

#pragma warning disable CS0618
        canvas.DrawBitmap(bitmap, rectangle);
#pragma warning restore CS0618

        var deferredCanvas = Assert.IsType<DeferredCanvasImpl>(canvas);
        var command = Assert.IsType<DrawImageCommand>(Assert.Single(deferredCanvas.DisplayList.Commands));
        Assert.NotEmpty(command.Bytes);
        Assert.Equal(new DisplayRectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height), command.Rectangle);
    }

    [Fact]
    public void DrawTextRecordsRendererNeutralTextStyle()
    {
        var canvas = new DeferredCanvasImpl();
        var textStyle = new TextStyle
        {
            Foreground = new Color(1, 2, 3, 4),
            FontFamily = new Font("Test")
            {
                LetterSpacing = (ushort) 10,
                Weight = (ushort) 700,
                Style = EFontStyle.Oblique,
            },
            FontSize = 14,
            Scale = 2,
            LineHeight = 3,
            Rotation = 4,
            StrokeThickness = 5,
            Decoration = TextDecoration.Underline | TextDecoration.StrikeThrough,
        };

        canvas.DrawText(textStyle, 96, "Text", 7, 8);

        var command = Assert.IsType<DrawTextCommand>(Assert.Single(canvas.DisplayList.Commands));
        Assert.Equal(new DisplayColor(1, 2, 3, 4), command.TextStyle.Foreground);
        Assert.Equal(14F, command.TextStyle.FontSize);
        Assert.Equal(new DisplayFont("Test")
        {
            LetterSpacing = 10,
            Weight = 700,
            Style = DisplayFontStyle.Oblique,
        }, command.TextStyle.FontFamily);
        Assert.Equal(2F, command.TextStyle.Scale);
        Assert.Equal(3F, command.TextStyle.LineHeight);
        Assert.Equal(4F, command.TextStyle.Rotation);
        Assert.Equal(5F, command.TextStyle.StrokeThickness);
        Assert.Equal(TextDecoration.Underline | TextDecoration.StrikeThrough, command.TextStyle.Decoration);
    }
}
