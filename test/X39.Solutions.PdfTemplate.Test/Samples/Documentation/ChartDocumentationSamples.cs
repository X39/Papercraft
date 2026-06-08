using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class ChartDocumentationSamples : DocumentationSampleBase
{
    private static DocumentOptions ChartDocumentOptions { get; } = new()
    {
        PageWidthInMillimeters = 120,
        PageHeightInMillimeters = 80,
        Margin = new Thickness(new Length(5, ELengthUnit.Millimeters)),
    };

    [Fact]
    public Task Chart_LineTrend()
        => RenderDocumentationSampleAsync(
            "chart-line-trend",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <chart>
                        <lineChart height="55mm" title="Orders" line-color="#2563eb">
                            <data x="0" y="18"/>
                            <data x="1" y="24"/>
                            <data x="2" y="21"/>
                            <data x="3" y="31"/>
                        </lineChart>
                    </chart>
                </body>
            </template>
            """,
            ChartDocumentOptions);

    [Fact]
    public Task Chart_BarValues()
        => RenderDocumentationSampleAsync(
            "chart-bar-values",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <chart>
                        <barChart height="55mm" title="Open items" show-data-labels="true" x-axis-label="Queue">
                            <data x="0" y="12" label="New" color="#2563eb"/>
                            <data x="1" y="19" label="Review" color="#16a34a"/>
                            <data x="2" y="7" label="Blocked" color="#f59e0b"/>
                        </barChart>
                    </chart>
                </body>
            </template>
            """,
            ChartDocumentOptions);

    [Fact]
    public Task Chart_PieShare()
        => RenderDocumentationSampleAsync(
            "chart-pie-share",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <chart>
                        <pieChart height="60mm" title="Request mix" pie-label-position="Outside">
                            <data y="45" label="Email" color="#2563eb"/>
                            <data y="35" label="Portal" color="#16a34a"/>
                            <data y="20" label="Phone" color="#f59e0b"/>
                        </pieChart>
                    </chart>
                </body>
            </template>
            """,
            ChartDocumentOptions);
}
