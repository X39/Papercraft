using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Control that reserves empty space without drawing output.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class SpacerControl : AlignableControl
{
    /// <summary>
    /// The requested spacer width.
    /// </summary>
    [Parameter]
    public Length Width { get; set; } = new(1F, ELengthUnit.Percent);

    /// <summary>
    /// The requested spacer height.
    /// </summary>
    [Parameter]
    public Length Height { get; set; } = new(0F, ELengthUnit.Pixel);

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
        => Size.Zero;

    private Size CalculateSize(float dpi, Size remainingSize)
        => new(
            Width.ToPixels(remainingSize.Width, dpi),
            Height.ToPixels(remainingSize.Height, dpi));
}
