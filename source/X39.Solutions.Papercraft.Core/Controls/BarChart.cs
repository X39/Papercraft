using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Bar chart.
/// </summary>
[Control(Constants.ControlsNamespace)]
public class BarChart : ChartBaseControl
{
    private const float DataLabelGap = 3f;

    /// <summary>
    /// Creates a new bar chart.
    /// </summary>
    public BarChart()
    {
    }

    /// <summary>
    /// Creates a new bar chart.
    /// </summary>
    /// <param name="textService">The text service used to measure and render chart labels.</param>
    [ControlConstructor]
    public BarChart(ITextService textService) : base(textService)
    {
    }

    /// <summary>
    /// Orientation of the bars.
    /// </summary>
    [Parameter]
    public EOrientation Orientation { get; set; } = EOrientation.Vertical;

    /// <summary>
    /// Width of each bar. If 0 (default), width is calculated automatically.
    /// </summary>
    [Parameter(Name = "bar-width")]
    public Length BarWidth { get; set; } = new(0, ELengthUnit.Pixel);

    /// <summary>
    /// Spacing between bars as a percentage of bar width.
    /// </summary>
    [Parameter(Name = "bar-spacing")]
    public Length BarSpacing { get; set; } = new(0.1f, ELengthUnit.Percent);

    /// <summary>
    /// Color of the bars.
    /// </summary>
    [Parameter(Name = "bar-color")]
    public Color? BarColor { get; set; }

