namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class TextDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Text_FontSizeAndColor()
        => RenderDocumentationSampleAsync(
            "text-font-size-and-color",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="9" foreground="#475569">Small supporting text</text>
                    <text fontsize="18" foreground="#1d4ed8">Larger blue heading</text>
                </body>
            </template>
            """);

    [Fact]
    public Task Text_Alignment()
        => RenderDocumentationSampleAsync(
            "text-alignment",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="10" horizontalAlignment="left">Left aligned</text>
                    <text fontsize="10" horizontalAlignment="center">Centered</text>
                    <text fontsize="10" horizontalAlignment="right">Right aligned</text>
                </body>
            </template>
            """);

    [Fact]
    public Task Text_Padding()
        => RenderDocumentationSampleAsync(
            "text-padding",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <border background="#f8fafc" color="#94a3b8" thickness="1pt" verticalAlignment="top">
                        <text fontsize="10" padding="3mm">Padded text inside a visible box</text>
                    </border>
                    <text fontsize="10" padding="3mm">Plain text can also reserve space.</text>
                </body>
            </template>
            """);

    [Fact]
    public Task Text_StyleAndWeight()
        => RenderDocumentationSampleAsync(
            "text-style-and-weight",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="12" weight="bold">Bold text</text>
                    <text fontsize="12" style="italic">Italic text</text>
                    <text fontsize="12" weight="semiBold" foreground="#166534">Semi-bold green text</text>
                </body>
            </template>
            """);
}
