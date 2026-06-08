using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Abstraction.Controls;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls.Base;

/// <summary>
/// Abstract base class for chart controls with shared functionality.
/// </summary>
public abstract class ChartBaseControl : AlignableContentControl, IChart
{
    private const float TitleFontSize = 14f;
    private const float LabelFontSize = 10f;
    private const float AxisLabelFontSize = 11f;
    private const float MinimumPadding = 6f;
    private const float TextWidthFactor = 0.55f;
    private const float TextHeightFactor = 1.25f;

    private readonly ITextService? _textService;

    /// <summary>
    /// Creates a chart control.
    /// </summary>
    /// <param name="textService">Optional text service used for accurate label measurement and rendering.</param>
    protected ChartBaseControl(ITextService? textService = null)
    {
        _textService = textService;
    }

    /// <summary>
    /// Width of the chart.
    /// </summary>
    [Parameter]
    public Length Width { get; set; } = new(1, ELengthUnit.Percent);

    /// <summary>
    /// Height of the chart.
    /// </summary>
    [Parameter]
    public Length Height { get; set; } = new(300, ELengthUnit.Pixel);

    /// <summary>
    /// Title of the chart.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Whether to show grid lines.
    /// </summary>
    [Parameter(Name = "show-grid")]
    public bool ShowGrid { get; set; } = true;

    /// <summary>
    /// Color of grid lines.
    /// </summary>
    [Parameter(Name = "grid-color")]
    public Color GridColor { get; set; } = new(0xCCCCCCFF);

    /// <summary>
    /// Color of axis lines.
    /// </summary>
    [Parameter(Name = "axis-color")]
    public Color AxisColor { get; set; } = new(0x000000FF);

    /// <summary>
    /// Whether to show X-axis.
    /// </summary>
    [Parameter(Name = "show-x-axis")]
    public bool ShowXAxis { get; set; } = true;

    /// <summary>
    /// Whether to show Y-axis.
    /// </summary>
    [Parameter(Name = "show-y-axis")]
    public bool ShowYAxis { get; set; } = true;

    /// <summary>
    /// Label for X-axis.
    /// </summary>
    [Parameter(Name = "x-axis-label")]
    public string XAxisLabel { get; set; } = string.Empty;

    /// <summary>
    /// Label for Y-axis.
    /// </summary>
    [Parameter(Name = "y-axis-label")]
    public string YAxisLabel { get; set; } = string.Empty;

    /// <summary>
    /// Whether to show automatic numeric value labels for line and bar chart data points.
    /// Explicit data labels are drawn even when this is false.
    /// </summary>
    [Parameter(Name = "show-data-labels")]
    public bool ShowDataLabels { get; set; }

    /// <summary>
    /// Default color palette for charts.
    /// </summary>
    protected static readonly Color[] DefaultColorPalette =
    [
        new(0x4472C4FF), // Blue
        new(0xED7D31FF), // Orange
        new(0xA5A5A5FF), // Gray
        new(0xFFC000FF), // Yellow
        new(0x5B9BD5FF), // Light Blue
        new(0x70AD47FF), // Green
        new(0x264478FF), // Dark Blue
        new(0x9E480EFF), // Dark Orange
    ];

    /// <summary>
    /// Parses data points from ChartDataControl children.
    /// </summary>
    /// <param name="requireX">Whether X values are required. When false, missing X values default to sequential indices.</param>
    /// <returns>List of tuples containing parsed X, Y values and the source control.</returns>
    protected List<(double X, double Y, ChartDataControl Control)> ParseDataPoints(bool requireX = true)
    {
        var dataPoints = new List<(double X, double Y, ChartDataControl Control)>();
        var index = 0;

        foreach (var child in Children.OfType<ChartDataControl>())
        {
            var x = child.GetParsedX();
            var y = child.GetParsedY();

            if (!y.HasValue)
            {
                index++;
                continue;
            }

            if (requireX && !x.HasValue)
            {
                index++;
                continue;
            }

            dataPoints.Add((x ?? index, y.Value, child));
            index++;
        }

        return dataPoints;
    }

