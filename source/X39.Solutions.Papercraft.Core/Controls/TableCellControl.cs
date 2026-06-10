using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// The table cell control.
/// </summary>
[Control(Constants.ControlsNamespace, "td")]
public sealed class TableCellControl : AlignableContentControl
{
    /// <summary>
    /// The width of the column.
    /// </summary>
    [Parameter]
    public ColumnLength Width { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of columns that the table cell spans.
    /// </summary>
    /// <remarks>
    /// The <see cref="ColumnSpan"/> property indicates the number of columns that the table cell spans.
    /// A table cell with a <see cref="ColumnSpan"/> of 1 occupies a single column.
    /// A table cell with a larger <see cref="ColumnSpan"/> value spans multiple columns and takes up the width
    /// of those columns.
    /// A table cell with a <see cref="ColumnSpan"/> of 0 spans will be ignored.
    /// </remarks>
    [Parameter]
    public ushort ColumnSpan { get; set; } = 1;

    /// <summary>
    /// The background color of the table cell.
    /// </summary>
    [Parameter]
    public Color Background { get; set; } = Colors.Transparent;

    /// <summary>
    /// The thickness of the table cell border.
    /// </summary>
    [Parameter]
    public Thickness BorderThickness { get; set; }

    /// <summary>
    /// The color of the table cell border.
    /// </summary>
    [Parameter]
    public Color BorderColor { get; set; } = Colors.Transparent;

    private readonly List<float> _heights = new();

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var borderOffset = TableBoxStyle.GetBorderOffset(BorderThickness, fullPageSize, dpi);
        var childSize    = TableBoxStyle.Deflate(remainingSize, borderOffset);
        var width = 0F;
        var height = 0F;
        foreach (var control in Children)
        {
            var size = control.Measure(dpi, fullPageSize, childSize, childSize, cultureInfo);
            width  =  Math.Max(width, size.Width);
            height += size.Height;
        }

        return new Size(width, height) + borderOffset;
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var borderOffset = TableBoxStyle.GetBorderOffset(BorderThickness, fullPageSize, dpi);
        var childSize    = TableBoxStyle.Deflate(remainingSize, borderOffset);
        _heights.Clear();
        var width = 0F;
        var height = 0F;
        foreach (var control in Children)
        {
            var size = control.Arrange(dpi, fullPageSize, childSize, childSize, cultureInfo);
            width  =  Math.Max(width, size.Width);
            height += size.Height;
            _heights.Add(size.Height);
        }

        if (HorizontalAlignment == EHorizontalAlignment.Stretch)
            width = Math.Max(width, childSize.Width);
        if (VerticalAlignment == EVerticalAlignment.Stretch)
            height = Math.Max(height, childSize.Height);

        var result = new Size(width, height) + borderOffset;
        return new Size(Math.Min(result.Width, remainingSize.Width), result.Height);
    }

    /// <inheritdoc />
    protected override Size PreRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var baseAdditionalSize = base.PreRender(canvas, dpi, parentSize, cultureInfo);
        if (!Clip)
            return baseAdditionalSize;

        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        dryRunCanvas.Translate(ArrangementInner);
        var contentAdditionalSize = RenderChildren(
            dryRunCanvas,
            dpi,
            parentSize,
            cultureInfo);

        return new Size(
            Math.Max(baseAdditionalSize.Width, contentAdditionalSize.Width),
            baseAdditionalSize.Height + contentAdditionalSize.Height);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        TableBoxStyle.Draw(
            canvas,
            Arrangement,
            ArrangementInner,
            Background,
            BorderThickness,
            BorderColor,
            parentSize,
            dpi);

        return RenderChildren(canvas, dpi, parentSize, cultureInfo);
    }

    private Size RenderChildren(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo)
    {
        var border = BorderThickness.ToRectangle(parentSize, dpi);
        var additionalWidth  = 0F;
        var additionalHeight = 0F;
        using (canvas.CreateState())
        {
            canvas.Translate(border.Left, border.Top);
            foreach (var (child, childHeight) in Children.Zip(_heights))
            {
                var (width, height) =  child.Render(canvas, dpi, parentSize, cultureInfo);
                additionalWidth     += width;
                additionalHeight    += height;
                canvas.Translate(0, childHeight + height);
            }
        }
        return new Size(additionalWidth, additionalHeight);
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => true;
}
