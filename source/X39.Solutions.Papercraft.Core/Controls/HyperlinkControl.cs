using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// A visual hyperlink control that renders styled text and an optional underline.
/// </summary>
[Control(Constants.ControlsNamespace, "hyperlink")]
public sealed class HyperlinkControl : TextBaseControl
{
    /// <summary>
    /// Creates a new instance of <see cref="HyperlinkControl"/>.
    /// </summary>
    /// <param name="textService">The text service to use.</param>
    [ControlConstructor]
    public HyperlinkControl(ITextService textService) : base(textService)
    {
        Foreground = Colors.Blue;
    }

    /// <summary>
    /// The hyperlink target.
    /// </summary>
    [Parameter]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// The text to display.
    /// </summary>
    [Parameter(IsContent = true)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Whether the visible text should be underlined.
    /// </summary>
    [Parameter]
    public bool Underline { get; set; } = true;

    /// <inheritdoc />
    protected override string GetText() => Text;

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var text = GetText().Trim();
        RenderText(canvas, dpi, text);

        if (Underline && text.Length > 0 && ArrangementInner is {Width: > 0F, Height: > 0F})
        {
            var y = Math.Max(0F, ArrangementInner.Height - 1F);
            canvas.DrawLine(Foreground, 1F, 0F, y, ArrangementInner.Width, y);
        }

        return Size.Zero;
    }
}
