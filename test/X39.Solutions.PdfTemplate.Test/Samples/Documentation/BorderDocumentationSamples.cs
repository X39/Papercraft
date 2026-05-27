namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class BorderDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Border_BottomRule()
        => RenderDocumentationSampleAsync(
            "border-bottom-rule",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <border
                        thickness="0 0 0 1pt"
                        color="#64748b"
                        padding="0 0 1.5mm 0"
                        margin="0 0 3mm 0"
                        verticalAlignment="top">
                        <text fontsize="14" weight="bold">Section title</text>
                    </border>
                    <text fontsize="10" foreground="#475569">The next content starts below the rule.</text>
                </body>
            </template>
            """);
}
