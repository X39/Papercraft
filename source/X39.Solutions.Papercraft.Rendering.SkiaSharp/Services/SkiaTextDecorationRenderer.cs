using SkiaSharp;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;

internal static class SkiaTextDecorationRenderer
{
    public readonly record struct DecorationLine(
        Color Color,
        float Thickness,
        float StartX,
        float StartY,
        float EndX,
        float EndY);

    public static void DrawText(
        SKCanvas canvas,
        SkPaintCache paintCache,
        TextStyle textStyle,
        float dpi,
        string text,
        float x,
        float y)
    {
        var textPaint = paintCache.GetText(textStyle, dpi);
        canvas.DrawText(text, x, y, textPaint.Font, textPaint.Paint);

        foreach (var line in GetDecorationLines(textStyle, textPaint, text, x, y))
        {
            canvas.DrawLine(
                line.StartX,
                line.StartY,
                line.EndX,
                line.EndY,
                paintCache.Get(line.Color, line.Thickness));
        }
    }

    public static IReadOnlyList<DecorationLine> GetDecorationLines(
        TextStyle textStyle,
        SkTextPaint textPaint,
        string text,
        float x,
        float y)
        => GetDecorationLines(textStyle, textPaint, textPaint.Font.MeasureText(text, textPaint.Paint), x, y);

    public static IReadOnlyList<DecorationLine> GetDecorationLines(
        TextStyle textStyle,
        SkTextPaint textPaint,
        float width,
        float x,
        float y)
    {
        if (textStyle.Decoration is TextDecoration.None || width <= 0F || textStyle.Foreground.Alpha is 0)
            return [];

        var decorations = textStyle.Decoration;
        var metrics = textPaint.Font.Metrics;
        var lines = new List<DecorationLine>(3);

        if (decorations.HasFlag(TextDecoration.DoubleUnderline))
        {
            AddUnderline(lines, textStyle, textPaint, metrics, width, x, y, isSecondLine: false);
            AddUnderline(lines, textStyle, textPaint, metrics, width, x, y, isSecondLine: true);
        }
        else if (decorations.HasFlag(TextDecoration.Underline))
        {
            AddUnderline(lines, textStyle, textPaint, metrics, width, x, y, isSecondLine: false);
        }

        if (decorations.HasFlag(TextDecoration.StrikeThrough))
            AddStrikeThrough(lines, textStyle, textPaint, metrics, width, x, y);

        return lines;
    }

    public static float GetDecorationExtraHeight(TextStyle textStyle, SkTextPaint textPaint)
    {
        if (textStyle.Decoration is TextDecoration.None)
            return 0F;

        var metrics = textPaint.Font.Metrics;
        var requiredBottom = metrics.Bottom;
        if (textStyle.Decoration.HasFlag(TextDecoration.Underline)
            || textStyle.Decoration.HasFlag(TextDecoration.DoubleUnderline))
        {
            var thickness = GetUnderlineThickness(metrics, textPaint);
            var underlineBottom = GetUnderlineTop(metrics) + thickness;
            if (textStyle.Decoration.HasFlag(TextDecoration.DoubleUnderline))
                underlineBottom += GetDoubleUnderlineGap(thickness, textPaint) + thickness;
            requiredBottom = Math.Max(requiredBottom, underlineBottom);
        }

        return Math.Max(0F, requiredBottom - metrics.Bottom);
    }

    private static void AddUnderline(
        ICollection<DecorationLine> lines,
        TextStyle textStyle,
        SkTextPaint textPaint,
        SKFontMetrics metrics,
        float width,
        float x,
        float y,
        bool isSecondLine)
    {
        var thickness = GetUnderlineThickness(metrics, textPaint);
        var lineY = y + GetUnderlineTop(metrics) + thickness / 2F;
        if (isSecondLine)
            lineY += thickness + GetDoubleUnderlineGap(thickness, textPaint);

        lines.Add(new DecorationLine(textStyle.Foreground, thickness, x, lineY, x + width, lineY));
    }

    private static void AddStrikeThrough(
        ICollection<DecorationLine> lines,
        TextStyle textStyle,
        SkTextPaint textPaint,
        SKFontMetrics metrics,
        float width,
        float x,
        float y)
    {
        var thickness = GetStrikeThroughThickness(metrics, textPaint);
        var strikeBottom = metrics.StrikeoutPosition ?? metrics.Ascent * 0.35F;
        var lineY = y + strikeBottom - thickness / 2F;
        lines.Add(new DecorationLine(textStyle.Foreground, thickness, x, lineY, x + width, lineY));
    }

    private static float GetUnderlineTop(SKFontMetrics metrics)
        => metrics.UnderlinePosition ?? Math.Max(1F, metrics.Descent * 0.5F);

    private static float GetUnderlineThickness(SKFontMetrics metrics, SkTextPaint textPaint)
        => GetDecorationThickness(metrics.UnderlineThickness, textPaint);

    private static float GetStrikeThroughThickness(SKFontMetrics metrics, SkTextPaint textPaint)
        => GetDecorationThickness(metrics.StrikeoutThickness, textPaint);

    private static float GetDecorationThickness(float? metricThickness, SkTextPaint textPaint)
        => Math.Max(1F, metricThickness.GetValueOrDefault(textPaint.Font.Size / 14F));

    private static float GetDoubleUnderlineGap(float thickness, SkTextPaint textPaint)
        => Math.Max(thickness, textPaint.Font.Size * 0.08F);
}
