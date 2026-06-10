using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Base class for table row controls.
/// </summary>
public abstract class TableRowControlBase : AlignableContentControl
{
    internal TableControl? Table { get; set; }

    /// <summary>
    /// The background color of the table row.
    /// </summary>
    [Parameter]
    public Color Background { get; set; } = Colors.Transparent;

    /// <summary>
    /// The thickness of the table row border.
    /// </summary>
    [Parameter]
    public Thickness BorderThickness { get; set; }

    /// <summary>
    /// The color of the table row border.
    /// </summary>
    [Parameter]
    public Color BorderColor { get; set; } = Colors.Transparent;

    /// <inheritdoc />
    protected override Size DoMeasure(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo
    ) => MeasureWithCellWidthOverriding(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo);

    private Size MeasureWithCellWidthOverriding(
        float       dpi,
        Size        fullPageSize,
        Size        framedPageSize,
        Size        remainingSize,
        CultureInfo cultureInfo
    )
    {
        if (Table?.CellWidths is not { } cellWidths)
            throw new InvalidOperationException(
                "A TableRowControl must be added to a TableControl with a valid CellWidths dictionary"
            );
        var    borderOffset  = TableBoxStyle.GetBorderOffset(BorderThickness, fullPageSize, dpi);
        var    childSize     = TableBoxStyle.Deflate(remainingSize, borderOffset);
        ushort cellIndex  = 0;
        var    maxHeight  = 0F;
        var    totalWidth = 0F;
        foreach (var child in Children.OfType<TableCellControl>().Where((cell) => cell.ColumnSpan > 0))
        {
            var size                 = child.Measure(dpi, fullPageSize, childSize, childSize, cultureInfo);
            var adjustedCellWidth    = child.Width / child.ColumnSpan;
            var adjustedDesiredWidth = size.Width / child.ColumnSpan;
            for (var localCellIndex = cellIndex; localCellIndex < cellIndex + child.ColumnSpan; localCellIndex++)
            {
                if (!cellWidths.TryGetValue(localCellIndex, out var tuple))
                    tuple = (size.Width, child.Width);
                cellWidths[localCellIndex] = (Math.Max(tuple.desiredWitdth, adjustedDesiredWidth),
                                              GetPreferredColumnLength(
                                                  tuple.columnLength,
                                                  adjustedCellWidth,
                                                  childSize.Width,
                                                  dpi));
            }

            cellIndex  += child.ColumnSpan;
            totalWidth += size.Width;
            maxHeight  =  Math.Max(maxHeight, size.Height);
        }

        return new Size(totalWidth, maxHeight) + borderOffset;
    }