    /// <summary>
    /// Calculates axis bounds with padding.
    /// </summary>
    /// <param name="dataPoints">The data points to analyze.</param>
    /// <param name="paddingPercent">Padding percentage (default 10%).</param>
    /// <returns>Tuple of (minX, maxX, minY, maxY).</returns>
    protected (double MinX, double MaxX, double MinY, double MaxY) CalculateAxisBounds(
        List<(double X, double Y, ChartDataControl Control)> dataPoints,
        double paddingPercent = 10.0)
    {
        if (dataPoints.Count == 0)
            return (0, 1, 0, 1);

        var minX = dataPoints.Min(p => p.X);
        var maxX = dataPoints.Max(p => p.X);
        var minY = dataPoints.Min(p => p.Y);
        var maxY = dataPoints.Max(p => p.Y);

        // Add padding
        var xRange = maxX - minX;
        var yRange = maxY - minY;

        // Handle zero range
        if (Math.Abs(xRange) < 0.0001)
        {
            minX -= 0.5;
            maxX += 0.5;
        }
        else
        {
            var xPadding = xRange * paddingPercent / 100.0;
            minX -= xPadding;
            maxX += xPadding;
        }

        if (Math.Abs(yRange) < 0.0001)
        {
            minY -= 0.5;
            maxY += 0.5;
        }
        else
        {
            var yPadding = yRange * paddingPercent / 100.0;
            minY -= yPadding;
            maxY += yPadding;
        }

        return (minX, maxX, minY, maxY);
    }

    /// <summary>
    /// Calculates scaling factors for data to pixels.
    /// </summary>
    /// <param name="plotWidth">Width of the plot area in pixels.</param>
    /// <param name="plotHeight">Height of the plot area in pixels.</param>
    /// <param name="minX">Minimum X value.</param>
    /// <param name="maxX">Maximum X value.</param>
    /// <param name="minY">Minimum Y value.</param>
    /// <param name="maxY">Maximum Y value.</param>
    /// <returns>Tuple of (scaleX, scaleY).</returns>
    protected (double ScaleX, double ScaleY) CalculateScaling(
        float plotWidth,
        float plotHeight,
        double minX,
        double maxX,
        double minY,
        double maxY)
    {
        var xRange = maxX - minX;
        var yRange = maxY - minY;

        var scaleX = xRange > 0.0001 ? plotWidth / xRange : 1.0;
        var scaleY = yRange > 0.0001 ? plotHeight / yRange : 1.0;

        return (scaleX, scaleY);
    }

    /// <summary>
    /// Renders grid lines on the canvas.
    /// </summary>
    protected void RenderGrid(
        IDeferredCanvas canvas,
        float plotLeft,
        float plotTop,
        float plotWidth,
        float plotHeight,
        int gridLinesX = 5,
        int gridLinesY = 5)
    {
        if (!ShowGrid)
            return;

        // Vertical grid lines
        for (var i = 0; i <= gridLinesX; i++)
        {
            var x = plotLeft + (plotWidth * i / gridLinesX);
            canvas.DrawLine(GridColor, 1f, x, plotTop, x, plotTop + plotHeight);
        }

        // Horizontal grid lines
        for (var i = 0; i <= gridLinesY; i++)
        {
            var y = plotTop + (plotHeight * i / gridLinesY);
            canvas.DrawLine(GridColor, 1f, plotLeft, y, plotLeft + plotWidth, y);
        }
    }

    /// <summary>
    /// Renders axis lines on the canvas.
    /// </summary>
    protected void RenderAxes(
        IDeferredCanvas canvas,
        float plotLeft,
        float plotTop,
        float plotWidth,
        float plotHeight)
    {
        if (ShowXAxis)
        {
            // X-axis at bottom
            canvas.DrawLine(AxisColor, 2f, plotLeft, plotTop + plotHeight, plotLeft + plotWidth, plotTop + plotHeight);
        }

        if (ShowYAxis)
        {
            // Y-axis at left
            canvas.DrawLine(AxisColor, 2f, plotLeft, plotTop, plotLeft, plotTop + plotHeight);
        }
    }

