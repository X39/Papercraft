namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class PageNumberDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task PageNumber_Footer()
        => RenderDocumentationSampleAsync(
            "page-number-footer",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="14" weight="bold">Monthly report</text>
                    <text fontsize="10" foreground="#475569">The footer shows the current page and total page count.</text>
                </body>
                <footer>
                    <line thickness="1pt" length="100%" color="#cbd5e1" margin="0 0 1mm 0"/>
                    <pageNumber
                        mode="CurrentTotal"
                        prefix="Page "
                        delimiter=" of "
                        fontsize="9"
                        foreground="#475569"
                        horizontalAlignment="right"/>
                </footer>
            </template>
            """);
}
