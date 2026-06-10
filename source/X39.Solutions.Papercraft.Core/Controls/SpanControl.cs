using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Inline text fragment for <see cref="ParagraphControl"/>.
/// </summary>
[Control(Constants.ControlsNamespace, "span")]
public sealed class SpanControl : Control
{
    private Color _foreground;
    private float _fontSize;
    private float _lineHeight;
    private float _scale;
    private float _rotation;
    private float _strokeThickness;
    private TextDecoration _decoration;
    private FontWidth _letterSpacing;
    private FontWeight _weight;
    private EFontStyle _style;
    private string _fontFamily = string.Empty;

    private bool _hasForeground;
    private bool _hasFontSize;
    private bool _hasLineHeight;
    private bool _hasScale;
    private bool _hasRotation;
    private bool _hasStrokeThickness;
    private bool _hasDecoration;
    private bool _hasLetterSpacing;
    private bool _hasWeight;
    private bool _hasStyle;
    private bool _hasFontFamily;

    /// <summary>
    /// The inline text.
    /// </summary>
    [Parameter(IsContent = true)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Optional foreground override for this span.
    /// </summary>
    [Parameter]
    public Color Foreground
    {
        get => _foreground;
        set
        {
            _foreground = value;
            _hasForeground = true;
        }
    }

    /// <summary>
    /// Optional font size override for this span.
    /// </summary>
    [Parameter]
    public float FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            _hasFontSize = true;
        }
    }

    /// <summary>
    /// Optional line height override for this span.
    /// </summary>
    [Parameter]
    public float LineHeight
    {
        get => _lineHeight;
        set
        {
            _lineHeight = value;
            _hasLineHeight = true;
        }
    }

    /// <summary>
    /// Optional scale override for this span.
    /// </summary>
    [Parameter]
    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _hasScale = true;
        }
    }

    /// <summary>
    /// Optional rotation override for this span.
    /// </summary>
    [Parameter]
    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _hasRotation = true;
        }
    }

    /// <summary>
    /// Optional stroke thickness override for this span.
    /// </summary>
    [Parameter]
    public float StrokeThickness
    {
        get => _strokeThickness;
        set
        {
            _strokeThickness = value;
            _hasStrokeThickness = true;
        }
    }

    /// <summary>
    /// Optional text decoration override for this span.
    /// </summary>
    [Parameter]
    public TextDecoration Decoration
    {
        get => _decoration;
        set
        {
            _decoration = value;
            _hasDecoration = true;
        }
    }

    /// <summary>
    /// Optional letter spacing override for this span.
    /// </summary>
    [Parameter]
    public FontWidth LetterSpacing
    {
        get => _letterSpacing;
        set
        {
            _letterSpacing = value;
            _hasLetterSpacing = true;
        }
    }

    /// <summary>
    /// Optional font weight override for this span.
    /// </summary>
    [Parameter]
    public FontWeight Weight
    {
        get => _weight;
        set
        {
            _weight = value;
            _hasWeight = true;
        }
    }

    /// <summary>
    /// Optional font style override for this span.
    /// </summary>
    [Parameter]
    public EFontStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            _hasStyle = true;
        }
    }

    /// <summary>
    /// Optional font family override for this span.
    /// </summary>
    [Parameter]
    public string FontFamily
    {
        get => _fontFamily;
        set
        {
            _fontFamily = value;
            _hasFontFamily = true;
        }
    }

    internal TextStyle ApplyOverrides(TextStyle baseStyle)
    {
        var textStyle = baseStyle;
        if (_hasForeground)
            textStyle = textStyle with { Foreground = _foreground };
        if (_hasFontSize)
            textStyle = textStyle with { FontSize = _fontSize };
        if (_hasLineHeight)
            textStyle = textStyle with { LineHeight = _lineHeight };
        if (_hasScale)
            textStyle = textStyle with { Scale = _scale };
        if (_hasRotation)
            textStyle = textStyle with { Rotation = _rotation };
        if (_hasStrokeThickness)
            textStyle = textStyle with { StrokeThickness = _strokeThickness };
        if (_hasDecoration)
            textStyle = textStyle with { Decoration = _decoration };
        if (_hasLetterSpacing)
            textStyle = textStyle with
            {
                FontFamily = textStyle.FontFamily with { LetterSpacing = _letterSpacing },
            };
        if (_hasWeight)
            textStyle = textStyle with
            {
                FontFamily = textStyle.FontFamily with { Weight = _weight },
            };
        if (_hasStyle)
            textStyle = textStyle with
            {
                FontFamily = textStyle.FontFamily with { Style = _style },
            };
        if (_hasFontFamily)
            textStyle = textStyle with
            {
                FontFamily = textStyle.FontFamily with { Family = _fontFamily },
            };

        return textStyle;
    }

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => Size.Zero;

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => Size.Zero;

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        => Size.Zero;
}
