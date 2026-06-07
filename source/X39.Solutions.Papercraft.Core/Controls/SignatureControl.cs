using System.ComponentModel;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Visual signature placeholder with a signing line and optional helper text.
/// </summary>
[Control(Constants.ControlsNamespace, "signature")]
public sealed class SignatureControl : AlignableControl
{
    private const float TextGap = 2F;

    private readonly ITextService _textService;
    private TextStyle _textStyle = new();

    /// <summary>
    /// Creates a new instance of <see cref="SignatureControl"/>.
    /// </summary>
    /// <param name="textService">The text service used to measure and render label text.</param>
    [ControlConstructor]
    public SignatureControl(ITextService textService)
    {
        _textService = textService;
    }

    /// <summary>
    /// The reserved signature block height.
    /// </summary>
    [Parameter]
    public Length Height { get; set; } = new(20F, ELengthUnit.Millimeters);

    /// <summary>
    /// The signature line width.
    /// </summary>
    [Parameter]
    public Length LineWidth { get; set; } = new(1F, ELengthUnit.Percent);

    /// <summary>
    /// The signature line thickness.
    /// </summary>
    [Parameter]
    public Length LineThickness { get; set; } = new(1F, ELengthUnit.Points);

    /// <summary>
    /// The signature line color.
    /// </summary>
    [Parameter]
    public Color LineColor { get; set; } = Colors.Black;

    /// <summary>
    /// The main helper text.
    /// </summary>
    [Parameter]
    public string Label { get; set; } = "Signature";

    /// <summary>
    /// Optional secondary helper text.
    /// </summary>
    [Parameter]
    public string Subtext { get; set; } = string.Empty;

    /// <summary>
    /// Where helper text is drawn relative to the signature line.
    /// </summary>
    [Parameter]
    public ESignatureTextPlacement TextPlacement { get; set; } = ESignatureTextPlacement.Below;

    /// <summary>
    /// The foreground color of the text.
    /// </summary>
    [Parameter]
    public Color Foreground
    {
        get => _textStyle.Foreground;
        set => _textStyle = _textStyle with
        {
            Foreground = value,
        };
    }

    /// <summary>
    /// The size of the text.
    /// </summary>
    [Parameter]
    public float FontSize
    {
        get => _textStyle.FontSize;
        set => _textStyle = _textStyle with
        {
            FontSize = value,
        };
    }

    /// <summary>
    /// The text line height.
    /// </summary>
    [Parameter]
    public float LineHeight
    {
        get => _textStyle.LineHeight;
        set => _textStyle = _textStyle with
        {
            LineHeight = value,
        };
    }

    /// <summary>
    /// The text scale.
    /// </summary>
    [Parameter]
    public float Scale
    {
        get => _textStyle.Scale;
        set => _textStyle = _textStyle with
        {
            Scale = value,
        };
    }

    /// <summary>
    /// The text rotation.
    /// </summary>
    [Parameter]
    public float Rotation
    {
        get => _textStyle.Rotation;
        set => _textStyle = _textStyle with
        {
            Rotation = value,
        };
    }

    /// <summary>
    /// The thickness of the text stroke.
    /// </summary>
    [Parameter]
    public float StrokeThickness
    {
        get => _textStyle.StrokeThickness;
        set => _textStyle = _textStyle with
        {
            StrokeThickness = value,
        };
    }

    /// <summary>
    /// The width or letter spacing of the font.
    /// </summary>
    [Parameter]
    public FontWidth LetterSpacing
    {
        get => _textStyle.FontFamily.LetterSpacing;
        set => _textStyle = _textStyle with
        {
            FontFamily = _textStyle.FontFamily with
            {
                LetterSpacing = value,
            },
        };
    }

    /// <summary>
    /// The weight of the font.
    /// </summary>
    [Parameter]
    public FontWeight Weight
    {
        get => _textStyle.FontFamily.Weight;
        set => _textStyle = _textStyle with
        {
            FontFamily = _textStyle.FontFamily with
            {
                Weight = value,
            },
        };
    }

    /// <summary>
    /// The style of the font.
    /// </summary>
    [Parameter]
    public EFontStyle Style
    {
        get => _textStyle.FontFamily.Style;
        set => _textStyle = _textStyle with
        {
            FontFamily = _textStyle.FontFamily with
            {
                Style = value,
            },
        };
    }

