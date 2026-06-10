using System.Globalization;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class TableCellControlTests
{
    [Fact]
    public async Task XmlParametersAreApplied()
    {
        var control = await """
                            <td
                                background="red"
                                borderThickness="1px 2px 3px 4px"
                                borderColor="blue">
                                  <mock width="100px" height="100px"/>
                            </td>
                            """.ToControl<TableCellControl>();

        Assert.Equal(Colors.Red, control.Background);
        Assert.Equal(new Thickness(1F, 2F, 3F, 4F), control.BorderThickness);
        Assert.Equal(Colors.Blue, control.BorderColor);
    }

    [Fact]
    public async Task BorderThicknessReservesSpaceAroundCellContent()
    {
        var control = await """
                            <td
                                horizontalAlignment="Left"
                                verticalAlignment="Top"
                                borderThickness="1px 2px 3px 4px"
                                borderColor="red">
                                  <mock width="10px" height="20px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};

        var measure = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(14, 26), measure);
        Assert.Equal(new Size(14, 26), arrange);
        mockCanvas.AssertState();
        mockCanvas.AssertClip(new Rectangle(0, 0, 14, 26));
        mockCanvas.AssertDrawRect(
            (new Rectangle(0, 0, 1, 26), Colors.Red),
            (new Rectangle(0, 0, 14, 2), Colors.Red),
            (new Rectangle(11, 0, 3, 26), Colors.Red),
            (new Rectangle(0, 22, 14, 4), Colors.Red));
    }

    [Fact]
    public async Task SingleCellContentMatchesSize()
    {
        var control = await """
                            <td horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                  <mock width="100px" height="100px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(200, 200), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 200, 200));
    }

    [Fact]
    public void RenderExpandsCellClipWhenTextLineMovesToNextPage()
    {
        var pageBounds = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var textControl = new TextControl(new FixedTextLayoutService())
        {
            Text = "cell",
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        var control = new TableCellControl
        {
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        control.Add(textControl);
        var textStyle = textControl.GetTextStyle();
        mockCanvas.Translate(new Point(0F, 95F));

        control.Measure(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Arrange(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(mockCanvas, 90, pageBounds, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 5F), additionalSize);
        mockCanvas.AssertState();
        mockCanvas.AssertClip(0, new Rectangle(0F, 95F, 10F, 15F));
        mockCanvas.AssertDrawText(textStyle, "cell", 0F, 108F);
    }

    [Fact]
    public async Task SingleCellContentMatchesSizeWithPadding()
    {
        var control = await """
                            <td padding="10px" horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                  <mock width="100px" height="100px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(120, 120), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(200, 200), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 200, 200));
    }

    [Fact]
    public async Task SingleCellContentMatchesSizeWithPaddingAndMargin()
    {
        var control = await """
                            <td padding="10px" margin="10px" horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                  <mock width="100px" height="100px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(140, 140), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(200, 200), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(10, 10, 180, 180));
    }

    [Fact]
    public async Task CellControlWithTwoChildrenStacksVertical()
    {
        var control = await """
                            <td horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                  <mock width="100px" height="100px"/>
                                  <mock width="100px" height="100px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 200), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(200, 200), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 200, 200));
    }
    
    [Fact]
    public async Task SingleCellContentWithLeftAndTopCellControl()
    {
        var control = await """
                            <td horizontalAlignment="Left" verticalAlignment="Top">
                                  <mock width="100px" height="100px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 100));
    }
    
    [Fact]
    public async Task SingleCellContentWithRightAndBottomCellControl()
    {
        var control = await """
                            <td horizontalAlignment="Right" verticalAlignment="Bottom">
                                  <mock width="100px" height="100px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(100, 100, 100, 100));
    }
    
    [Fact]
    public async Task SingleCellContentWithCenterCellControl()
    {
        var control = await """
                            <td horizontalAlignment="Center" verticalAlignment="Center">
                                  <mock width="100px" height="100px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(50, 50, 100, 100));
    }
    
    [Fact]
    public async Task CellControlWithTwoChildrenStacksVerticallyLeftAndTop()
    {
        var control = await """
                            <td horizontalAlignment="Left" verticalAlignment="Top">
                                  <mock width="50px" height="50px"/>
                                  <mock width="50px" height="50px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 50, 100));
    }
    
    [Fact]
    public async Task CellControlWithTwoChildrenStacksVerticallyRightAndBottom()
    {
        var control = await """
                            <td horizontalAlignment="Right" verticalAlignment="Bottom">
                                  <mock width="50px" height="50px"/>
                                  <mock width="50px" height="50px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(150, 100, 50, 100));
    }
    
    [Fact]
    public async Task CellControlWithTwoChildrenStacksVerticallyCenter()
    {
        var control = await """
                            <td horizontalAlignment="Center" verticalAlignment="Center">
                                  <mock width="50px" height="50px"/>
                                  <mock width="50px" height="50px"/>
                            </td>
                            """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(75, 50, 50, 100));
    }
}
