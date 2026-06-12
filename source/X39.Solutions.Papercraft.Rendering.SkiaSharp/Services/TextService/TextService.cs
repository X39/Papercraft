using SkiaSharp;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp.Services.TextService;

internal class TextService : ITextLayoutService
{
    private readonly SkPaintCache _paintCache;
    public TextService(SkPaintCache paintCache)
    {
        _paintCache = paintCache;
    }
    private ref struct ReadOnlySpanPair<T>
    {
        public ReadOnlySpan<T> Left;
        public ReadOnlySpan<T> Right;

        public ReadOnlySpanPair(ReadOnlySpan<T> left, ReadOnlySpan<T> right)
        {
            Left  = left;
            Right = right;
        }

        public void Deconstruct(out ReadOnlySpan<T> left, out ReadOnlySpan<T> right)
        {
            left  = Left;
            right = Right;
        }
    }

    public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        var skPaint = _paintCache.Get(textStyle, dpi);
        var lines = Layout(textStyle, skPaint, text, maxWidth, out var height);
        if (lines.Count is 0)
            return Size.Zero;

        var resultWidth = lines
            .Select((q) => q.Width)
            .DefaultIfEmpty()
            .Max();
        return new Size(resultWidth, height + (lines.Count - 1) * (height * textStyle.LineHeight));
    }
    private static ReadOnlySpanPair<char> NextLine(ReadOnlySpan<char> text)
    {
        var index = text.IndexOf('\n');
        return index == -1
            ? new ReadOnlySpanPair<char>(text, ReadOnlySpan<char>.Empty)
            : new ReadOnlySpanPair<char>(text[..index], text[(index + 1)..]);
    }

    private static ReadOnlySpanPair<char> DivideAndConquer(ReadOnlySpan<char> text, SKPaint skPaint, float maxWidth, out float leftWidth)
    {
        if (text.IsEmpty)
        {
            leftWidth = 0F;
            return new ReadOnlySpanPair<char>(ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty);
        }

        var end = text.Length;
        leftWidth = skPaint.MeasureText(text);
        while (leftWidth > maxWidth)
        {
            var breakIndex = LastWhitespaceBefore(text, end);
            if (breakIndex < 0)
            {
                end       = FirstTokenEnd(text);
                leftWidth = ConstrainLineWidth(skPaint.MeasureText(text[..end]), maxWidth);
                return new ReadOnlySpanPair<char>(text[..end], text[end..]);
            }

            end       = breakIndex;
            leftWidth = skPaint.MeasureText(text[..end]);
        }
        return new ReadOnlySpanPair<char>(text[..end], text[end..]);
    }

    private static int LastWhitespaceBefore(ReadOnlySpan<char> text, int endExclusive)
    {
        for (var i = Math.Min(endExclusive, text.Length) - 1; i > 0; i--)
        {
            if (char.IsWhiteSpace(text[i]))
                return i;
        }

        return -1;
    }

    private static int FirstTokenEnd(ReadOnlySpan<char> text)
    {
        var isWhiteSpace = char.IsWhiteSpace(text[0]);
        for (var i = 1; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]) != isWhiteSpace)
                return i;
        }

        return text.Length;
    }

    private static float ConstrainLineWidth(float width, float maxWidth)
    {
        if (float.IsNaN(maxWidth))
            return width;

        return Math.Max(0F, Math.Min(width, maxWidth));
    }

    public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        var skPaint = _paintCache.Get(textStyle, dpi);
        foreach (var line in Layout(textStyle, skPaint, text, maxWidth, out _))
        {
            canvas.DrawText(textStyle, dpi, line.Text, line.X, line.BaselineY);
        }
    }

    public IReadOnlyList<TextLineLayout> Layout(
        TextStyle textStyle,
        float dpi,
        ReadOnlySpan<char> text,
        float maxWidth)
    {
        var skPaint = _paintCache.Get(textStyle, dpi);
        return Layout(textStyle, skPaint, text, maxWidth, out _);
    }

    private static IReadOnlyList<TextLineLayout> Layout(
        TextStyle textStyle,
        SKPaint skPaint,
        ReadOnlySpan<char> text,
        float maxWidth,
        out float height)
    {
        height = skPaint.FontMetrics.Bottom
                 + -skPaint.FontMetrics.Top
                 + SkiaTextDecorationRenderer.GetDecorationExtraHeight(textStyle, skPaint);
        var right = text;
        var left = text;
        var y = 0F;
        var lines = new List<TextLineLayout>();
        while (!right.IsEmpty && !left.IsEmpty)
        {
            var (fullLine, remainder) = NextLine(right);
            var trimmedFullLine                  = fullLine.TrimStart();
            if (fullLine.IsEmpty && !remainder.IsEmpty)
            { // new line character
                right = remainder;
                continue;
            }
            var (divided, _) = DivideAndConquer(trimmedFullLine, skPaint, maxWidth, out var width);
            left             = divided;
            right            = right[(left.Length + fullLine.Length - trimmedFullLine.Length)..];
            var lineText = left.ToString();
            var baselineY = y - skPaint.FontMetrics.Ascent;
            var (lineTop, lineHeight) = GetRenderedLineBounds(textStyle, skPaint, lineText, width, baselineY);
            lines.Add(
                new TextLineLayout(
                    lineText,
                    0F,
                    baselineY,
                    lineTop,
                    lineHeight,
                    width));
            y += height * textStyle.LineHeight;
        }

        return lines;
    }

    private static (float Top, float Height) GetRenderedLineBounds(
        TextStyle textStyle,
        SKPaint skPaint,
        string text,
        float width,
        float baselineY)
    {
        var textBounds = new SKRect();
        _ = skPaint.MeasureText(text, ref textBounds);
        var top = baselineY + textBounds.Top;
        var bottom = baselineY + textBounds.Bottom;

        foreach (var decorationLine in SkiaTextDecorationRenderer.GetDecorationLines(
                     textStyle,
                     skPaint,
                     width,
                     0F,
                     baselineY))
        {
            var halfThickness = decorationLine.Thickness / 2F;
            top = Math.Min(top, decorationLine.StartY - halfThickness);
            bottom = Math.Max(bottom, decorationLine.StartY + halfThickness);
        }

        return bottom > top
            ? (top, bottom - top)
            : (baselineY, 0F);
    }
}