    /// <summary>
    /// Renders the chart title.
    /// </summary>
    protected void RenderTitle(
        IDeferredCanvas canvas,
        float dpi,
        float chartWidth,
        float y)
    {
        if (string.IsNullOrEmpty(Title))
            return;

        var textStyle = CreateTitleTextStyle();
        var size = MeasureText(textStyle, dpi, Title, chartWidth);
        DrawText(canvas, dpi, textStyle, Title, (chartWidth - size.Width) / 2, y, size.Width);
    }

    /// <summary>
    /// Creates the default chart title text style.
    /// </summary>
    protected TextStyle CreateTitleTextStyle()
        => new()
        {
            Foreground = AxisColor,
            FontSize = TitleFontSize,
        };

    /// <summary>
    /// Creates the default chart label text style.
    /// </summary>
    protected TextStyle CreateLabelTextStyle()
        => new()
        {
            Foreground = AxisColor,
            FontSize = LabelFontSize,
        };

    /// <summary>
    /// Creates the default chart axis label text style.
    /// </summary>
    protected TextStyle CreateAxisLabelTextStyle()
        => new()
        {
            Foreground = AxisColor,
            FontSize = AxisLabelFontSize,
        };

    /// <summary>
    /// Measures chart text.
    /// </summary>
    protected Size MeasureText(TextStyle textStyle, float dpi, string text, float maxWidth)
    {
        if (string.IsNullOrEmpty(text))
            return Size.Zero;

        if (_textService is not null)
            return _textService.Measure(textStyle, dpi, text.AsSpan(), Math.Max(1f, maxWidth));

        return new Size(
            Math.Min(maxWidth, text.Length * textStyle.FontSize * TextWidthFactor),
            textStyle.FontSize * TextHeightFactor);
    }

    /// <summary>
    /// Draws chart text using top-left coordinates.
    /// </summary>
    protected void DrawText(
        IDeferredCanvas canvas,
        float dpi,
        TextStyle textStyle,
        string text,
        float x,
        float y,
        float maxWidth)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (_textService is not null)
        {
            using var state = canvas.CreateState();
            canvas.Translate(x, y);
            _textService.Draw(canvas, textStyle, dpi, text.AsSpan(), Math.Max(1f, maxWidth));
            return;
        }

