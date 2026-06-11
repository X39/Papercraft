using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Rendering.Svg.Services;

internal sealed class SvgTextService : ITextService
{
    public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        var fontSize = GetFontSize(textStyle, dpi);
        var lineHeight = GetLineHeight(textStyle, fontSize);
        var maxLineWidth = 0F;
        var lineCount = 0;
        foreach (var line in LayoutLines(textStyle, dpi, text, maxWidth))
        {
            maxLineWidth = Math.Max(maxLineWidth, MeasureLine(textStyle, dpi, line));
            lineCount++;
        }

        return lineCount is 0
            ? Size.Zero
            : new Size(maxLineWidth, lineCount * lineHeight);
    }

    public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        var fontSize = GetFontSize(textStyle, dpi);
        var lineHeight = GetLineHeight(textStyle, fontSize);
        var y = fontSize;
        foreach (var line in LayoutLines(textStyle, dpi, text, maxWidth))
        {
            canvas.DrawText(textStyle, dpi, line, 0F, y);
            y += lineHeight;
        }
    }

    private static IReadOnlyList<string> LayoutLines(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        var lines = new List<string>();
        foreach (var paragraph in text.ToString().Split('\n'))
        {
            lines.AddRange(WrapLine(textStyle, dpi, paragraph.Trim(), maxWidth));
        }

        return lines;
    }

    private static IReadOnlyList<string> WrapLine(TextStyle textStyle, float dpi, string text, float maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
            return lines;

        if (!float.IsFinite(maxWidth) || maxWidth <= 0F)
        {
            lines.Add(text);
            return lines;
        }

        var remaining = text;
        while (!string.IsNullOrWhiteSpace(remaining))
        {
            var end = remaining.Length;
            while (end > 1 && MeasureLine(textStyle, dpi, remaining.AsSpan(0, end)) > maxWidth)
            {
                var whitespace = LastWhitespaceBefore(remaining.AsSpan(), end);
                end = whitespace > 0
                    ? whitespace
                    : Math.Max(1, end - 1);
                if (whitespace <= 0)
                    break;
            }

            lines.Add(remaining[..end].Trim());
            remaining = remaining[end..].TrimStart();
        }

        return lines;
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

    private static float MeasureLine(TextStyle textStyle, float dpi, ReadOnlySpan<char> line)
        => line.Length * GetFontSize(textStyle, dpi) * GetAverageGlyphWidth(textStyle);

    private static float GetFontSize(TextStyle textStyle, float dpi)
        => Math.Max(1F, textStyle.FontSize * dpi / 72.272F * Math.Max(0.01F, textStyle.Scale));

    private static float GetLineHeight(TextStyle textStyle, float fontSize)
        => fontSize * Math.Max(0.1F, textStyle.LineHeight);

    private static float GetAverageGlyphWidth(TextStyle textStyle)
        => textStyle.FontFamily.Family.Contains("mono", StringComparison.OrdinalIgnoreCase)
            ? 0.6F
            : 0.55F;
}
