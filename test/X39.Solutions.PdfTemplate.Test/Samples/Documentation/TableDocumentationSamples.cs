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
                        <th borderThickness="0 0 0 1pt" borderColor="#475569">
                            <td padding="1mm">
                                <text fontsize="9" weight="bold">Item</text>
                            </td>
                            <td padding="1mm">
                                <text fontsize="9" weight="bold" horizontalAlignment="right">Qty</text>
                            </td>
                            <td padding="1mm">
                                <text fontsize="9" weight="bold" horizontalAlignment="right">Total</text>
                            </td>
                        </th>
                        <tr>
                            <td padding="1mm"><text fontsize="9">Design</text></td>
                            <td padding="1mm"><text fontsize="9" horizontalAlignment="right">2</text></td>
                            <td padding="1mm"><text fontsize="9" horizontalAlignment="right">300.00</text></td>
                        </tr>
                        <tr>
                            <td padding="1mm"><text fontsize="9">Review</text></td>
                            <td padding="1mm"><text fontsize="9" horizontalAlignment="right">1</text></td>
                            <td padding="1mm"><text fontsize="9" horizontalAlignment="right">120.00</text></td>
                        </tr>
                    </table>
                </body>
            </template>
            """);

    [Fact]
    public Task Table_TwoColumnLayout()
        => RenderDocumentationSampleAsync(
            "table-two-column-layout",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <table>
                        <tr>
                            <td
                                width="1*"
                                borderThickness="1pt"
                                borderColor="#cbd5e1"
                                background="#f8fafc"
                                padding="2mm"
                                verticalAlignment="top">
                                <text fontsize="9" weight="bold">Bill to</text>
                                <text fontsize="8">Mira Lane</text>
                                <text fontsize="8">42 Market Street</text>
                            </td>
                            <td
                                width="1*"
                                borderThickness="1pt"
                                borderColor="#67e8f9"
                                background="#ecfeff"
                                padding="2mm"
                                verticalAlignment="top">
                                <text fontsize="9" weight="bold">Ship to</text>
                                <text fontsize="8">Warehouse North</text>
                                <text fontsize="8">Dock 3</text>
                            </td>
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
                            <td width="2*" background="#e0f2fe" padding="1mm">
                                <text fontsize="9">Description uses 2*</text>
                            </td>
                            <td width="1*" background="#fef3c7" padding="1mm">
                                <text fontsize="9">Code uses 1*</text>
                            </td>
                            <td width="20mm" background="#dcfce7" padding="1mm">
                                <text fontsize="9" horizontalAlignment="right">20 mm</text>
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
                        <th borderThickness="0 0 0 1pt" borderColor="#334155">
                            <td width="2*" padding="1mm">
                                <text fontsize="9" weight="bold">Task</text>
                            </td>
                            <td width="1*" padding="1mm">
                                <text fontsize="9" weight="bold" horizontalAlignment="right">Hours</text>
                            </td>
                        </th>
                        @alternate on RowBackground with ["#f8fafc", "#e2e8f0"] {
                        <tr background="@RowBackground">
                            <td padding="1mm"><text fontsize="9">Draft</text></td>
                            <td padding="1mm"><text fontsize="9" horizontalAlignment="right">3.5</text></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr background="@RowBackground">
                            <td padding="1mm"><text fontsize="9">Revise</text></td>
                            <td padding="1mm"><text fontsize="9" horizontalAlignment="right">1.0</text></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr background="@RowBackground">
                            <td padding="1mm"><text fontsize="9">Approve</text></td>
                            <td padding="1mm"><text fontsize="9" horizontalAlignment="right">0.5</text></td>
                        </tr>
                        }
                    </table>
                </body>
            </template>
            """);

    [Fact]
    public Task Table_RepeatedDataRows()
        => RenderDocumentationSampleAsync(
            "table-repeated-data-rows",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <table>
                        <th borderThickness="0 0 0 1pt" borderColor="#334155">
                            <td width="2*" padding="1mm">
                                <text fontsize="9" weight="bold">Task from data</text>
                            </td>
                            <td width="1*" padding="1mm">
                                <text fontsize="9" weight="bold" horizontalAlignment="right">Status</text>
                            </td>
                        </th>
                        @foreach TaskName in Tasks {
                        @alternate on RowBackground with ["#ffffff", "#f1f5f9"] {
                        <tr background="@RowBackground">
                            <td padding="1mm"><text fontsize="9">@TaskName</text></td>
                            <td padding="1mm"><text fontsize="9" horizontalAlignment="right">Ready</text></td>
                        </tr>
                        }
                        }
                    </table>
                </body>
            </template>
            """,
            configureGenerator: (generator) =>
            {
                generator.TemplateData.SetVariable("Tasks", new[] {"Draft", "Review", "Approve"});
            });
}
