using PdfSharp.Drawing;
using PdfSharp.Pdf;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

internal sealed class PdfSharpDisplayListRenderer
{
    public void Render(XGraphics graphics, PdfPage page, DisplayList displayList)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(displayList);

        var state = new RenderState(page.Height.Point);
        foreach (var command in displayList.Commands)
        {
            RenderCommand(graphics, page, command, state);
        }
    }

    private static void RenderCommand(
        XGraphics graphics,
        PdfPage page,
        DisplayCommand command,
        RenderState state)
    {
        switch (command)
        {
            case PushStateCommand:
                graphics.Save();
                state.Push();
                break;
            case PopStateCommand:
                if (state.CanPop)
                {
                    graphics.Restore();
                    state.Pop();
                }
                break;
            case TranslateCommand translate:
                state.Translate(translate.Offset.X, translate.Offset.Y);
                break;
            case ClipCommand clip:
                graphics.IntersectClip(ToXRect(TransformRectangle(clip.Rectangle, state)));
                break;
            case DrawLineCommand line:
                DrawLine(graphics, line, state);
                break;
            case DrawRectangleCommand rectangle:
                DrawRectangle(graphics, rectangle, state);
                break;
            case DrawTextCommand text:
                DrawText(graphics, text, state);
                break;
            case DrawImageCommand image:
                DrawImage(graphics, image, state);
                break;
            case LinkAnnotationCommand link:
                AddLinkAnnotation(page, link, state);
                break;
        }
    }

    private static void DrawLine(
        XGraphics graphics,
        DrawLineCommand line,
        RenderState state)
    {
        var pen = new XPen(ToXColor(line.Color), Math.Max(0D, line.Thickness));
        graphics.DrawLine(
            pen,
            line.StartX + state.TranslateX,
            line.StartY + state.TranslateY,
            line.EndX + state.TranslateX,
            line.EndY + state.TranslateY);
    }

    private static void DrawRectangle(
        XGraphics graphics,
        DrawRectangleCommand rectangle,
        RenderState state)
    {
        if (rectangle.Color.Alpha is 0)
            return;

        var brush = new XSolidBrush(ToXColor(rectangle.Color));
        graphics.DrawRectangle(brush, ToXRect(TransformRectangle(rectangle.Rectangle, state)));
    }

    private static void DrawText(
        XGraphics graphics,
        DrawTextCommand text,
        RenderState state)
    {
        if (string.IsNullOrEmpty(text.Text) || text.TextStyle.Foreground.Alpha is 0)
            return;

        var font = CreateFont(text.TextStyle, text.Dpi);
        var brush = new XSolidBrush(ToXColor(text.TextStyle.Foreground));
        var x = text.X + state.TranslateX;
        var y = text.Y + state.TranslateY;
        if (NeedsTextTransform(text.TextStyle))
        {
            var graphicsState = graphics.Save();
            try
            {
                graphics.TranslateTransform(x, y);
                if (Math.Abs(text.TextStyle.Rotation) > float.Epsilon)
                    graphics.RotateTransform(text.TextStyle.Rotation);
                if (Math.Abs(text.TextStyle.Scale - 1F) > float.Epsilon)
                    graphics.ScaleTransform(text.TextStyle.Scale, 1D);

                graphics.DrawString(text.Text, font, brush, 0D, 0D);
            }
            finally
            {
                graphics.Restore(graphicsState);
            }

            return;
        }

        graphics.DrawString(text.Text, font, brush, x, y);
    }

    private static void DrawImage(
        XGraphics graphics,
        DrawImageCommand image,
        RenderState state)
    {
        if (image.Bytes.Length is 0 || image.Rectangle.Width <= 0F || image.Rectangle.Height <= 0F)
            return;

        using var stream = new MemoryStream(image.Bytes, writable: false);
        try
        {
            using var xImage = XImage.FromStream(stream);
            graphics.DrawImage(xImage, ToXRect(TransformRectangle(image.Rectangle, state)));
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }
    }

    private static void AddLinkAnnotation(PdfPage page, LinkAnnotationCommand link, RenderState state)
    {
        if (string.IsNullOrWhiteSpace(link.Uri)
            || link.Rectangle.Width <= 0F
            || link.Rectangle.Height <= 0F)
        {
            return;
        }

        page.AddWebLink(ToPdfRectangle(link.Rectangle, state), link.Uri);
    }

    private static XFont CreateFont(DisplayTextStyle textStyle, float dpi)
        => new(
            NormalizeFontFamily(textStyle.FontFamily.Family),
            GetFontSize(textStyle.FontSize, dpi),
            ToFontStyle(textStyle));

    private static double GetFontSize(float fontSize, float dpi)
    {
        var normalized = dpi > 0F
            ? fontSize * dpi / 72.272F
            : fontSize;
        return Math.Max(1D, normalized);
    }

    private static bool NeedsTextTransform(DisplayTextStyle textStyle)
        => Math.Abs(textStyle.Rotation) > float.Epsilon
           || Math.Abs(textStyle.Scale - 1F) > float.Epsilon;

    private static XFontStyleEx ToFontStyle(DisplayTextStyle textStyle)
    {
        var style = XFontStyleEx.Regular;
        if (textStyle.FontFamily.Weight >= 600)
            style |= XFontStyleEx.Bold;
        if (textStyle.FontFamily.Style is DisplayFontStyle.Italic or DisplayFontStyle.Oblique)
            style |= XFontStyleEx.Italic;
        if (textStyle.Decoration.HasFlag(TextDecoration.Underline)
            || textStyle.Decoration.HasFlag(TextDecoration.DoubleUnderline))
            style |= XFontStyleEx.Underline;
        if (textStyle.Decoration.HasFlag(TextDecoration.StrikeThrough))
            style |= XFontStyleEx.Strikeout;
        return style;
    }

    private static string NormalizeFontFamily(string family)
    {
        if (string.Equals(family, "sans-serif", StringComparison.OrdinalIgnoreCase))
            return "Arial";
        if (string.Equals(family, "serif", StringComparison.OrdinalIgnoreCase))
            return "Times New Roman";
        if (string.Equals(family, "monospace", StringComparison.OrdinalIgnoreCase)
            || string.Equals(family, "mono", StringComparison.OrdinalIgnoreCase))
            return "Courier New";
        return string.IsNullOrWhiteSpace(family)
            ? "Arial"
            : family;
    }

    private static XRect ToXRect(DisplayRectangle rectangle)
        => new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);

    private static DisplayRectangle TransformRectangle(DisplayRectangle rectangle, RenderState state)
        => new(
            (float)(rectangle.Left + state.TranslateX),
            (float)(rectangle.Top + state.TranslateY),
            rectangle.Width,
            rectangle.Height);

    private static PdfRectangle ToPdfRectangle(DisplayRectangle rectangle, RenderState state)
    {
        var left = rectangle.Left + state.TranslateX;
        var top = rectangle.Top + state.TranslateY;
        var right = left + rectangle.Width;
        var bottom = top + rectangle.Height;
        return new PdfRectangle(
            new XPoint(left, state.PageHeight - bottom),
            new XPoint(right, state.PageHeight - top));
    }

    private static XColor ToXColor(DisplayColor color)
        => XColor.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

    private sealed class RenderState
    {
        private readonly Stack<(double TranslateX, double TranslateY)> _stack = new();

        public RenderState(double pageHeight)
        {
            PageHeight = pageHeight;
        }

        public double PageHeight { get; }

        public double TranslateX { get; private set; }

        public double TranslateY { get; private set; }

        public bool CanPop => _stack.Count > 0;

        public void Push()
            => _stack.Push((TranslateX, TranslateY));

        public void Pop()
        {
            var restored = _stack.Pop();
            TranslateX = restored.TranslateX;
            TranslateY = restored.TranslateY;
        }

        public void Translate(double x, double y)
        {
            TranslateX += x;
            TranslateY += y;
        }
    }
}
