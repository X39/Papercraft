using SkiaSharp;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class CompleteExampleDocumentationSamples : DocumentationSampleBase
{
    private static DocumentOptions CompleteExampleDocumentOptions { get; } = new()
    {
        PageWidthInMillimeters = 130,
        PageHeightInMillimeters = 160,
        Margin = new Thickness(new Length(8, ELengthUnit.Millimeters)),
    };

    [Fact]
    public Task CompleteExample_InvoicePreview()
        => RenderDocumentationSampleAsync(
            "complete-invoice-preview",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <template.style>
                    <text fontsize="9" foreground="#111827"/>
                </template.style>
                <header>
                    <text fontsize="20" weight="bold">Invoice INV-1007</text>
                    <text foreground="#475569">Issued 2026-05-27 | Due 2026-06-26</text>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 2mm 0 0"/>
                </header>
                <body>
                    <table margin="0 0 0 5mm">
                        <tr>
                            <td width="1*">
                                <border background="#f8fafc" padding="2mm" margin="0 0 2mm 0">
                                    <text fontsize="8" foreground="#475569">From</text>
                                    <text weight="bold">Northwind Design Studio</text>
                                    <text>100 Market Street</text>
                                    <text>Springfield, IL 62701</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border background="#eff6ff" padding="2mm">
                                    <text fontsize="8" foreground="#475569">Bill to</text>
                                    <text weight="bold">Mira Silva</text>
                                    <text>24 River Road</text>
                                    <text>Madison, WI 53703</text>
                                </border>
                            </td>
                        </tr>
                    </table>

                    <text fontsize="11" weight="bold" margin="0 0 0 1mm">Line items</text>
                    <table>
                        <th>
                            <td width="2*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold">Description</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold" horizontalAlignment="right">Qty</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold" horizontalAlignment="right">Amount</text>
                                </border>
                            </td>
                        </th>
                        @alternate on RowBackground with ["#ffffff", "#f8fafc"] {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>Template design</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">1</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">450.00</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>Review workshop</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">2</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">240.00</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>Print-ready adjustments</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">1</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">275.00</text></border></td>
                        </tr>
                        }
                    </table>

                    <text margin="0 5mm 0 0" horizontalAlignment="right">Subtotal 965.00</text>
                    <text horizontalAlignment="right">Tax 77.20</text>
                    <line thickness="1pt" length="35mm" color="#111827" horizontalAlignment="right" margin="0 1mm"/>
                    <text weight="bold" horizontalAlignment="right">Total 1042.20</text>
                </body>
                <footer>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 0 0 1mm"/>
                    <pageNumber
                        mode="CurrentTotal"
                        prefix="Page "
                        delimiter=" of "
                        fontsize="8"
                        foreground="#64748b"
                        horizontalAlignment="right"/>
                </footer>
            </template>
            """,
            CompleteExampleDocumentOptions);

    [Fact]
    public Task CompleteExample_ReportPreview()
        => RenderDocumentationSampleAsync(
            "complete-report-preview",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <template.style>
                    <text fontsize="9" foreground="#0f172a"/>
                </template.style>
                <header>
                    <text fontsize="18" weight="bold">Operations report</text>
                    <text foreground="#475569">May 2026 summary</text>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 2mm 0 0"/>
                </header>
                <body>
                    <border background="#f8fafc" padding="2mm" margin="0 0 0 5mm" verticalAlignment="top">
                        <text fontsize="8" foreground="#475569">Overview</text>
                        <text>Orders increased while open support items stayed stable.</text>
                    </border>

                    <table margin="0 0 0 5mm">
                        <tr>
                            <td width="1*">
                                <border background="#ecfeff" padding="2mm" margin="0 0 1mm 0">
                                    <text fontsize="8" foreground="#475569">Orders</text>
                                    <text fontsize="16" weight="bold">128</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border background="#f0fdf4" padding="2mm" margin="0 0 1mm 0">
                                    <text fontsize="8" foreground="#475569">Shipments</text>
                                    <text fontsize="16" weight="bold">117</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border background="#fefce8" padding="2mm">
                                    <text fontsize="8" foreground="#475569">Open items</text>
                                    <text fontsize="16" weight="bold">9</text>
                                </border>
                            </td>
                        </tr>
                    </table>

                    <text fontsize="11" weight="bold" margin="0 0 0 1mm">Highlights</text>
                    <table>
                        <th>
                            <td width="2*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold">Area</text>
                                </border>
                            </td>
                            <td width="2*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold">Status</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold" horizontalAlignment="right">Owner</text>
                                </border>
                            </td>
                        </th>
                        <tr>
                            <td><border padding="1mm"><text>Warehouse</text></border></td>
                            <td><border padding="1mm"><text>Picking time improved</text></border></td>
                            <td><border padding="1mm"><text horizontalAlignment="right">Ava</text></border></td>
                        </tr>
                        <tr>
                            <td><border padding="1mm" background="#f8fafc"><text>Support</text></border></td>
                            <td><border padding="1mm" background="#f8fafc"><text>Backlog unchanged</text></border></td>
                            <td><border padding="1mm" background="#f8fafc"><text horizontalAlignment="right">Noah</text></border></td>
                        </tr>
                        <tr>
                            <td><border padding="1mm"><text>Billing</text></border></td>
                            <td><border padding="1mm"><text>All invoices sent</text></border></td>
                            <td><border padding="1mm"><text horizontalAlignment="right">Mira</text></border></td>
                        </tr>
                    </table>

                    <text fontsize="11" weight="bold" margin="0 4mm 0 1mm">Next actions</text>
                    <text>Finalize the supplier review.</text>
                    <text>Confirm support staffing for June.</text>
                    <text>Prepare the next billing export.</text>
                </body>
                <footer>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 0 0 1mm"/>
                    <pageNumber
                        mode="CurrentTotal"
                        prefix="Operations report | Page "
                        delimiter=" of "
                        fontsize="8"
                        foreground="#64748b"
                        horizontalAlignment="right"/>
                </footer>
            </template>
            """,
            CompleteExampleDocumentOptions);

    [Fact]
    public Task CompleteExample_TableHeavyPreview()
        => RenderDocumentationSampleAsync(
            "complete-table-heavy-preview",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <template.style>
                    <text fontsize="8" foreground="#0f172a"/>
                </template.style>
                <header>
                    <text fontsize="18" weight="bold">Warehouse count sheet</text>
                    <text foreground="#475569">Prepared 2026-05-27 | Morning count</text>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 2mm 0 0"/>
                </header>
                <body>
                    <border background="#f8fafc" padding="2mm" margin="0 0 0 4mm" verticalAlignment="top">
                        <text fontsize="8" foreground="#475569">Use case</text>
                        <text>Use this layout for dense review tables with repeated labels, quantities and status notes.</text>
                    </border>

                    <table margin="0 0 0 4mm">
                        <tr>
                            <td width="1*">
                                <border background="#ecfeff" padding="2mm" margin="0 0 1mm 0">
                                    <text fontsize="8" foreground="#475569">Rows checked</text>
                                    <text fontsize="15" weight="bold">8</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border background="#f0fdf4" padding="2mm" margin="0 0 1mm 0">
                                    <text fontsize="8" foreground="#475569">Matched</text>
                                    <text fontsize="15" weight="bold">5</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border background="#fefce8" padding="2mm">
                                    <text fontsize="8" foreground="#475569">Needs review</text>
                                    <text fontsize="15" weight="bold">3</text>
                                </border>
                            </td>
                        </tr>
                    </table>

                    <text fontsize="11" weight="bold" margin="0 0 0 1mm">Count detail</text>
                    <table>
                        <th>
                            <td width="14mm">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold">Zone</text>
                                </border>
                            </td>
                            <td width="2*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold">Item</text>
                                </border>
                            </td>
                            <td width="17mm">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold" horizontalAlignment="right">Expected</text>
                                </border>
                            </td>
                            <td width="17mm">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold" horizontalAlignment="right">Counted</text>
                                </border>
                            </td>
                            <td width="14mm">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold" horizontalAlignment="right">Diff</text>
                                </border>
                            </td>
                            <td width="22mm">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold" horizontalAlignment="right">Status</text>
                                </border>
                            </td>
                        </th>
                        @alternate on RowBackground with ["#ffffff", "#f8fafc"] {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>A1</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text>Sensor gateway</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">48</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">48</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">0</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">Ready</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>A2</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text>Mounting rail</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">30</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">28</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">-2</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">Review</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>B1</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text>Setup card</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">80</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">80</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">0</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">Ready</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>B2</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text>Power adapter</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">64</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">64</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">0</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">Ready</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>C1</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text>Wireless sensor</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">96</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">92</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">-4</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">Review</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>C2</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text>Label roll</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">12</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">12</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">0</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">Ready</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>D1</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text>Battery pack</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">40</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">41</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">+1</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">Review</text></border></td>
                        </tr>
                        }
                        @alternate on RowBackground {
                        <tr>
                            <td><border background="@RowBackground" padding="1mm"><text>D2</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text>Shipping insert</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">75</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">75</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">0</text></border></td>
                            <td><border background="@RowBackground" padding="1mm"><text horizontalAlignment="right">Ready</text></border></td>
                        </tr>
                        }
                    </table>
                </body>
                <footer>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 0 0 1mm"/>
                    <pageNumber
                        mode="CurrentTotal"
                        prefix="Warehouse count | Page "
                        delimiter=" of "
                        fontsize="8"
                        foreground="#64748b"
                        horizontalAlignment="right"/>
                </footer>
            </template>
            """,
            CompleteExampleDocumentOptions);

    [Fact]
    public Task CompleteExample_ProductSheetPreview()
        => RenderDocumentationSampleAsync(
            "complete-product-sheet-preview",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <template.style>
                    <text fontsize="9" foreground="#0f172a"/>
                </template.style>
                <header>
                    <table>
                        <tr>
                            <td width="28mm">
                                <image
                                    source="@LogoImage"
                                    width="24mm"
                                    height="14mm"
                                    horizontalAlignment="left"
                                    verticalAlignment="top"/>
                            </td>
                            <td width="2*">
                                <text fontsize="18" weight="bold">Product sheet</text>
                                <text foreground="#475569">Compact sensor kit</text>
                            </td>
                        </tr>
                    </table>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 2mm 0 0"/>
                </header>
                <body>
                    <border background="#f8fafc" padding="2mm" margin="0 0 0 5mm" verticalAlignment="top">
                        <text fontsize="8" foreground="#475569">Overview</text>
                        <text>The kit combines a small gateway, two wireless sensors and a ready-to-print setup card.</text>
                    </border>

                    <text fontsize="11" weight="bold" margin="0 5mm 0 1mm">Package contents</text>
                    <table>
                        <th>
                            <td width="2*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold">Item</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold" horizontalAlignment="right">Qty</text>
                                </border>
                            </td>
                        </th>
                        <tr>
                            <td><border padding="1mm"><text>Gateway unit</text></border></td>
                            <td><border padding="1mm"><text horizontalAlignment="right">1</text></border></td>
                        </tr>
                        <tr>
                            <td><border padding="1mm" background="#f8fafc"><text>Wireless sensors</text></border></td>
                            <td><border padding="1mm" background="#f8fafc"><text horizontalAlignment="right">2</text></border></td>
                        </tr>
                        <tr>
                            <td><border padding="1mm"><text>Setup card</text></border></td>
                            <td><border padding="1mm"><text horizontalAlignment="right">1</text></border></td>
                        </tr>
                    </table>

                    <text fontsize="11" weight="bold" margin="0 5mm 0 1mm">Best fit</text>
                    <border background="#eff6ff" padding="2mm" verticalAlignment="top">
                        <text>Use this layout for datasheets, product inserts and branded one-page handouts.</text>
                    </border>
                </body>
                <footer>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 0 0 1mm"/>
                    <pageNumber
                        mode="CurrentTotal"
                        prefix="Product sheet | Page "
                        delimiter=" of "
                        fontsize="8"
                        foreground="#64748b"
                        horizontalAlignment="right"/>
                </footer>
            </template>
            """,
            CompleteExampleDocumentOptions,
            (generator) =>
            {
                generator.TemplateData.SetVariable("LogoImage", CreateLogoDataUri());
            });

    [Fact]
    public Task CompleteExample_ChartDashboardPreview()
        => RenderDocumentationSampleAsync(
            "complete-chart-dashboard-preview",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <template.style>
                    <text fontsize="9" foreground="#0f172a"/>
                </template.style>
                <header>
                    <text fontsize="18" weight="bold">Sales dashboard</text>
                    <text foreground="#475569">Weekly order trend and channel mix</text>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 2mm 0 0"/>
                </header>
                <body>
                    <table margin="0 0 0 4mm">
                        <tr>
                            <td width="1*">
                                <border background="#ecfeff" padding="2mm" margin="0 0 1mm 0">
                                    <text fontsize="8" foreground="#475569">Orders</text>
                                    <text fontsize="16" weight="bold">326</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border background="#f0fdf4" padding="2mm" margin="0 0 1mm 0">
                                    <text fontsize="8" foreground="#475569">Revenue</text>
                                    <text fontsize="16" weight="bold">48.2k</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border background="#fefce8" padding="2mm">
                                    <text fontsize="8" foreground="#475569">Open leads</text>
                                    <text fontsize="16" weight="bold">41</text>
                                </border>
                            </td>
                        </tr>
                    </table>

                    <border background="#f8fafc" padding="2mm" margin="0 0 0 4mm" verticalAlignment="top">
                        <text fontsize="8" foreground="#475569">Summary</text>
                        <text>Orders rose through the week, with the web channel carrying the largest share.</text>
                    </border>

                    <chart margin="0 0 0 4mm">
                        <lineChart height="45mm" title="Orders by day" line-color="#2563eb">
                            <data x="1" y="42"/>
                            <data x="2" y="46"/>
                            <data x="3" y="51"/>
                            <data x="4" y="58"/>
                            <data x="5" y="63"/>
                        </lineChart>
                    </chart>

                    <text fontsize="11" weight="bold" margin="0 1mm 0 1mm">Channel mix</text>
                    <table>
                        <th>
                            <td width="2*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold">Channel</text>
                                </border>
                            </td>
                            <td width="1*">
                                <border thickness="0 0 0 1pt" color="#334155" padding="1mm">
                                    <text weight="bold" horizontalAlignment="right">Share</text>
                                </border>
                            </td>
                        </th>
                        <tr>
                            <td><border padding="1mm"><text>Web</text></border></td>
                            <td><border padding="1mm"><text horizontalAlignment="right">52%</text></border></td>
                        </tr>
                        <tr>
                            <td><border padding="1mm" background="#f8fafc"><text>Partners</text></border></td>
                            <td><border padding="1mm" background="#f8fafc"><text horizontalAlignment="right">31%</text></border></td>
                        </tr>
                        <tr>
                            <td><border padding="1mm"><text>Direct</text></border></td>
                            <td><border padding="1mm"><text horizontalAlignment="right">17%</text></border></td>
                        </tr>
                    </table>
                </body>
                <footer>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 0 0 1mm"/>
                    <pageNumber
                        mode="CurrentTotal"
                        prefix="Sales dashboard | Page "
                        delimiter=" of "
                        fontsize="8"
                        foreground="#64748b"
                        horizontalAlignment="right"/>
                </footer>
            </template>
            """,
            CompleteExampleDocumentOptions);

    private static string CreateLogoDataUri()
    {
        using var bitmap = new SKBitmap(160, 90);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(new SKColor(219, 234, 254));

        using var bluePaint = new SKPaint { Color = new SKColor(37, 99, 235), IsAntialias = true };
        using var greenPaint = new SKPaint { Color = new SKColor(22, 163, 74), IsAntialias = true };
        using var darkPaint = new SKPaint { Color = new SKColor(15, 23, 42), IsAntialias = true };

        canvas.DrawRect(new SKRect(0, 0, 160, 22), darkPaint);
        canvas.DrawCircle(44, 54, 22, bluePaint);
        canvas.DrawRoundRect(new SKRect(78, 36, 138, 72), 8, 8, greenPaint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return $"data:image/png;base64,{Convert.ToBase64String(data.ToArray())}";
    }
}
