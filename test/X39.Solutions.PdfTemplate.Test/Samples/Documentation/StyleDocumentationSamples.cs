namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class StyleDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Styles_SharedTextAndBorder()
        => RenderDocumentationSampleAsync(
            "styles-shared-text-and-border",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <template.style>
                    <text fontsize="9" foreground="#0f172a"/>
                    <border
                        thickness="1pt"
                        color="#0f766e"
                        background="#ccfbf1"
                        padding="2mm"
                        margin="0 0 2mm 0"
                        verticalAlignment="top"/>
                </template.style>
                <body>
                    <border>
                        <text>Shared text and border attributes.</text>
                    </border>
                    <border color="#2563eb" background="#dbeafe">
                        <text>This box overrides only its colors.</text>
                    </border>
                </body>
            </template>
            """);
}
