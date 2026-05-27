namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class TableDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Table_BasicRowsAndColumns()
        => RenderDocumentationSampleAsync(
            "table-basic-rows-and-columns",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <table>
                        <th>
                            <td>
                                <border thickness="0 0 0 1pt" color="#475569" padding="1mm">
                                    <text fontsize="9" weight="bold">Item</text>
                                </border>
                            </td>
                            <td>
                                <border thickness="0 0 0 1pt" color="#475569" padding="1mm">
                                    <text fontsize="9" weight="bold" horizontalAlignment="right">Qty</text>
                                </border>
                            </td>
                            <td>
                                <border thickness="0 0 0 1pt" color="#475569" padding="1mm">
                                    <text fontsize="9" weight="bold" horizontalAlignment="right">Total</text>
                                </border>
                            </td>
                        </th>
                        <tr>
                            <td><border padding="1mm"><text fontsize="9">Design</text></border></td>
                            <td><border padding="1mm"><text fontsize="9" horizontalAlignment="right">2</text></border></td>
                            <td><border padding="1mm"><text fontsize="9" horizontalAlignment="right">300.00</text></border></td>
                        </tr>
                        <tr>
                            <td><border padding="1mm"><text fontsize="9">Review</text></border></td>
                            <td><border padding="1mm"><text fontsize="9" horizontalAlignment="right">1</text></border></td>
                            <td><border padding="1mm"><text fontsize="9" horizontalAlignment="right">120.00</text></border></td>
                        </tr>
                    </table>
                </body>
            </template>
            """);

    [Fact]
    public Task Table_ColumnWidths()
        => RenderDocumentationSampleAsync(
            "table-column-widths",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <table>
                        <tr>
                            <td width="2*">
                                <border background="#e0f2fe" padding="1mm">
                                    <text fontsize="9">Description uses 2*</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border background="#fef3c7" padding="1mm">
                                    <text fontsize="9">Code uses 1*</text>
                                </border>
                            </td>
                            <td width="20mm">
                                <border background="#dcfce7" padding="1mm">
                                    <text fontsize="9" horizontalAlignment="right">20 mm</text>
                                </border>
                            </td>
                        </tr>
                    </table>
                </body>
            </template>
            """);

    [Fact]
    public Task Table_AlternatingRows()
        => RenderDocumentationSampleAsync(
            "table-alternating-rows",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <table>
                        <th>
                            <td width="2*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text fontsize="9" weight="bold">Task</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text fontsize="9" weight="bold" horizontalAlignment="right">Hours</text>
                                </border>
                            </td>
                        </th>
                        @alternate on RowBackground with ["#f8fafc", "#e2e8f0"] {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text fontsize="9">Draft</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text fontsize="9" horizontalAlignment="right">3.5</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text fontsize="9">Revise</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text fontsize="9" horizontalAlignment="right">1.0</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text fontsize="9">Approve</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text fontsize="9" horizontalAlignment="right">0.5</text></border></td>
                        </tr>
                        }
                    </table>
                </body>
            </template>
            """);
}