        canvas.DrawText(textStyle, dpi, text, x, y + EstimateTextHeight(textStyle));
    }

    /// <summary>
    /// Draws chart text centered around the provided point.
    /// </summary>
    protected void DrawCenteredText(
        IDeferredCanvas canvas,
        float dpi,
        TextStyle textStyle,
        string text,
        float centerX,
        float centerY,
        float maxWidth)
    {
        var size = MeasureText(textStyle, dpi, text, maxWidth);
        DrawText(canvas, dpi, textStyle, text, centerX - size.Width / 2, centerY - size.Height / 2, maxWidth);
    }

    /// <summary>
    /// Clamps a top-left text position to stay inside the given bounds.
    /// </summary>
    protected Point ClampTextPosition(Point position, Size textSize, Rectangle bounds)
    {
        var maxX = Math.Max(bounds.Left, bounds.Right - textSize.Width);
        var maxY = Math.Max(bounds.Top, bounds.Bottom - textSize.Height);
        return new Point(
            Math.Clamp(position.X, bounds.Left, maxX),
            Math.Clamp(position.Y, bounds.Top, maxY));
    }

    /// <summary>
    /// Calculates a plot area for axis-based charts.
    /// </summary>
    protected ChartPlotLayout CalculateAxisChartLayout(float dpi, float chartWidth, float chartHeight)
    {
        var titleStyle = CreateTitleTextStyle();
        var axisStyle = CreateAxisLabelTextStyle();

        var titleHeight = string.IsNullOrEmpty(Title)
            ? 0f
            : MeasureText(titleStyle, dpi, Title, chartWidth).Height + MinimumPadding;
        var xAxisLabelHeight = string.IsNullOrEmpty(XAxisLabel)
            ? 0f
            : MeasureText(axisStyle, dpi, XAxisLabel, chartWidth).Height + MinimumPadding;
        var yAxisLabelWidth = string.IsNullOrEmpty(YAxisLabel)
            ? 0f
            : MeasureText(axisStyle, dpi, YAxisLabel, chartHeight).Height + MinimumPadding;

        var top = MinimumPadding + titleHeight;
        var left = MinimumPadding + yAxisLabelWidth + (ShowYAxis ? 8f : 0f);
        var right = MinimumPadding;
        var bottom = MinimumPadding + xAxisLabelHeight + (ShowXAxis ? 8f : 0f);
        var plotWidth = Math.Max(1f, chartWidth - left - right);
        var plotHeight = Math.Max(1f, chartHeight - top - bottom);

        return new ChartPlotLayout(
            new Rectangle(left, top, plotWidth, plotHeight),
            new Rectangle(0f, 0f, chartWidth, chartHeight),
            titleHeight,
            xAxisLabelHeight,
            yAxisLabelWidth);
    }

    /// <summary>
    /// Renders axis labels for a plot area.
    /// </summary>
    protected void RenderAxisLabels(IDeferredCanvas canvas, float dpi, ChartPlotLayout layout)
    {
        var textStyle = CreateAxisLabelTextStyle();
        if (!string.IsNullOrEmpty(XAxisLabel))
        {
            var size = MeasureText(textStyle, dpi, XAxisLabel, layout.Bounds.Width);
            DrawText(
                canvas,
                dpi,
                textStyle,
                XAxisLabel,
                layout.PlotArea.Left + (layout.PlotArea.Width - size.Width) / 2,
                layout.PlotArea.Bottom + MinimumPadding,
                layout.Bounds.Width);
        }

        if (!string.IsNullOrEmpty(YAxisLabel))
        {
            var size = MeasureText(textStyle, dpi, YAxisLabel, layout.Bounds.Height);
            DrawText(
                canvas,
                dpi,
                textStyle with { Rotation = -90f },
                YAxisLabel,
                MinimumPadding,
                layout.PlotArea.Top + (layout.PlotArea.Height + size.Width) / 2,
                layout.Bounds.Height);
        }
    }

    /// <summary>
    /// Gets the visible label for a data point.
    /// </summary>
    protected string GetDataLabel(ChartDataControl control, double y, CultureInfo cultureInfo)
    {
        var valueLabel = y.ToString("G", cultureInfo);
        if (!string.IsNullOrWhiteSpace(control.Label))
            return ShowDataLabels ? $"{control.Label.Trim()}: {valueLabel}" : control.Label.Trim();
        if (!string.IsNullOrWhiteSpace(control.YLabel))
            return ShowDataLabels ? $"{control.YLabel.Trim()}: {valueLabel}" : control.YLabel.Trim();
        if (!string.IsNullOrWhiteSpace(control.XLabel))
            return ShowDataLabels ? $"{control.XLabel.Trim()}: {valueLabel}" : control.XLabel.Trim();
        return ShowDataLabels ? valueLabel : string.Empty;
    }

    /// <summary>
    /// Estimates a single-line text height for fallback drawing.
    /// </summary>
    protected static float EstimateTextHeight(TextStyle textStyle)
        => textStyle.FontSize * TextHeightFactor;


    /// <summary>
    /// Gets a color from the palette by index.
    /// </summary>
    protected Color GetPaletteColor(int index)
    {
        return DefaultColorPalette[index % DefaultColorPalette.Length];
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type)
        => type.IsEquivalentTo(typeof(ChartDataControl));

    /// <summary>
    /// Layout information for an axis-based chart.
    /// </summary>
    protected readonly record struct ChartPlotLayout(
        Rectangle PlotArea,
        Rectangle Bounds,
        float TitleHeight,
        float XAxisLabelHeight,
        float YAxisLabelWidth);
}