    /// <summary>
    /// The font family.
    /// </summary>
    [Parameter]
    public string FontFamily
    {
        get => _textStyle.FontFamily.Family;
        set => _textStyle = _textStyle with
        {
            FontFamily = _textStyle.FontFamily with
            {
                Family = value,
            },
        };
    }

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => CalculateSize(dpi, remainingSize);

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => CalculateSize(dpi, remainingSize);

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var lineWidth = GetLineWidth(dpi, parentSize);
        var height = Math.Max(0F, ArrangementInner.Height);
        var lineThickness = GetLineThickness(dpi, height);
        var textBlockSize = MeasureTextBlock(dpi, Math.Max(lineWidth, ArrangementInner.Width));
        var lineY = GetLineY(height, lineThickness, textBlockSize.Height);

        if (lineWidth > 0F && lineThickness > 0F)
            canvas.DrawLine(LineColor, lineThickness, 0F, lineY, lineWidth, lineY);

        RenderTextBlock(canvas, dpi, lineY, lineThickness, textBlockSize.Height);
        return Size.Zero;
    }

    private Size CalculateSize(float dpi, Size remainingSize)
    {
        var lineWidth = GetLineWidth(dpi, remainingSize);
        var height = GetHeight(dpi, remainingSize);
        var textMeasureWidth = Math.Max(lineWidth, remainingSize.Width);
        var textWidth = MeasureTextBlock(dpi, textMeasureWidth).Width;
        return new Size(Math.Max(lineWidth, textWidth), height);
    }

    private Size MeasureTextBlock(float dpi, float maxWidth)
    {
        var labelSize = MeasureText(dpi, GetLabel(), maxWidth);
        var subtextSize = MeasureText(dpi, GetSubtext(), maxWidth);
        return new Size(
            Math.Max(labelSize.Width, subtextSize.Width),
            labelSize.Height + subtextSize.Height);
    }

    private Size MeasureText(float dpi, string text, float maxWidth)
        => string.IsNullOrEmpty(text)
            ? Size.Zero
            : _textService.Measure(_textStyle, dpi, text.AsSpan(), maxWidth);

    private void RenderTextBlock(
        IDeferredCanvas canvas,
        float dpi,
        float lineY,
        float lineThickness,
        float textBlockHeight)
    {
        var label = GetLabel();
        var subtext = GetSubtext();
        if (string.IsNullOrEmpty(label) && string.IsNullOrEmpty(subtext))
            return;

        var y = TextPlacement switch
        {
            ESignatureTextPlacement.Below => lineY + lineThickness / 2F + TextGap,
            ESignatureTextPlacement.Above => Math.Max(0F, lineY - lineThickness / 2F - TextGap - textBlockHeight),
            _ => throw new InvalidEnumArgumentException(
                nameof(TextPlacement),
                (int) TextPlacement,
                typeof(ESignatureTextPlacement)),
        };
        RenderText(canvas, dpi, label, ref y);
        RenderText(canvas, dpi, subtext, ref y);
    }

    private void RenderText(IDeferredCanvas canvas, float dpi, string text, ref float y)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var textSize = _textService.Measure(_textStyle, dpi, text.AsSpan(), ArrangementInner.Width);
        using var state = canvas.CreateState();
        canvas.Translate(0F, y);
        _textService.Draw(canvas, _textStyle, dpi, text.AsSpan(), ArrangementInner.Width);
        y += textSize.Height;
    }

    private float GetLineY(float height, float lineThickness, float textBlockHeight)
        => TextPlacement switch
        {
            ESignatureTextPlacement.Below => Math.Max(
                lineThickness / 2F,
                height - textBlockHeight - (textBlockHeight > 0F ? TextGap : 0F) - lineThickness / 2F),
            ESignatureTextPlacement.Above => Math.Max(lineThickness / 2F, height - lineThickness / 2F),
            _ => throw new InvalidEnumArgumentException(
                nameof(TextPlacement),
                (int) TextPlacement,
                typeof(ESignatureTextPlacement)),
        };

    private float GetLineWidth(float dpi, Size bounds)
        => Math.Max(0F, LineWidth.ToPixels(bounds.Width, dpi));

    private float GetHeight(float dpi, Size bounds)
        => Math.Max(0F, Height.ToPixels(bounds.Height, dpi));

    private float GetLineThickness(float dpi, float height)
        => Math.Max(0F, LineThickness.ToPixels(height, dpi));

    private string GetLabel()
        => Label.Trim();

    private string GetSubtext()
        => Subtext.Trim();
}
