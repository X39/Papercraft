using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class FlowControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(100, 100);

    [Fact]
    public async Task SpacerXmlParametersAreApplied()
    {
        var control = await """
                            <spacer
                                width="50%"
                                height="8mm"
                                padding="1px 2px"
                                margin="3px"
                                clip="false"
                                horizontalAlignment="right"
                                verticalAlignment="bottom"/>
                            """.ToControl<SpacerControl>();

        Assert.Equal(new Length(0.5F, ELengthUnit.Percent), control.Width);
        Assert.Equal(new Length(8F, ELengthUnit.Millimeters), control.Height);
        Assert.Equal(new Thickness(1F, 2F), control.Padding);
        Assert.Equal(new Thickness(3F), control.Margin);
        Assert.False(control.Clip);
        Assert.Equal(EHorizontalAlignment.Right, control.HorizontalAlignment);
        Assert.Equal(EVerticalAlignment.Bottom, control.VerticalAlignment);
    }

    [Fact]
    public void SpacerMeasuresAndArrangesConfiguredSize()
    {
        var control = new SpacerControl
        {
            Width = 50F,
            Height = 20F,
        };

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(50, 20), measured);
        Assert.Equal(new Size(50, 20), arranged);
        Assert.Equal(new Rectangle(0, 0, 50, 20), control.Arrangement);
        Assert.Equal(new Rectangle(0, 0, 50, 20), control.ArrangementInner);
    }

    [Fact]
    public void SpacerUsesFullWidthDefaultAndIncludesMarginAndPadding()
    {
        var control = new SpacerControl
        {
            Height = 20F,
            Margin = new Thickness(5F),
            Padding = new Thickness(10F),
        };

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(100, 50), measured);
        Assert.Equal(new Size(100, 50), arranged);
        Assert.Equal(new Rectangle(5, 5, 90, 40), control.Arrangement);
        Assert.Equal(new Rectangle(15, 15, 70, 20), control.ArrangementInner);
    }

    [Fact]
    public void SpacerRendersNoDrawCalls()
    {
        var canvas = CreateCanvas();
        var control = new SpacerControl
        {
            Height = 20F,
        };

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(Size.Zero, additionalSize);
        Assert.Equal(0, canvas.DrawLineCount);
        Assert.Equal(0, canvas.DrawRectCount);
        Assert.Equal(0, canvas.DrawTextCount);
        Assert.Equal(0, canvas.DrawBitmapCount);
        canvas.AssertState();
    }

    [Fact]
    public async Task PageBreakXmlCreatesRegisteredControl()
    {
        var control = await "<pageBreak/>".ToControl<PageBreakControl>();

        Assert.NotNull(control);
    }

    [Theory]
    [InlineData(0F, 0F)]
    [InlineData(35F, 65F)]
    [InlineData(100F, 0F)]
    public void PageBreakReturnsRemainingPageHeightWhenRendered(float currentY, float expectedAdditionalHeight)
    {
        var canvas = CreateCanvas();
        canvas.Translate(new Point(0F, currentY));
        var control = new PageBreakControl();

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(Size.Zero, measured);
        Assert.Equal(Size.Zero, arranged);
        Assert.Equal(new Size(0, expectedAdditionalHeight), additionalSize);
        Assert.Equal(new Point(0, currentY), canvas.Translation);
        Assert.Equal(0, canvas.DrawLineCount);
        Assert.Equal(0, canvas.DrawRectCount);
        Assert.Equal(0, canvas.DrawTextCount);
        Assert.Equal(0, canvas.DrawBitmapCount);
        canvas.AssertState();
    }

    [Fact]
    public async Task PageBreakCreatesSecondRasterPageInBodyFlow()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();
        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        var pages = new List<MemoryStream>();
        using var reader = CreateReader(
            """
            <spacer height="1px"/>
            <pageBreak/>
            <spacer height="1px"/>
            """);

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
}
