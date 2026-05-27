namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class LineDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Line_VerticalDivider()
        => RenderDocumentationSampleAsync(
            "line-vertical-divider",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <table>
                        <tr>
                            <td width="2*">
                                <text fontsize="9">Left note</text>
                            </td>
                            <td width="4mm">
                                <line
                                    orientation="vertical"
                                    length="14mm"
                                    thickness="1pt"
                                    color="#2563eb"
                                    horizontalAlignment="center"/>
                            </td>
                            <td width="2*">
                                <text fontsize="9">Right note</text>
                            </td>
                        </tr>
                    </table>
                </body>
            </template>
            """);
}
