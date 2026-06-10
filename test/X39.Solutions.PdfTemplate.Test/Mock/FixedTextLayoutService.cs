using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.PdfTemplate.Test.Mock;

internal sealed class FixedTextLayoutService : ITextLayoutService
{
    private readonly float _lineHeight;
    private readonly float _baselineOffset;
    private readonly float _width;

    public FixedTextLayoutService(float lineHeight = 10F, float baselineOffset = 8F, float width = 10F)
    {
        _lineHeight = lineHeight;
        _baselineOffset = baselineOffset;
        _width = width;
    }

    public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        var lines = Layout(textStyle, dpi, text, maxWidth);
        if (lines.Count is 0)
            return Size.Zero;

        return new Size(
            lines.Max((q) => q.Width),
            _lineHeight + (lines.Count - 1) * (_lineHeight * textStyle.LineHeight));
    }

    public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        foreach (var line in Layout(textStyle, dpi, text, maxWidth))
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
        if (text.IsEmpty)
            return Array.Empty<TextLineLayout>();

        var lines = text.ToString().Split('\n');
        var result = new TextLineLayout[lines.Length];
        for (var i = 0; i < lines.Length; i++)
        {
            var top = i * _lineHeight * textStyle.LineHeight;
            result[i] = new TextLineLayout(
                lines[i],
                0F,
                top + _baselineOffset,
                top,
                _lineHeight,
                _width);
        }

        return result;
    }
}
