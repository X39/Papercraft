namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class RichTextDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task RichText_Paragraph()
        => RenderDocumentationSampleAsync(
            "rich-text-paragraph",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <paragraph foreground="red" fontsize="14" lineheight="1.5" decoration="underline">
                        <span>Total: </span>
                        <span foreground="blue" weight="bold" decoration="doubleUnderline">Value</span>
                        <br/>
                    </paragraph>
                </body>
            </template>
            """);

    [Fact]
    public Task RichText_Hyperlink()
        => RenderDocumentationSampleAsync(
            "rich-text-hyperlink",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <hyperlink
                        href="https://example.test/invoice/123"
                        underline="false"
                        foreground="red"
                        fontsize="14">View invoice</hyperlink>
                </body>
            </template>
            """);
}
