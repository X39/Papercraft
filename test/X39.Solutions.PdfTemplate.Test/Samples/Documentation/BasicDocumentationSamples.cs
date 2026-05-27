namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class BasicDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Text_Basic()
        => RenderDocumentationSampleAsync(
            "text-basic",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="18">Hello from a template</text>
                </body>
            </template>
            """);

    [Fact]
    public Task Border_WithBackground()
        => RenderDocumentationSampleAsync(
            "border-with-background",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <border
                        thickness="1pt"
                        color="#2f5597"
                        background="#eaf2ff"
                        padding="4mm"
                        horizontalAlignment="left"
                        verticalAlignment="top">
                        <text>Content can sit inside a border.</text>
                    </border>
                </body>
            </template>
            """);

    [Fact]
    public Task Line_HorizontalSeparator()
        => RenderDocumentationSampleAsync(
            "line-horizontal-separator",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text>Header text</text>
                    <line thickness="1pt" length="100%" color="#666666" margin="0 1mm"/>
                    <text>Body text starts below the line.</text>
                </body>
            </template>
            """);
}