    /// <inheritdoc />
    protected override Size DoMeasure(float dpi, in Size fullPageSize, in Size framedPageSize, in Size remainingSize, CultureInfo cultureInfo)
    {
        var width = Width.ToPixels(remainingSize.Width, dpi);
        var height = Height.ToPixels(remainingSize.Height, dpi);
        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size DoArrange(float dpi, in Size fullPageSize, in Size framedPageSize, in Size remainingSize, CultureInfo cultureInfo)
    {
        var width = Width.ToPixels(remainingSize.Width, dpi);
        var height = Height.ToPixels(remainingSize.Height, dpi);
        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        // Use the control's own arranged dimensions, not the parent's size
        var chartWidth = ArrangementInner.Width;
        var chartHeight = ArrangementInner.Height;
        var dataPoints = ParseDataPoints();

        // Handle empty data
        if (dataPoints.Count == 0)
        {
            var textStyle = new TextStyle
            {
                Foreground = AxisColor,
                FontSize = 12f,
            };
            canvas.DrawText(textStyle, dpi, "No Data Available", 10, 20);
            return Size.Zero;
        }

        // Sort by X value
        dataPoints.Sort((a, b) => a.X.CompareTo(b.X));

        // Calculate bounds and scaling
        var (minX, maxX, minY, maxY) = CalculateAxisBounds(dataPoints, 5.0);

        // Adjust Y bounds to include zero baseline
        if (minY > 0)
            minY = 0;
        if (maxY < 0)
            maxY = 0;

        var layout = CalculateAxisChartLayout(dpi, chartWidth, chartHeight);
        var plotLeft = layout.PlotArea.Left;
        var plotTop = layout.PlotArea.Top;
        var plotWidth = layout.PlotArea.Width;
        var plotHeight = layout.PlotArea.Height;

        // Render title
        if (!string.IsNullOrEmpty(Title))
        {
            RenderTitle(canvas, dpi, chartWidth, 4f);
        }

        // Render grid
        RenderGrid(canvas, plotLeft, plotTop, plotWidth, plotHeight);

        // Render axes
        RenderAxes(canvas, plotLeft, plotTop, plotWidth, plotHeight);
        RenderAxisLabels(canvas, dpi, layout);

        // Calculate scaling
        var (scaleX, scaleY) = CalculateScaling(plotWidth, plotHeight, minX, maxX, minY, maxY);

        // Determine bar color
        var barColor = BarColor ?? GetPaletteColor(0);

        if (Orientation == EOrientation.Vertical)
        {
            RenderVerticalBars(canvas, dpi, cultureInfo, dataPoints, plotLeft, plotTop, plotWidth, plotHeight, minX, maxX, minY, maxY, scaleX, scaleY, barColor);
        }
        else
        {
            RenderHorizontalBars(canvas, dpi, cultureInfo, dataPoints, plotLeft, plotTop, plotWidth, plotHeight, minX, maxX, minY, maxY, scaleX, scaleY, barColor);
        }

        return Size.Zero;
    }

    private void RenderVerticalBars(
        IDeferredCanvas canvas,
        float dpi,
        CultureInfo cultureInfo,
        List<(double X, double Y, ChartDataControl Control)> dataPoints,
        float plotLeft,
        float plotTop,
        float plotWidth,
        float plotHeight,
        double minX,
        double maxX,
        double minY,
        double maxY,
        double scaleX,
        double scaleY,
        Color barColor)
    {
        // Calculate bar width using index-based positioning to prevent overflow
        var spacingPercent = BarSpacing.ToPixels(1, dpi);
        var slotWidth = plotWidth / dataPoints.Count;
        var barWidthPixels = BarWidth.ToPixels(plotWidth, dpi);

        if (barWidthPixels <= 0)
        {
            barWidthPixels = slotWidth * (1 - spacingPercent);
        }

        // Calculate baseline Y position (where Y=0 is)
        var baselineY = plotTop + plotHeight - (float)((0 - minY) * scaleY);

        for (var i = 0; i < dataPoints.Count; i++)
        {
            var (x, y, control) = dataPoints[i];
            // Position bars by index for even spacing within the plot area
            var barCenterX = plotLeft + slotWidth * (i + 0.5f);
            var screenY = plotTop + plotHeight - (float)((y - minY) * scaleY);

            // Use control color if available, otherwise use default
            var color = control.Color ?? barColor;

            // Draw bar from baseline to value
            var barLeft = barCenterX - barWidthPixels / 2;
            var barTop = y >= 0 ? screenY : baselineY;
            var barHeight = Math.Abs(screenY - baselineY);

            canvas.DrawRect(
                new Rectangle(barLeft, barTop, barWidthPixels, barHeight),
                color);

            RenderVerticalDataLabel(canvas, dpi, cultureInfo, control, y, barLeft, barTop, barWidthPixels, baselineY, plotLeft, plotTop, plotWidth, plotHeight);
        }
    }

    private void RenderHorizontalBars(
        IDeferredCanvas canvas,
        float dpi,
        CultureInfo cultureInfo,
        List<(double X, double Y, ChartDataControl Control)> dataPoints,
        float plotLeft,
        float plotTop,
        float plotWidth,
        float plotHeight,
        double minX,
        double maxX,
        double minY,
        double maxY,
        double scaleX,
        double scaleY,
        Color barColor)
    {
        // Calculate bar height using index-based positioning to prevent overflow
        var spacingPercent = BarSpacing.ToPixels(1, dpi);
        var slotHeight = plotHeight / dataPoints.Count;
        var barHeightPixels = BarWidth.ToPixels(plotHeight, dpi);

        if (barHeightPixels <= 0)
        {
            barHeightPixels = slotHeight * (1 - spacingPercent);
        }

        // For horizontal bars, Y values map to the horizontal axis
        var yRange = maxY - minY;
        var horizontalScale = yRange > 0.0001 ? plotWidth / yRange : 1.0;

        // Calculate baseline X position (where Y=0 is)
        var baselineX = plotLeft + (float)((0 - minY) * horizontalScale);

        for (var i = 0; i < dataPoints.Count; i++)
        {
            var (x, y, control) = dataPoints[i];
            // Position bars by index for even spacing within the plot area
            var barCenterY = plotTop + slotHeight * (i + 0.5f);
            var screenX = plotLeft + (float)((y - minY) * horizontalScale);

            // Use control color if available, otherwise use default
            var color = control.Color ?? barColor;

            // Draw bar from baseline to value
            var barTop = barCenterY - barHeightPixels / 2;
            var barLeft = y >= 0 ? baselineX : screenX;
            var barWidth = Math.Abs(screenX - baselineX);

            canvas.DrawRect(
                new Rectangle(barLeft, barTop, barWidth, barHeightPixels),
                color);

            RenderHorizontalDataLabel(canvas, dpi, cultureInfo, control, y, barLeft, barTop, barWidth, barHeightPixels, baselineX, plotLeft, plotTop, plotWidth, plotHeight);
        }
    }

    private void RenderVerticalDataLabel(
        IDeferredCanvas canvas,
        float dpi,
        CultureInfo cultureInfo,
        ChartDataControl control,
        double y,
        float barLeft,
        float barTop,
        float barWidth,
        float baselineY,
        float plotLeft,
        float plotTop,
        float plotWidth,
        float plotHeight)
    {
        var label = GetDataLabel(control, y, cultureInfo);
        if (string.IsNullOrEmpty(label))
            return;

        var style = CreateLabelTextStyle();
        var size = MeasureText(style, dpi, label, plotWidth);
        var labelX = barLeft + (barWidth - size.Width) / 2;
        var labelY = y >= 0
            ? barTop - size.Height - DataLabelGap
            : baselineY + DataLabelGap;
        var position = ClampTextPosition(
            new Point(labelX, labelY),
            size,
            new Rectangle(plotLeft, plotTop, plotWidth, plotHeight));
        DrawText(canvas, dpi, style, label, position.X, position.Y, plotWidth);
    }

    private void RenderHorizontalDataLabel(
        IDeferredCanvas canvas,
        float dpi,
        CultureInfo cultureInfo,
        ChartDataControl control,
        double y,
        float barLeft,
        float barTop,
        float barWidth,
        float barHeight,
        float baselineX,
        float plotLeft,
        float plotTop,
        float plotWidth,
        float plotHeight)
    {
        var label = GetDataLabel(control, y, cultureInfo);
        if (string.IsNullOrEmpty(label))
            return;

        var style = CreateLabelTextStyle();
        var size = MeasureText(style, dpi, label, plotWidth);
        var labelX = y >= 0
            ? barLeft + barWidth + DataLabelGap
            : baselineX - size.Width - DataLabelGap;
        var labelY = barTop + (barHeight - size.Height) / 2;
        var position = ClampTextPosition(
            new Point(labelX, labelY),
            size,
            new Rectangle(plotLeft, plotTop, plotWidth, plotHeight));
        DrawText(canvas, dpi, style, label, position.X, position.Y, plotWidth);
    }
}
