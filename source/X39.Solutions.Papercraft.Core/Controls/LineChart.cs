using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Line chart.
/// </summary>
[Control(Constants.ControlsNamespace)]
public class LineChart : ChartBaseControl
{
    private const float DataLabelGap = 4f;

    /// <summary>
    /// Creates a new line chart.
    /// </summary>
    public LineChart()
    {
    }

    /// <summary>
    /// Creates a new line chart.
    /// </summary>
    /// <param name="textService">The text service used to measure and render chart labels.</param>
    [ControlConstructor]
    public LineChart(ITextService textService) : base(textService)
    {
    }

    /// <summary>
    /// Thickness of the line.
    /// </summary>
    [Parameter(Name = "line-thickness")]
    public Length LineThickness { get; set; } = new(2, ELengthUnit.Pixel);

    /// <summary>
    /// Color of the line.
    /// </summary>
    [Parameter(Name = "line-color")]
    public Color? LineColor { get; set; }

    /// <summary>
    /// Whether to show point markers.
    /// </summary>
    [Parameter(Name = "show-points")]
    public bool ShowPoints { get; set; } = true;

    /// <summary>
    /// Size of point markers.
    /// </summary>
    [Parameter(Name = "point-size")]
    public float PointSize { get; set; } = 4f;

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
        var (minX, maxX, minY, maxY) = CalculateAxisBounds(dataPoints);

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

        // Determine line color
        var lineColor = LineColor ?? GetPaletteColor(0);
        var thickness = LineThickness.ToPixels(chartHeight, dpi);

        // Convert data points to screen coordinates
        var screenPoints = new List<(float X, float Y)>();
        foreach (var (x, y, _) in dataPoints)
        {
            var screenX = plotLeft + (float)((x - minX) * scaleX);
            var screenY = plotTop + plotHeight - (float)((y - minY) * scaleY);
            screenPoints.Add((screenX, screenY));
        }

        // Draw lines connecting points
        for (var i = 0; i < screenPoints.Count - 1; i++)
        {
            var (x1, y1) = screenPoints[i];
            var (x2, y2) = screenPoints[i + 1];
            canvas.DrawLine(lineColor, thickness, x1, y1, x2, y2);
        }

        // Draw point markers
        if (ShowPoints)
        {
            foreach (var (x, y) in screenPoints)
            {
                // Draw point as a small filled circle (approximated with cross)
                canvas.DrawLine(lineColor, PointSize, x - PointSize / 2, y, x + PointSize / 2, y);
                canvas.DrawLine(lineColor, PointSize, x, y - PointSize / 2, x, y + PointSize / 2);
            }
        }

        for (var i = 0; i < dataPoints.Count; i++)
        {
            var (_, y, control) = dataPoints[i];
            RenderDataLabel(canvas, dpi, cultureInfo, control, y, screenPoints[i], plotLeft, plotTop, plotWidth, plotHeight);
        }

        return Size.Zero;
    }

    private void RenderDataLabel(
        IDeferredCanvas canvas,
        float dpi,
        CultureInfo cultureInfo,
        ChartDataControl control,
        double y,
        (float X, float Y) point,
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
        var position = ClampTextPosition(
            new Point(point.X - size.Width / 2, point.Y - size.Height - DataLabelGap - PointSize / 2),
            size,
            new Rectangle(plotLeft, plotTop, plotWidth, plotHeight));
        DrawText(canvas, dpi, style, label, position.X, position.Y, plotWidth);
    }
}
