using System.Globalization;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class TableControlTest
{
    [Fact]
    public async Task TableWith2X100PxLinesWillScaleToFullPageSize()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td><line thickness="100px" length="100px"/></td>
                                      <td><line thickness="100px" length="100px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200, 100)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200, 100)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100, 100)); // td
        mockCanvas.AssertClip(3, new Rectangle(0,   0, 100, 100)); // line
        mockCanvas.AssertClip(4, new Rectangle(100, 0, 100, 100)); // td
        mockCanvas.AssertClip(5, new Rectangle(100, 0, 100, 100)); // line
    }

    [Fact]
    public async Task TableWith2X200PxLinesWillScaleToFullPageSize()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td><line thickness="100px" length="2000px"/></td>
                                      <td><line thickness="100px" length="2000px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200,  100)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200,  100)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100, 100)); // td
        mockCanvas.AssertClip(3, new Rectangle(0,   0, 2000, 100)); // line
        mockCanvas.AssertClip(4, new Rectangle(100, 0, 100, 100)); // td
        mockCanvas.AssertClip(5, new Rectangle(100, 0, 2000, 100)); // line
    }

    [Fact]
    public async Task TableWith2X50PxLinesWillScaleToFullPageSize()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td><line thickness="100px" length="50px"/></td>
                                      <td><line thickness="100px" length="50px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200, 100)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200, 100)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100, 100)); // td
        mockCanvas.AssertClip(3, new Rectangle(0,   0, 50,  100)); // line
        mockCanvas.AssertClip(4, new Rectangle(100, 0, 100, 100)); // td
        mockCanvas.AssertClip(5, new Rectangle(100, 0, 50,  100)); // line
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public async Task TableWithEmptyColsWillScaleToFullPageSizeInSum(int amount)
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                    @for i from 0 to {{amount}} {
                                        <td></td>
                                      }
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 200, 0)); // table
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 200, 0)); // tr
        for (var i = 0; i < amount; i++)
        {
            var width = 200F / amount;
            mockCanvas.AssertClip(2 + i, new Rectangle(width * i, 0, width, 0)); // td
        }
    }

    [Fact]
    public async Task TableWith2EmptyColsWillScaleToFullPageSizeEachColHalf()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td></td>
                                      <td></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200, 0)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200, 0)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100, 0)); // td
        mockCanvas.AssertClip(3, new Rectangle(100, 0, 100, 0)); // td
    }

    [Fact]
    public async Task UnbreakableTextInNarrowCellIsRenderedForClipping()
    {
        const string text = "thisdoesnotappearontheoutputdocumentviually";
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td width="1*" background="red"></td>
                                      <td width="2*" background="green"></td>
                                      <td width="Auto" background="yellow"></td>
                                      <td width="1cm" background="cyan"><text>{{text}}</text></td>
                                      <td width="2cm" background="wheat"></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};

        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        mockCanvas.AssertState();
        mockCanvas.AssertAnyDrawTextContains(text);
    }

    [Fact]
    public async Task LaterAutoCellsDoNotOverrideEarlierFixedColumnWidth()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td width="40px"><mock width="1px" height="10px"/></td>
                                      <td width="1*"><mock width="1px" height="10px"/></td>
                                  </tr>
                                  <tr>
                                      <td><mock width="1px" height="10px"/></td>
                                      <td><mock width="1px" height="10px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0,  0, 200, 20)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,  0, 200, 10)); // first tr
        mockCanvas.AssertClip(2, new Rectangle(0,  0, 40,  10)); // fixed-width td
        mockCanvas.AssertClip(3, new Rectangle(40, 0, 160, 10)); // star-width td
        mockCanvas.AssertClip(4, new Rectangle(0,  10, 200, 10)); // second tr
        mockCanvas.AssertClip(5, new Rectangle(0,  10, 40,  10)); // auto cell using fixed column width
        mockCanvas.AssertClip(6, new Rectangle(40, 10, 160, 10)); // auto cell using star column width
    }

    [Fact]
    public async Task RightAlignedContentIsNotClippedAway()
    {
        var control = await $$"""
                                <table>
                                    <tr>
                                     	<td><line horizontalAlignment="right" length="20px" thickness="20px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200, 20)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200, 20)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 200, 20)); // td
        mockCanvas.AssertClip(3, new Rectangle(180, 0, 20,  20)); // line
    }

    [Fact]
    public async Task RowBackgroundSpansArrangedRow()
    {
        var control = await $$"""
                              <table>
                                  <tr background="red">
                                      <td><mock width="50px" height="10px"/></td>
                                      <td><mock width="50px" height="10px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};

        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        mockCanvas.AssertState();
        mockCanvas.AssertDrawRect((new Rectangle(0, 0, 100, 10), Colors.Red));
    }

    [Fact]
    public async Task CellBackgroundSpansArrangedCell()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td background="blue"><mock width="50px" height="10px"/></td>
                                      <td><mock width="50px" height="10px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};

        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        mockCanvas.AssertState();
        mockCanvas.AssertDrawRect((new Rectangle(0, 0, 50, 10), Colors.Blue));
    }

    [Fact]
    public async Task CellBackgroundRendersAfterRowBackground()
    {
        var control = await $$"""
                              <table>
                                  <tr background="red">
                                      <td background="blue"><mock width="50px" height="10px"/></td>
                                      <td><mock width="50px" height="10px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};

        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        mockCanvas.AssertState();
        mockCanvas.AssertDrawRect(
            (new Rectangle(0, 0, 100, 10), Colors.Red),
            (new Rectangle(0, 0, 50, 10), Colors.Blue));
    }

    [Fact]
    public async Task RowBorderContributesToRowHeight()
    {
        var control = await $$"""
                              <table>
                                  <tr borderThickness="0 2px 0 3px" borderColor="red">
                                      <td><mock width="100px" height="10px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};

        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var arrangedSize = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        mockCanvas.AssertState();
        Assert.Equal(new Size(100, 15), arrangedSize);
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 15)); // table
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 100, 15)); // row
        mockCanvas.AssertClip(2, new Rectangle(0, 2, 100, 10)); // row border offsets the cell content
        mockCanvas.AssertDrawRect(
            (new Rectangle(0, 0, 100, 2), Colors.Red),
            (new Rectangle(0, 12, 100, 3), Colors.Red));
    }

    [Fact]
    public async Task HeaderBorderContributesToRepeatedHeaderHeight()
    {
        var control = await $$"""
                              <table>
                                  <th borderThickness="0 0 0 10px" borderColor="red">
                                      <td><mock width="100px" height="10px"/></td>
                                  </th>
                                  <tr>
                                      <td><mock width="100px" height="80px"/></td>
                                  </tr>
                                  <tr>
                                      <td><mock width="100px" height="20px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};

        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var arrangedSize         = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var additionalRenderSize = control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        mockCanvas.AssertState();
        Assert.Equal(new Size(100, 120), arrangedSize);
        Assert.Equal(new Size(0, 20), additionalRenderSize);
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 140)); // table, including repeated header space
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 100, 20)); // initial table header
        mockCanvas.AssertClip(5, new Rectangle(0, 100, 100, 20)); // repeated header
        mockCanvas.AssertDrawRect(
            (new Rectangle(0, 10, 100, 10), Colors.Red),
            (new Rectangle(0, 110, 100, 10), Colors.Red));
    }

    [Fact]
    public async Task PaddedCellClipDoesNotExtendIntoNextColumn()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td width="100px" padding="10px"><line thickness="10px" length="200px"/></td>
                                      <td width="1*" padding="10px"><line thickness="10px" length="10px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200, 30)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200, 30)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100, 30)); // first td
        mockCanvas.AssertClip(4, new Rectangle(100, 0, 100, 30)); // second td
    }

    [Fact]
    public async Task RowHeightCorrectlyAdjustsForTableHeightExceedingPage()
    {
        var control = await $$"""
                                <table>
                                    <tr>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                    </tr>
                                    <tr>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                    </tr>
                                    <tr>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 125);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 125)); // table
        // Assert that the first row is 25px high and 100px wide
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 100, 25)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0, 0, 25,  25)); // td
        mockCanvas.AssertClip(3, new Rectangle(25, 0, 25,  25)); // td
        mockCanvas.AssertClip(4, new Rectangle(50, 0, 25,  25)); // td
        mockCanvas.AssertClip(5, new Rectangle(75, 0, 25,  25)); // td
        // Assert that the second row is 50px high and 100px wide
        mockCanvas.AssertClip(6, new Rectangle(0, 25, 100, 50)); // tr
        mockCanvas.AssertClip(7, new Rectangle(0, 25, 25,  50)); // td
        mockCanvas.AssertClip(8, new Rectangle(25, 25, 25,  50)); // td
        mockCanvas.AssertClip(9, new Rectangle(50, 25, 25,  50)); // td
        mockCanvas.AssertClip(10, new Rectangle(75, 25, 25,  50)); // td
        // Assert that the third row is 50px high and 100px wide
        mockCanvas.AssertClip(11, new Rectangle(0, 75, 100, 50)); // tr
        mockCanvas.AssertClip(12, new Rectangle(0, 75, 25,  50)); // td
        mockCanvas.AssertClip(13, new Rectangle(25, 75, 25,  50)); // td
        mockCanvas.AssertClip(14, new Rectangle(50, 75, 25,  50)); // td
        mockCanvas.AssertClip(15, new Rectangle(75, 75, 25,  50)); // td
    }

    [Fact]
    public async Task RowHeightCorrectlyAdjustsForPageFittingTable()
    {
        var control = await $$"""
                                <table>
                                    <tr>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                    </tr>
                                    <tr>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                    </tr>
                                    <tr>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 125);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 125)); // table
        // Assert that the first row is 25px high and 100px wide
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 100, 25)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0, 0, 25,  25)); // td
        mockCanvas.AssertClip(3, new Rectangle(25, 0, 25,  25)); // td
        mockCanvas.AssertClip(4, new Rectangle(50, 0, 25,  25)); // td
        mockCanvas.AssertClip(5, new Rectangle(75, 0, 25,  25)); // td
        // Assert that the second row is 50px high and 100px wide
        mockCanvas.AssertClip(6, new Rectangle(0, 25, 100, 50)); // tr
        mockCanvas.AssertClip(7, new Rectangle(0, 25, 25,  50)); // td
        mockCanvas.AssertClip(8, new Rectangle(25, 25, 25,  50)); // td
        mockCanvas.AssertClip(9, new Rectangle(50, 25, 25,  50)); // td
        mockCanvas.AssertClip(10, new Rectangle(75, 25, 25,  50)); // td
        // Assert that the third row is 50px high and 100px wide
        mockCanvas.AssertClip(11, new Rectangle(0, 75, 100, 50)); // tr
        mockCanvas.AssertClip(12, new Rectangle(0, 75, 25,  50)); // td
        mockCanvas.AssertClip(13, new Rectangle(25, 75, 25,  50)); // td
        mockCanvas.AssertClip(14, new Rectangle(50, 75, 25,  50)); // td
        mockCanvas.AssertClip(15, new Rectangle(75, 75, 25,  50)); // td
    }

    [Fact]
    public async Task RowTallerThanPageStartsAfterRepeatedHeaderAndIsNotSplit()
    {
        var control = await $$"""
                                <table>
                                    <th>
                                        <td><mock width="100px" height="10px"/></td>
                                    </th>
                                    <tr>
                                        <td><mock width="100px" height="120px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var arrangedSize = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var additionalRenderSize = control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();

        Assert.Equal(new Size(100, 130), arrangedSize);
        Assert.Equal(new Size(0, 100), additionalRenderSize);
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 230)); // table, including the skipped page space
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 100, 10)); // initial table header
        mockCanvas.AssertClip(2, new Rectangle(0, 0, 100, 10)); // initial header cell
        mockCanvas.AssertClip(3, new Rectangle(0, 100, 100, 10)); // repeated header on the next page
        mockCanvas.AssertClip(4, new Rectangle(0, 100, 100, 10)); // repeated header cell
        mockCanvas.AssertClip(5, new Rectangle(0, 110, 100, 120)); // oversized row remains one row
        mockCanvas.AssertClip(6, new Rectangle(0, 110, 100, 120)); // oversized row cell
    }

    [Fact]
    public void OversizedRowExpandsTableClipWhenNestedCellTextMovesToNextPage()
    {
        var textControl = new TextControl(
            new FixedTextLayoutService(
                lineHeight: 15F,
                baselineOffset: 10F,
                lineTopOffset: -5F))
        {
            Text = "cell",
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        var cell = new TableCellControl
        {
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        cell.Add(new SpacerControl { Height = 95F });
        cell.Add(textControl);
        var row = new TableRowControl
        {
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        row.Add(cell);
        var control = new TableControl
        {
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        control.Add(row);
        var pageSize = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var textStyle = textControl.GetTextStyle();

        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var additionalRenderSize = control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 110F), additionalRenderSize);
        mockCanvas.AssertState();
        mockCanvas.AssertClip(0, new Rectangle(0F, 0F, 100F, 220F));
        mockCanvas.AssertDrawText(textStyle, "cell", 0F, 215F);
    }

    [Fact]
    public async Task HeaderIsRepeatedWhenPreviousRowEndsExactlyAtPageBoundary()
    {
        var control = await $$"""
                                <table>
                                    <th>
                                        <td><mock width="100px" height="10px"/></td>
                                    </th>
                                    <tr>
                                        <td><mock width="100px" height="45px"/></td>
                                    </tr>
                                    <tr>
                                        <td><mock width="100px" height="45px"/></td>
                                    </tr>
                                    <tr>
                                        <td><mock width="100px" height="45px"/></td>
                                    </tr>
                                    <tr>
                                        <td><mock width="100px" height="45px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var arrangedSize         = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var additionalRenderSize = control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();

        Assert.Equal(new Size(100, 190), arrangedSize);
        Assert.Equal(new Size(0, 10), additionalRenderSize);
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 200)); // table, including repeated header space
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 100, 10)); // initial table header
        mockCanvas.AssertClip(2, new Rectangle(0, 0, 100, 10)); // initial header cell
        mockCanvas.AssertClip(3, new Rectangle(0, 10, 100, 45)); // first row
        mockCanvas.AssertClip(4, new Rectangle(0, 10, 100, 45)); // first row cell
        mockCanvas.AssertClip(5, new Rectangle(0, 55, 100, 45)); // second row
        mockCanvas.AssertClip(6, new Rectangle(0, 55, 100, 45)); // second row cell
        mockCanvas.AssertClip(7, new Rectangle(0, 100, 100, 10)); // repeated header at page boundary
        mockCanvas.AssertClip(8, new Rectangle(0, 100, 100, 10)); // repeated header cell
        mockCanvas.AssertClip(9, new Rectangle(0, 110, 100, 45)); // third row
        mockCanvas.AssertClip(10, new Rectangle(0, 110, 100, 45)); // third row cell
        mockCanvas.AssertClip(11, new Rectangle(0, 155, 100, 45)); // fourth row
        mockCanvas.AssertClip(12, new Rectangle(0, 155, 100, 45)); // fourth row cell
    }

    [Fact]
    public async Task HeaderIsNotRepeatedWhenNextRowWouldNotFitBelowIt()
    {
        var control = await $$"""
                                <table>
                                    <th>
                                        <td><mock width="100px" height="95px"/></td>
                                    </th>
                                    <tr>
                                        <td><mock width="100px" height="10px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var arrangedSize         = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        var additionalRenderSize = control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();

        Assert.Equal(new Size(100, 105), arrangedSize);
        Assert.Equal(new Size(0, 5), additionalRenderSize);
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 110)); // table, including only the page-break gap
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 100, 95)); // initial table header
        mockCanvas.AssertClip(2, new Rectangle(0, 0, 100, 95)); // initial header cell
        mockCanvas.AssertClip(3, new Rectangle(0, 100, 100, 10)); // first row starts at the next page
        mockCanvas.AssertClip(4, new Rectangle(0, 100, 100, 10)); // first row cell
    }

    [Fact]
    public async Task InitialHeaderMovesWithFirstRowWhenRemainingPageCannotFitBoth()
    {
        var control = await $$"""
                                <table>
                                    <th>
                                        <td><mock width="100px" height="10px"/></td>
                                    </th>
                                    <tr>
                                        <td><mock width="100px" height="10px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.Translate(new Point(0F, 85F));

        var additionalRenderSize = control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        mockCanvas.AssertState();
        Assert.Equal(new Size(0, 15), additionalRenderSize);
        Assert.Equal(5, mockCanvas.ClipCount);
        mockCanvas.AssertClip(0, new Rectangle(0, 85, 100, 35)); // table, including the page-break gap
        mockCanvas.AssertClip(1, new Rectangle(0, 100, 100, 10)); // initial table header moved to the next page
        mockCanvas.AssertClip(2, new Rectangle(0, 100, 100, 10)); // initial header cell
        mockCanvas.AssertClip(3, new Rectangle(0, 110, 100, 10)); // first row follows the initial header
        mockCanvas.AssertClip(4, new Rectangle(0, 110, 100, 10)); // first row cell
    }

    [Fact]
    public async Task InitialHeaderUsesOuterPageFrameWhenTableHasBottomMargin()
    {
        var control = await $$"""
                                <table margin="0 0 0 20px">
                                    <th>
                                        <td><mock width="100px" height="10px"/></td>
                                    </th>
                                    <tr>
                                        <td><mock width="100px" height="20px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.Translate(new Point(0F, 80F));

        var additionalRenderSize = control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        mockCanvas.AssertState();
        Assert.Equal(new Size(0, 20), additionalRenderSize);
        Assert.Equal(5, mockCanvas.ClipCount);
        mockCanvas.AssertClip(0, new Rectangle(0, 80, 100, 50)); // table, including page-break gap and margin
        mockCanvas.AssertClip(1, new Rectangle(0, 100, 100, 10)); // initial table header moved to the next page
        mockCanvas.AssertClip(2, new Rectangle(0, 100, 100, 10)); // initial header cell
        mockCanvas.AssertClip(3, new Rectangle(0, 110, 100, 20)); // first row follows the initial header
        mockCanvas.AssertClip(4, new Rectangle(0, 110, 100, 20)); // first row cell
    }

    [Fact]
    public void RowMovesBeforeRenderingWhenCellTextWouldPaginate()
    {
        var textControl = new TextControl(
            new FixedTextLayoutService(
                lineHeight: 15F,
                baselineOffset: 10F,
                lineTopOffset: 5F))
        {
            Text = "cell",
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        var cell = new TableCellControl
        {
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        cell.Add(textControl);
        var row = new TableRowControl
        {
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        row.Add(cell);
        var control = new TableControl
        {
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        control.Add(row);
        var pageSize   = new Size(100, 100);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var textStyle  = textControl.GetTextStyle();

        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.Translate(new Point(0F, 85F));

        var additionalRenderSize = control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);

        mockCanvas.AssertState();
        Assert.Equal(new Size(0F, 15F), additionalRenderSize);
        mockCanvas.AssertClip(0, new Rectangle(0F, 85F, 100F, 30F)); // table, including the page-break gap
        mockCanvas.AssertClip(1, new Rectangle(0F, 100F, 100F, 15F)); // row moved before cell text rendered
        mockCanvas.AssertClip(2, new Rectangle(0F, 100F, 10F, 15F)); // cell moved with the row
        mockCanvas.AssertDrawText(textStyle, "cell", 0F, 110F);
    }
}
