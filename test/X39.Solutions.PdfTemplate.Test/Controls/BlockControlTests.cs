using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class BlockControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(100, 100);

    [Fact]
    public async Task XmlParametersAreApplied()
    {
        var control = await """
                            <block
                                background="green"
                                minHeight="12px"
                                pageBreakBefore="true"
                                pageBreakAfter="true"
                                keepTogether="true"
                                padding="2px"
                                margin="3px"
                                clip="false"
                                horizontalAlignment="right"
                                verticalAlignment="bottom">
                                <mock width="10px" height="20px"/>
                            </block>
                            """.ToControl<BlockControl>();

        Assert.Equal(Colors.Green, control.Background);
        Assert.Equal(new Length(12F, ELengthUnit.Pixel), control.MinHeight);
        Assert.True(control.PageBreakBefore);
        Assert.True(control.PageBreakAfter);
        Assert.True(control.KeepTogether);
        Assert.Equal(new Thickness(2F), control.Padding);
        Assert.Equal(new Thickness(3F), control.Margin);
        Assert.False(control.Clip);
        Assert.Equal(EHorizontalAlignment.Right, control.HorizontalAlignment);
        Assert.Equal(EVerticalAlignment.Bottom, control.VerticalAlignment);
        Assert.Single(control.Children);
    }

    [Fact]
    public void CanAddAllowsAnyControlType()
    {
        var control = new BlockControl();

        Assert.True(control.CanAdd(typeof(TextControl)));
        Assert.True(control.CanAdd(typeof(BorderControl)));
        Assert.True(control.CanAdd(typeof(MockControl)));
    }

    [Fact]
    public void MeasureAndArrangeStackChildrenVertically()
    {
        var control = CreateBlock();
        control.Add(new FixedSizeControl(new Size(80F, 20F)));
        control.Add(new FixedSizeControl(new Size(60F, 30F)));

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(80F, 50F), measured);
        Assert.Equal(new Size(80F, 50F), arranged);
        Assert.Equal(new Rectangle(0F, 0F, 80F, 50F), control.Arrangement);
        Assert.Equal(new Rectangle(0F, 0F, 80F, 50F), control.ArrangementInner);
    }

    [Fact]
    public void MeasureAndArrangeHonorMinHeight()
    {
        var control = CreateBlock();
        control.MinHeight = 60F;
        control.Add(new FixedSizeControl(new Size(20F, 10F)));

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(20F, 60F), measured);
        Assert.Equal(new Size(20F, 60F), arranged);
    }

    [Fact]
    public void RenderDrawsBackgroundBeforeChildren()
    {
        var control = CreateBlock();
        control.Background = Colors.Blue;
        control.Add(new BackgroundAwareDrawingControl(new Size(20F, 10F), Colors.Black));
        var canvas = CreateCanvas();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(Size.Zero, additionalSize);
        canvas.AssertState();
        canvas.AssertDrawRect((new Rectangle(0F, 0F, 20F, 10F), Colors.Blue));
        canvas.AssertDrawLine((Colors.Black, 1F, 0F, 0F, 1F, 1F));
    }

    [Fact]
    public void RenderSkipsTransparentBackground()
    {
        var control = CreateBlock();
        control.Background = Colors.Transparent;
        control.Add(new DrawingControl(new Size(20F, 10F), Colors.Black));
        var canvas = CreateCanvas();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(0, canvas.DrawRectCount);
        canvas.AssertDrawLine((Colors.Black, 1F, 0F, 0F, 1F, 1F));
    }

    [Fact]
    public void AdditionalChildRenderHeightShiftsFollowingChildrenAndIsReturned()
    {
        var control = CreateBlock();
        control.Add(new AdditionalHeightControl(new Size(10F, 10F), 15F, Colors.Black));
        control.Add(new DrawingControl(new Size(10F, 10F), Colors.Magenta));
        var canvas = CreateCanvas();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 15F), additionalSize);
        canvas.AssertState();
        canvas.AssertDrawLine(
            (Colors.Black, 1F, 0F, 0F, 1F, 1F),
            (Colors.Magenta, 1F, 0F, 25F, 1F, 26F));
    }

    [Fact]
    public void NestedPageBreakAdditionalHeightShiftsFollowingChildren()
    {
        var inner = CreateBlock();
        inner.PageBreakAfter = true;
        inner.Add(new DrawingControl(new Size(10F, 35F), Colors.Black));

        var outer = CreateBlock();
        outer.Add(inner);
        outer.Add(new DrawingControl(new Size(10F, 10F), Colors.Magenta));
        var canvas = CreateCanvas();

        outer.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = outer.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 65F), additionalSize);
        canvas.AssertState();
        canvas.AssertDrawLine(
            (Colors.Black, 1F, 0F, 0F, 1F, 1F),
            (Colors.Magenta, 1F, 0F, 100F, 1F, 101F));
    }

    [Fact]
    public void PageBreakBeforeMovesContentToNextPageAndReturnsAdditionalHeight()
    {
        var control = CreateBlock();
        control.PageBreakBefore = true;
        control.Add(new DrawingControl(new Size(10F, 10F), Colors.Black));
        var canvas = CreateCanvas();
        canvas.Translate(0F, 35F);

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 65F), additionalSize);
        Assert.Equal(new Point(0F, 35F), canvas.Translation);
        canvas.AssertDrawLine((Colors.Black, 1F, 0F, 100F, 1F, 101F));
    }

    [Fact]
    public void PageBreakBeforeDoesNotCreateBlankPageAtBoundary()
    {
        var control = CreateBlock();
        control.PageBreakBefore = true;
        control.Add(new DrawingControl(new Size(10F, 10F), Colors.Black));
        var canvas = CreateCanvas();
        canvas.Translate(0F, 100F);

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(Size.Zero, additionalSize);
        canvas.AssertDrawLine((Colors.Black, 1F, 0F, 100F, 1F, 101F));
    }

    [Fact]
    public void PageBreakAfterReturnsRemainingHeightAfterChildren()
    {
        var control = CreateBlock();
        control.PageBreakAfter = true;
        control.Add(new DrawingControl(new Size(10F, 35F), Colors.Black));
        var canvas = CreateCanvas();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 65F), additionalSize);
        canvas.AssertState();
        canvas.AssertDrawLine((Colors.Black, 1F, 0F, 0F, 1F, 1F));
    }

    [Fact]
    public void PageBreakAfterDoesNotCreateBlankPageAtBoundary()
    {
        var control = CreateBlock();
        control.PageBreakAfter = true;
        control.Add(new DrawingControl(new Size(10F, 100F), Colors.Black));
        var canvas = CreateCanvas();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(Size.Zero, additionalSize);
    }

    [Theory]
    [InlineData("""
                <spacer height="1px"/>
                <block pageBreakBefore="true">
                    <spacer height="1px"/>
                </block>
                """)]
    [InlineData("""
                <block pageBreakAfter="true">
                    <spacer height="1px"/>
                </block>
                <spacer height="1px"/>
                """)]
    public async Task PageBreakAttributesCreateSecondRasterPage(string body)
    {
        var services = new ServiceCollection();
        services.AddPapercraft();
        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        var pages = new List<MemoryStream>();
        using var reader = CreateReader(body);

        await generator.RenderRasterPagesAsync(
            reader,
            new RasterPageRenderOutput(
                PapercraftMediaTypes.ImagePng,
                (_, _) =>
                {
                    var stream = new MemoryStream();
                    pages.Add(stream);
                    return ValueTask.FromResult<Stream>(stream);
                },
                leaveStreamsOpen: true),
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions
            {
                DocumentOptions = new DocumentOptions
                {
                    DotsPerInch = 25.4F,
                    PageWidthInMillimeters = 10F,
                    PageHeightInMillimeters = 10F,
                },
            });

        Assert.Equal(2, pages.Count);
    }

    private static BlockControl CreateBlock()
        => new()
        {
            Clip = false,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };

    private static DeferredCanvasMock CreateCanvas()
        => new() {ActualPageSize = PageSize, PageSize = PageSize};

    private static XmlReader CreateReader(string body)
    {
        var xml = $$"""
                    <?xml version="1.0" encoding="utf-8"?>
                    <template xmlns="{{Constants.ControlsNamespace}}">
                        <body>
                            {{body}}
                        </body>
                    </template>
                    """;
        return XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
    }

    private sealed class FixedSizeControl(Size size) : IControl
    {
        public Size Measure(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
            => size;

        public Size Arrange(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
            => size;

        public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
            => Size.Zero;
    }

    private class DrawingControl(Size size, Color color) : IControl
    {
        public Size Measure(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
            => size;

        public Size Arrange(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
            => size;

        public virtual Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        {
            canvas.DrawLine(color, 1F, 0F, 0F, 1F, 1F);
            return Size.Zero;
        }
    }

    private sealed class BackgroundAwareDrawingControl(Size size, Color color) : DrawingControl(size, color)
    {
        public override Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        {
            if (canvas is DeferredCanvasMock mock)
                Assert.Equal(1, mock.DrawRectCount);

            return base.Render(canvas, dpi, parentSize, cultureInfo);
        }
    }

    private sealed class AdditionalHeightControl(Size size, float additionalHeight, Color color) : DrawingControl(size, color)
    {
        public override Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        {
            base.Render(canvas, dpi, parentSize, cultureInfo);
            return new Size(0F, additionalHeight);
        }
    }
}
