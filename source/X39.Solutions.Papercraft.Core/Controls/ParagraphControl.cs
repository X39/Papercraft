using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Rich text paragraph with inline spans and explicit line breaks.
/// </summary>
[Control(Constants.ControlsNamespace, "paragraph")]
public sealed class ParagraphControl : AlignableContentControl
{
    private readonly ITextService _textService;
    private RichTextLayout? _arrangedLayout;

    /// <summary>
    /// Creates a new instance of <see cref="ParagraphControl"/>.
    /// </summary>
    /// <param name="textService">The text service used to measure and render inline text.</param>
    [ControlConstructor]
    public ParagraphControl(ITextService textService)
    {
        _textService = textService;
    }

    /// <summary>
    /// The paragraph-level text style inherited by spans.
    /// </summary>
    private TextStyle TextStyle { get; set; } = new();

    /// <summary>
    /// The foreground color of the text.
    /// </summary>
    [Parameter]
    public Color Foreground
    {
        get => TextStyle.Foreground;
        set => TextStyle = TextStyle with { Foreground = value };
    }

    /// <summary>
    /// The size of the text.
    /// </summary>
    [Parameter]
    public float FontSize
    {
        get => TextStyle.FontSize;
        set => TextStyle = TextStyle with { FontSize = value };
    }

    /// <summary>
    /// The height of a line.
    /// </summary>
    [Parameter]
    public float LineHeight
    {
        get => TextStyle.LineHeight;
        set => TextStyle = TextStyle with { LineHeight = value };
    }

    /// <summary>
    /// The scale of the text.
    /// </summary>
    [Parameter]
    public float Scale
    {
        get => TextStyle.Scale;
        set => TextStyle = TextStyle with { Scale = value };
    }

    /// <summary>
    /// The rotation of the text.
    /// </summary>
    [Parameter]
    public float Rotation
    {
        get => TextStyle.Rotation;
        set => TextStyle = TextStyle with { Rotation = value };
    }

    /// <summary>
    /// The thickness of the stroke for the <see cref="Foreground"/> color.
    /// </summary>
    [Parameter]
    public float StrokeThickness
    {
        get => TextStyle.StrokeThickness;
        set => TextStyle = TextStyle with { StrokeThickness = value };
    }

    /// <summary>
    /// Decorations applied to the text.
    /// </summary>
    [Parameter]
    public TextDecoration Decoration
    {
        get => TextStyle.Decoration;
        set => TextStyle = TextStyle with { Decoration = value };
    }

    /// <summary>
    /// The width or letter-spacing of the font.
    /// </summary>
    [Parameter]
    public FontWidth LetterSpacing
    {
        get => TextStyle.FontFamily.LetterSpacing;
        set => TextStyle = TextStyle with
        {
            FontFamily = TextStyle.FontFamily with { LetterSpacing = value },
        };
    }

    /// <summary>
    /// The weight of the font.
    /// </summary>
    [Parameter]
    public FontWeight Weight
    {
        get => TextStyle.FontFamily.Weight;
        set => TextStyle = TextStyle with
        {
            FontFamily = TextStyle.FontFamily with { Weight = value },
        };
    }

    /// <summary>
    /// The style of the font.
    /// </summary>
    [Parameter]
    public EFontStyle Style
    {
        get => TextStyle.FontFamily.Style;
        set => TextStyle = TextStyle with
        {
            FontFamily = TextStyle.FontFamily with { Style = value },
        };
    }

    /// <summary>
    /// The font family.
    /// </summary>
    [Parameter]
    public string FontFamily
    {
        get => TextStyle.FontFamily.Family;
        set => TextStyle = TextStyle with
        {
            FontFamily = TextStyle.FontFamily with { Family = value },
        };
    }

    /// <inheritdoc />
    public override void Add(IControl item)
    {
        if (item is not SpanControl and not BrControl)
            throw new ArgumentException("Only SpanControl and BrControl can be added to a paragraph.", nameof(item));

        _arrangedLayout = null;
        base.Add(item);
    }

    /// <inheritdoc />
    public override void Clear()
    {
        _arrangedLayout = null;
        base.Clear();
    }

