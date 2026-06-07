using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Control that advances body flow to the next page without drawing output.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class PageBreakControl : AlignableControl
{
    private const float PageBoundaryTolerance = 0.001F;

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
    {
        if (parentSize.Height <= 0)
            return Size.Zero;

        var usedHeight = canvas.GetUsedPageHeight(parentSize.Height);
        if (usedHeight <= PageBoundaryTolerance)
            return Size.Zero;

        var remainingPageHeight = canvas.GetRemainingPageHeight(parentSize.Height);
        return remainingPageHeight <= PageBoundaryTolerance
            ? Size.Zero
            : new Size(0F, remainingPageHeight);
    }
}
