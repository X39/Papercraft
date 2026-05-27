namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class LayoutDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Layout_SpacingAndPadding()
        => RenderDocumentationSampleAsync(
            "layout-spacing-and-padding",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <border
                        thickness="1pt"
                        color="#94a3b8"
                        background="#f8fafc"
                        padding="2mm"
                        verticalAlignment="top">
                        <text fontsize="9">Outer box</text>
                        <border
                            thickness="1pt"
                            color="#2563eb"
                            background="#dbeafe"
                            margin="3mm 0"
                            padding="2mm"
                            verticalAlignment="top">
                            <text fontsize="9">Margin separates boxes. Padding keeps text away from the border.</text>
                        </border>
                        <text fontsize="9">The next line starts after the inner box.</text>
                    </border>
                </body>
            </template>
            """);

    [Fact]
    public Task Layout_Alignment()
        => RenderDocumentationSampleAsync(
            "layout-alignment",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="9">Left</text>
                    <line length="35%" thickness="2pt" color="#2563eb" horizontalAlignment="left" margin="0 1mm"/>
                    <text fontsize="9" horizontalAlignment="center">Center</text>
                    <line length="35%" thickness="2pt" color="#16a34a" horizontalAlignment="center" margin="0 1mm"/>
                    <text fontsize="9" horizontalAlignment="right">Right</text>
                    <line length="35%" thickness="2pt" color="#dc2626" horizontalAlignment="right" margin="0 1mm"/>
                </body>
            </template>
            """);

    [Fact]
    public Task Layout_LengthsAndColors()
        => RenderDocumentationSampleAsync(
            "layout-lengths-and-colors",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="9">30mm fixed length</text>
                    <line length="30mm" thickness="2pt" color="#2f5597" margin="0 1mm"/>
                    <text fontsize="9">50% of available width</text>
                    <line length="50%" thickness="2pt" color="orange" margin="0 1mm"/>
                    <text fontsize="9">Transparent background color</text>
                    <border
                        thickness="1pt"
                        color="#166534"
                        background="#dcfce7aa"
                        padding="2mm"
                        margin="1mm 0 0 0"
                        verticalAlignment="top">
                        <text fontsize="9">Colors can use names or hex values.</text>
                    </border>
                </body>
            </template>
            """);

    [Fact]
    public Task Layout_ClipOverflow()
        => RenderDocumentationSampleAsync(
            "layout-clip-overflow",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="8" weight="bold">Default clip</text>
                    <table margin="0 0 2mm 0">
                        <tr>
                            <td width="22mm" padding="1mm">
                                <line length="45mm" thickness="3pt" color="#2563eb"/>
                            </td>
                            <td width="1*" padding="1mm">
                                <text fontsize="8" foreground="#475569">Next cell</text>
                            </td>
                        </tr>
                    </table>
                    <text fontsize="8" weight="bold">clip="false"</text>
                    <table>
                        <tr>
                            <td width="22mm" padding="1mm" clip="false">
                                <line length="45mm" thickness="3pt" color="#dc2626"/>
                            </td>
                            <td width="1*" padding="1mm">
                                <text fontsize="8" foreground="#475569">Next cell</text>
                            </td>
                        </tr>
                    </table>
                </body>
            </template>
            """);
}
