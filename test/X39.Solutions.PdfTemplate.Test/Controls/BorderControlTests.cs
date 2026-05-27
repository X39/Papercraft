using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class BorderControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(200, 100);

    [Fact]
    public async Task XmlParametersAreApplied()
    {
        var control = await """
                            <border
                                thickness="1px 2px 3px 4px"
                                color="red"
                                background="green"
                                padding="5px"
                                margin="6px"
                                clip="false"
                                horizontalAlignment="right"
                                verticalAlignment="bottom">
                                <mock width="10px" height="20px"/>
                            </border>
                            """.ToControl<BorderControl>();

        Assert.Equal(new Thickness(1F, 2F, 3F, 4F), control.Thickness);
        Assert.Equal(Colors.Red, control.Color);
        Assert.Equal(Colors.Green, control.Background);
        Assert.Equal(new Thickness(5F), control.Padding);
        Assert.Equal(new Thickness(6F), control.Margin);
        Assert.False(control.Clip);
        Assert.Equal(EHorizontalAlignment.Right, control.HorizontalAlignment);
        Assert.Equal(EVerticalAlignment.Bottom, control.VerticalAlignment);
        Assert.Single(control.Children);
    }

    [Fact]
    public void CanAddAllowsAnyControlType()
    {
        var control = new BorderControl();

        Assert.True(control.CanAdd(typeof(TextControl)));
        Assert.True(control.CanAdd(typeof(BorderControl)));
        Assert.True(control.CanAdd(typeof(MockControl)));
    }

    [Fact]
    public void MeasureIncludesBorderThicknessAroundStackedChildren()
    {
        var control = CreateBorder(
            thickness: new Thickness(1F, 2F, 3F, 4F),
            horizontalAlignment: EHorizontalAlignment.Left,
            verticalAlignment: EVerticalAlignment.Top);
        control.Add(new MockControl {Width = 100F, Height = 20F});
        control.Add(new MockControl {Width = 80F, Height = 30F});

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(104, 56), measured);
        Assert.Equal(new Rectangle(0, 0, 104, 56), control.Measurement);
        Assert.Equal(new Rectangle(0, 0, 104, 56), control.MeasurementInner);
    }

    [Fact]
    public void ArrangeWithTopLeftAlignmentFitsContentAndBorderThickness()
    {
        var control = CreateBorder(
            thickness: new Thickness(1F, 2F, 3F, 4F),
            horizontalAlignment: EHorizontalAlignment.Left,
            verticalAlignment: EVerticalAlignment.Top);
        control.Add(new MockControl {Width = 100F, Height = 20F});
        control.Add(new MockControl {Width = 80F, Height = 30F});

        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(104, 56), arranged);
        Assert.Equal(new Rectangle(0, 0, 104, 56), control.Arrangement);
        Assert.Equal(new Rectangle(0, 0, 104, 56), control.ArrangementInner);
    }

    [Fact]
    public void ArrangeWithStretchAlignmentExpandsToRemainingSize()
    {
        var control = CreateBorder(thickness: new Thickness(1F, 2F, 3F, 4F));
        control.Add(new MockControl {Width = 100F, Height = 20F});
        control.Add(new MockControl {Width = 80F, Height = 30F});

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(104, 56), measured);
        Assert.Equal(PageSize, arranged);
        Assert.Equal(new Rectangle(0, 0, 200, 100), control.Arrangement);
        Assert.Equal(new Rectangle(0, 0, 200, 100), control.ArrangementInner);
    }

    [Fact]
    public void RenderDrawsBackgroundAndEveryBorderSide()
    {
        var control = CreateBorder(
            thickness: new Thickness(1F, 2F, 3F, 4F),
            color: Colors.Green,
            background: Colors.Blue,
            horizontalAlignment: EHorizontalAlignment.Left,
            verticalAlignment: EVerticalAlignment.Top);
        control.Add(new MockControl {Width = 100F, Height = 50F});
        var canvas = CreateCanvas();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        canvas.AssertState();
        canvas.AssertDrawRect(new Rectangle(0, 0, 104, 56), Colors.Blue);
        canvas.AssertDrawLine(
            (Colors.Green, 2F, 0F, 0F, 0F, 56F),
            (Colors.Green, 4F, 0F, 0F, 104F, 0F),
            (Colors.Green, 6F, 104F, 56F, 104F, 0F),
            (Colors.Green, 8F, 104F, 56F, 0F, 56F));
    }

    [Fact]
    public void RenderSkipsTransparentBackgroundAndZeroThicknessSides()
    {
        var control = CreateBorder(
            thickness: new Thickness(0F, 2F, 0F, 4F),
            color: Colors.Green,
            background: Colors.Transparent,
            horizontalAlignment: EHorizontalAlignment.Left,
            verticalAlignment: EVerticalAlignment.Top);
        control.Add(new MockControl {Width = 50F, Height = 20F});
        var canvas = CreateCanvas();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(0, canvas.DrawRectCount);
        canvas.AssertDrawLine(
            (Colors.Green, 4F, 0F, 0F, 50F, 0F),
            (Colors.Green, 8F, 50F, 26F, 0F, 26F));
    }

    [Fact]
    public void RenderAppliesMarginPaddingAndClipToOuterBorder()
    {
        var control = CreateBorder(
            thickness: new Thickness(1F),
            color: Colors.Red,
            background: Colors.Blue,
            horizontalAlignment: EHorizontalAlignment.Left,
            verticalAlignment: EVerticalAlignment.Top);
        control.Margin = new Thickness(5F);
        control.Padding = new Thickness(10F);
        control.Clip = true;
        control.Add(new MockControl {Width = 20F, Height = 10F});
        var canvas = CreateCanvas();

        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(52, 42), arranged);
        canvas.AssertState();
        canvas.AssertClip(new Rectangle(5, 5, 42, 32));
        canvas.AssertDrawRect(new Rectangle(5, 5, 42, 32), Colors.Blue);
        canvas.AssertDrawLine(
            (Colors.Red, 2F, 5F, 5F, 5F, 37F),
            (Colors.Red, 2F, 5F, 5F, 47F, 5F),
            (Colors.Red, 2F, 47F, 37F, 47F, 5F),
            (Colors.Red, 2F, 47F, 37F, 5F, 37F));
    }

    [Fact]
    public void RenderPlacesChildrenInsideBorderAndStacksByArrangedHeight()
    {
        var control = CreateBorder(
            thickness: new Thickness(1F, 2F, 3F, 4F),
            color: Colors.Red,
            horizontalAlignment: EHorizontalAlignment.Left,
            verticalAlignment: EVerticalAlignment.Top);
        control.Clip = false;
        control.Add(new DrawingControl(new Size(10, 12), Colors.Black));
        control.Add(new DrawingControl(new Size(20, 8), Colors.Magenta));
        var canvas = CreateCanvas();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        canvas.AssertDrawLine(
            (Colors.Red, 2F, 0F, 0F, 0F, 26F),
            (Colors.Red, 4F, 0F, 0F, 24F, 0F),
            (Colors.Red, 6F, 24F, 26F, 24F, 0F),
            (Colors.Red, 8F, 24F, 26F, 0F, 26F),
            (Colors.Black, 1F, 1F, 2F, 2F, 3F),
            (Colors.Magenta, 1F, 1F, 14F, 2F, 15F));
    }

    [Fact]
    public void RepeatedArrangeUsesLatestChildArrangements()
    {
        var control = CreateBorder(
            thickness: new Thickness(1F),
            color: Colors.Red,
            horizontalAlignment: EHorizontalAlignment.Left,
            verticalAlignment: EVerticalAlignment.Top);
        control.Clip = false;
        control.Add(new SequencedDrawingControl(Colors.Black, new Size(10, 10), new Size(10, 30)));
        control.Add(new SequencedDrawingControl(Colors.Magenta, new Size(10, 20), new Size(10, 40)));
        var canvas = CreateCanvas();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        canvas.AssertDrawLine(
            (Colors.Red, 2F, 0F, 0F, 0F, 72F),
            (Colors.Red, 2F, 0F, 0F, 12F, 0F),
            (Colors.Red, 2F, 12F, 72F, 12F, 0F),
            (Colors.Red, 2F, 12F, 72F, 0F, 72F),
            (Colors.Black, 1F, 1F, 1F, 2F, 2F),
            (Colors.Magenta, 1F, 1F, 31F, 2F, 32F));
    }

    private static BorderControl CreateBorder(
        Thickness? thickness = null,
        Color? color = null,
        Color? background = null,
        EHorizontalAlignment horizontalAlignment = EHorizontalAlignment.Stretch,
        EVerticalAlignment verticalAlignment = EVerticalAlignment.Stretch)
    {
        return new BorderControl
        {
            Thickness = thickness ?? default,
            Color = color ?? Colors.Black,
            Background = background ?? Colors.Transparent,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
        };
    }

    private static DeferredCanvasMock CreateCanvas()
    {
        return new DeferredCanvasMock {ActualPageSize = PageSize, PageSize = PageSize};
    }

    private sealed class DrawingControl(Size size, Color color) : IControl
    {
        public Size Measure(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
        {
            return size;
        }

        public Size Arrange(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
        {
            return size;
        }

        public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        {
            canvas.DrawLine(color, 1F, 0F, 0F, 1F, 1F);
            return Size.Zero;
        }
    }

    private sealed class SequencedDrawingControl(Color color, params Size[] arrangedSizes) : IControl
    {
        private int _arrangeIndex;

        public Size Measure(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
        {
            return arrangedSizes.Last();
        }

        public Size Arrange(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
        {
            var index = Math.Min(_arrangeIndex, arrangedSizes.Length - 1);
            _arrangeIndex++;
            return arrangedSizes[index];
        }

        public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        {
            canvas.DrawLine(color, 1F, 0F, 0F, 1F, 1F);
            return Size.Zero;
        }
    }
}