    internal Size MeasureWithCellWidth(
        float       dpi,
        Size        fullPageSize,
        Size        framedPageSize,
        Size        remainingSize,
        CultureInfo cultureInfo
    )
    {
        if (Table?.CellWidths is not { } cellWidths)
            throw new InvalidOperationException(
                "A TableRowControl must be added to a TableControl with a valid CellWidths dictionary"
            );
        var    borderOffset  = TableBoxStyle.GetBorderOffset(BorderThickness, fullPageSize, dpi);
        var    childSize     = TableBoxStyle.Deflate(remainingSize, borderOffset);
        var    widthScale    = GetCellWidthScale(childSize.Width);
        ushort cellIndex  = 0;
        var    maxHeight  = 0F;
        var    totalWidth = 0F;
        foreach (var child in Children.OfType<TableCellControl>().Where((cell) => cell.ColumnSpan > 0))
        {
            var availableWidth = 0F;
            for (var localCellIndex = cellIndex; localCellIndex < cellIndex + child.ColumnSpan; localCellIndex++)
            {
                var (width, _) = cellWidths[localCellIndex];
                availableWidth += width;
            }
            availableWidth *= widthScale;
            var size                 = child.Measure(dpi, fullPageSize, framedPageSize, remainingSize with
            {
                Width = availableWidth,
                Height = childSize.Height
            }, cultureInfo);
            cellIndex  += child.ColumnSpan;
            totalWidth += size.Width;
            maxHeight  =  Math.Max(maxHeight, size.Height);
        }

        return new Size(totalWidth, maxHeight) + borderOffset;
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo
    )
    {
        if (Table?.CellWidths is not { } cellWidths)
            throw new InvalidOperationException(
                "A TableRowControl must be added to a TableControl with a valid CellWidths dictionary"
            );
        var borderOffset = TableBoxStyle.GetBorderOffset(BorderThickness, fullPageSize, dpi);
        var childSize    = TableBoxStyle.Deflate(remainingSize, borderOffset);
        var widthScale   = GetCellWidthScale(childSize.Width);
        var   maxHeight = Math.Max(0F, MeasurementInner.Height - borderOffset.Height);
        float previousMaxHeight;
        var   count = 0;
        do
        {
            previousMaxHeight = maxHeight;
            ushort cellIndex  = 0;
            foreach (var child in Children.OfType<TableCellControl>().Where((cell) => cell.ColumnSpan > 0))
            {
                var widthAvailable = 0F;
                for (var localCellIndex = cellIndex; localCellIndex < cellIndex + child.ColumnSpan; localCellIndex++)
                {
                    _              =  cellWidths.TryGetValue(localCellIndex, out var tuple);
                    widthAvailable += tuple.desiredWitdth;
                }
                widthAvailable *= widthScale;

                cellIndex += child.ColumnSpan;

                var size = child.Arrange(
                    dpi,
                    fullPageSize,
                    new Size(Width: widthAvailable, Height: maxHeight),
                    new Size(Width: widthAvailable, Height: maxHeight),
                    cultureInfo
                );
                maxHeight = Math.Max(maxHeight, size.Height);
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
        } while (previousMaxHeight != maxHeight && ++count < 2);

        return remainingSize with { Height = maxHeight + borderOffset.Height };
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

        var borderOffset = TableBoxStyle.GetBorderOffset(BorderThickness, parentSize, dpi);
        var border       = BorderThickness.ToRectangle(parentSize, dpi);
        var widthScale   = GetCellWidthScale(Math.Max(0F, ArrangementInner.Width - borderOffset.Width));
        var additionalWidth  = 0F;
        var additionalHeight = 0F;
        if (Table is null)
            throw new InvalidOperationException("A TableRowControl must be added to a TableControl");
        using (canvas.CreateState())
        {
            canvas.Translate(border.Left, border.Top);
            ushort localCellIndex = 0;
            foreach (var control in Children.OfType<TableCellControl>().Where((cell) => cell.ColumnSpan > 0))
            {
                var widthAvailable = 0F;
                for (var i = localCellIndex; i < localCellIndex + control.ColumnSpan; i++)
                {
                    _              =  Table.CellWidths.TryGetValue(i, out var tuple);
                    widthAvailable += tuple.desiredWitdth;
                }
                widthAvailable *= widthScale;

                localCellIndex      += control.ColumnSpan;
                var (width, height) =  control.Render(canvas, dpi, parentSize, cultureInfo);
                additionalWidth     += width;
                additionalHeight    += height;
                canvas.Translate(widthAvailable, 0);
            }
        }

        return new Size(additionalWidth, additionalHeight);
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => type.IsEquivalentTo(typeof(TableCellControl));

    private float GetCellWidthScale(float availableWidth)
    {
        var totalWidth = GetTotalCellWidth();
        if (totalWidth <= 0F)
            return 1F;

        return availableWidth / totalWidth;
    }

    private static ColumnLength GetPreferredColumnLength(
        ColumnLength current,
        ColumnLength candidate,
        float bounds,
        float dpi)
    {
        if (IsAuto(current))
            return candidate;
        if (IsAuto(candidate))
            return current;

        if (current.Unit is EColumnUnit.Parts || candidate.Unit is EColumnUnit.Parts)
            return GetPartsValue(candidate) > GetPartsValue(current) ? candidate : current;

        var currentWidth = current.Length?.ToPixels(bounds, dpi) ?? 0F;
        var candidateWidth = candidate.Length?.ToPixels(bounds, dpi) ?? 0F;
        return candidateWidth > currentWidth ? candidate : current;
    }

    private static bool IsAuto(ColumnLength length)
        => length is {Unit: EColumnUnit.Length, Length.Unit: ELengthUnit.Auto};

    private static float GetPartsValue(ColumnLength length)
        => length.Unit is EColumnUnit.Parts ? length.Value ?? 0F : 0F;

    private float GetTotalCellWidth()
    {
        if (Table?.CellWidths is not { } cellWidths)
            return 0F;

        ushort cellIndex = 0;
        var totalWidth = 0F;
        foreach (var child in Children.OfType<TableCellControl>().Where((cell) => cell.ColumnSpan > 0))
        {
            for (var localCellIndex = cellIndex; localCellIndex < cellIndex + child.ColumnSpan; localCellIndex++)
            {
                _          =  cellWidths.TryGetValue(localCellIndex, out var tuple);
                totalWidth += tuple.desiredWitdth;
            }

            cellIndex += child.ColumnSpan;
        }

        return totalWidth;
    }
}