    /// <inheritdoc />
    public override bool Remove(IControl item)
    {
        var removed = base.Remove(item);
        if (removed)
            _arrangedLayout = null;
        return removed;
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type)
        => type.IsEquivalentTo(typeof(SpanControl))
           || type.IsEquivalentTo(typeof(BrControl));

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => Layout(dpi, remainingSize.Width).Size;

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        _arrangedLayout = Layout(dpi, remainingSize.Width);
        return _arrangedLayout.Size;
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var layout = _arrangedLayout ?? Layout(dpi, ArrangementInner.Width > 0F ? ArrangementInner.Width : parentSize.Width);
        foreach (var run in layout.Runs)
        {
            using var state = canvas.CreateState();
            canvas.Translate(run.X, run.Y);
            _textService.Draw(canvas, run.TextStyle, dpi, run.Text.AsSpan(), float.MaxValue);
        }

        return Size.Zero;
    }

    internal TextStyle GetTextStyle() => TextStyle;

    private RichTextLayout Layout(float dpi, float maxWidth)
    {
        var effectiveMaxWidth = maxWidth > 0F ? maxWidth : float.MaxValue;
        var positionedRuns = new List<PositionedTextRun>();
        var x = 0F;
        var y = 0F;
        var currentLineHeight = 0F;
        var maxLineWidth = 0F;
        var hasLineContent = false;
        var hasCompletedLine = false;
        var defaultLineHeight = MeasureLineHeight(TextStyle, dpi);

        void CompleteLine(bool force)
        {
            if (!hasLineContent && !force)
                return;

            maxLineWidth = Math.Max(maxLineWidth, x);
            y += currentLineHeight > 0F ? currentLineHeight : defaultLineHeight;
            x = 0F;
            currentLineHeight = 0F;
            hasLineContent = false;
            hasCompletedLine = true;
        }

        foreach (var child in Children)
        {
            switch (child)
            {
                case BrControl:
                    CompleteLine(force: true);
                    break;
                case SpanControl span:
                    AddSpan(span);
                    break;
            }
        }

        if (hasLineContent)
        {
            maxLineWidth = Math.Max(maxLineWidth, x);
            y += currentLineHeight > 0F ? currentLineHeight : defaultLineHeight;
        }

        return new RichTextLayout(
            new Size(maxLineWidth, hasLineContent || hasCompletedLine ? y : 0F),
            positionedRuns.AsReadOnly());

        void AddSpan(SpanControl span)
        {
            var textStyle = span.ApplyOverrides(TextStyle);
            foreach (var token in Tokenize(span.Text))
            {
                var isWhiteSpace = token.All(char.IsWhiteSpace);
                if (isWhiteSpace && !hasLineContent)
                    continue;

                var tokenSize = MeasureToken(textStyle, dpi, token);
                if (hasLineContent && x + tokenSize.Width > effectiveMaxWidth)
                {
                    CompleteLine(force: true);
                    if (isWhiteSpace)
                        continue;
                }

                positionedRuns.Add(new PositionedTextRun(token, textStyle, x, y));
                x += tokenSize.Width;
                currentLineHeight = Math.Max(currentLineHeight, MeasureLineHeight(textStyle, dpi));
                hasLineContent = true;
            }
        }
    }

    private Size MeasureToken(TextStyle textStyle, float dpi, string text)
        => _textService.Measure(textStyle, dpi, text.AsSpan(), float.MaxValue);

    private float MeasureLineHeight(TextStyle textStyle, float dpi)
    {
        var size = _textService.Measure(textStyle, dpi, "M".AsSpan(), float.MaxValue);
        return Math.Max(0F, size.Height * textStyle.LineHeight);
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        var index = 0;
        while (index < text.Length)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;
            if (index >= text.Length)
                yield break;

            var start = index;
            while (index < text.Length && !char.IsWhiteSpace(text[index]))
                index++;
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;

            yield return text[start..index];
        }
    }

    private sealed record RichTextLayout(Size Size, IReadOnlyList<PositionedTextRun> Runs);

    private readonly record struct PositionedTextRun(string Text, TextStyle TextStyle, float X, float Y);
}
