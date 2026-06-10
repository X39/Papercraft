using SkiaSharp;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;

/// <summary>
/// Renders Papercraft display-list commands onto a SkiaSharp canvas.
/// </summary>
public sealed class SkiaSharpDisplayListRenderer
{
    private readonly SkPaintCache _paintCache;

    /// <summary>
    /// Creates a new display-list renderer.
    /// </summary>
    /// <param name="paintCache">The Skia paint cache to use while rendering commands.</param>
    public SkiaSharpDisplayListRenderer(SkPaintCache paintCache)
    {
        ArgumentNullException.ThrowIfNull(paintCache);
        _paintCache = paintCache;
    }

    /// <summary>
    /// Renders all commands in a display list to the supplied Skia canvas.
    /// </summary>
    /// <param name="canvas">The Skia canvas to render to.</param>
    /// <param name="displayList">The display list to render.</param>
    public void Render(SKCanvas canvas, DisplayList displayList)
        => RenderCore(canvas, displayList, null);

    internal void Render(SKCanvas canvas, DisplayList displayList, SkiaImageDecodeCache imageDecodeCache)
        => RenderCore(canvas, displayList, imageDecodeCache);

    private void RenderCore(SKCanvas canvas, DisplayList displayList, SkiaImageDecodeCache? imageDecodeCache)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(displayList);

        using var activity = PapercraftActivity.Start(SkiaSharpActivityNames.DisplayListRender);
        activity?.SetTag(PapercraftActivity.DisplayListCommandCountTag, displayList.Commands.Count);
        try
        {
            foreach (var command in displayList.Commands)
            {
                RenderCommand(canvas, command, imageDecodeCache);
            }
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private void RenderCommand(SKCanvas canvas, DisplayCommand command, SkiaImageDecodeCache? imageDecodeCache)
    {
        switch (command)
        {
            case PushStateCommand:
                canvas.Save();
                break;
            case PopStateCommand:
                canvas.Restore();
                break;
            case TranslateCommand translate:
                canvas.Translate(translate.Offset.X, translate.Offset.Y);
                break;
            case ClipCommand clip:
                canvas.ClipRect(ToSkRect(clip.Rectangle));
                break;
            case DrawLineCommand line:
                canvas.DrawLine(
                    line.StartX,
                    line.StartY,
                    line.EndX,
                    line.EndY,
                    _paintCache.Get(ToColor(line.Color), line.Thickness));
                break;
            case DrawRectangleCommand rectangle:
                canvas.DrawRect(ToSkRect(rectangle.Rectangle), _paintCache.Get(ToColor(rectangle.Color)));
                break;
            case DrawTextCommand text:
                SkiaTextDecorationRenderer.DrawText(
                    canvas,
                    _paintCache,
                    ToTextStyle(text.TextStyle),
                    text.Dpi,
                    text.Text,
                    text.X,
                    text.Y);
                break;
            case LinkAnnotationCommand link:
                canvas.DrawUrlAnnotation(ToSkRect(link.Rectangle), link.Uri);
                break;
            case DrawImageCommand image:
                DrawImage(canvas, image, imageDecodeCache);
                break;
        }
    }

    private static void DrawImage(SKCanvas canvas, DrawImageCommand image, SkiaImageDecodeCache? imageDecodeCache)
    {
        if (imageDecodeCache is not null)
        {
            var cachedBitmap = imageDecodeCache.GetOrDecode(image.Bytes);
            if (cachedBitmap is not null)
                canvas.DrawBitmap(cachedBitmap, ToSkRect(image.Rectangle));
            return;
        }

        using var stream = new MemoryStream(image.Bytes);
        using var bitmap = SKBitmap.Decode(stream);
        if (bitmap is null)
            return;
        canvas.DrawBitmap(bitmap, ToSkRect(image.Rectangle));
    }

    private static SKRect ToSkRect(DisplayRectangle rectangle)
        => new(
            rectangle.Left,
            rectangle.Top,
            rectangle.Left + rectangle.Width,
            rectangle.Top + rectangle.Height);

    private static Color ToColor(DisplayColor color)
        => new(color.Red, color.Green, color.Blue, color.Alpha);

    private static TextStyle ToTextStyle(DisplayTextStyle textStyle)
        => new()
        {
            Foreground = ToColor(textStyle.Foreground),
            FontSize = textStyle.FontSize,
            FontFamily = ToFont(textStyle.FontFamily),
            Scale = textStyle.Scale,
            LineHeight = textStyle.LineHeight,
            Rotation = textStyle.Rotation,
            StrokeThickness = textStyle.StrokeThickness,
            Decoration = textStyle.Decoration,
        };

    private static Font ToFont(DisplayFont font)
        => new(font.Family)
        {
            LetterSpacing = font.LetterSpacing,
            Weight = font.Weight,
            Style = font.Style switch
            {
                DisplayFontStyle.Italic => EFontStyle.Italic,
                DisplayFontStyle.Oblique => EFontStyle.Oblique,
                _ => EFontStyle.Upright,
            },
        };
}
