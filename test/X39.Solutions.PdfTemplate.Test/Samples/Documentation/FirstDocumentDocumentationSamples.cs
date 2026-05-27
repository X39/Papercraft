using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class FirstDocumentDocumentationSamples : DocumentationSampleBase
{
    private static DocumentOptions StarterDocumentOptions { get; } = new()
    {
        PageWidthInMillimeters = 100,
        PageHeightInMillimeters = 60,
        Margin = new Thickness(new Length(6, ELengthUnit.Millimeters)),
    };

    private static DocumentOptions WideMarginDocumentOptions { get; } = StarterDocumentOptions with
    {
        Margin = new Thickness(new Length(12, ELengthUnit.Millimeters)),
    };

    [Fact]
    public Task FirstDocument_Minimal()
        => RenderDocumentationSampleAsync(
            "first-document-minimal",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="18">Packing list</text>
                    <text>Use the body for the main document content.</text>
                </body>
            </template>
            """,
            StarterDocumentOptions);

    [Fact]
    public Task FirstDocument_HeaderFooter()
        => RenderDocumentationSampleAsync(
            "first-document-header-footer",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <header>
                    <text fontsize="14">Packing list</text>
                    <line thickness="1pt" length="100%" color="#6b7280"/>
                </header>
                <body>
                    <text>Body content starts below the header.</text>
                    <text>Use this space for the information that changes from document to document.</text>
                </body>
                <footer>
                    <line thickness="1pt" length="100%" color="#6b7280"/>
                    <text fontsize="9">Footer text is repeated at the bottom of each page.</text>
                </footer>
            </template>
            """,
            StarterDocumentOptions);

    [Fact]
    public Task FirstDocument_PageMargin()
        => RenderDocumentationSampleAsync(
            "first-document-page-margin",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <border thickness="1pt" color="#2563eb" background="#eff6ff" padding="3mm">
                        <text>The page margin leaves space around this content area.</text>
                    </border>
                </body>
            </template>
            """,
            WideMarginDocumentOptions);

    [Fact]
    public Task FirstDocument_BackgroundForeground()
        => RenderDocumentationSampleAsync(
            "first-document-background-foreground",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <background>
                    <border background="#f3f4f6"/>
                </background>
                <body>
                    <text fontsize="16">Draft invoice</text>
                    <text>The body is rendered between the background and foreground layers.</text>
                </body>
                <foreground>
                    <text
                        fontsize="24"
                        foreground="#b91c1c66"
                        horizontalAlignment="center"
                        verticalAlignment="center">DRAFT</text>
                </foreground>
            </template>
            """,
            StarterDocumentOptions);

    [Fact]
    public Task FirstDocument_FixedArea()
        => RenderDocumentationSampleAsync(
            "first-document-fixed-area",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="16">Shipping note</text>
                    <text>The body uses the normal page flow.</text>
                </body>
                <areas>
                    <area width="32mm" height="10mm" right="4mm" top="4mm">
                        <border background="#dcfce7" color="#166534" thickness="1pt" padding="2mm">
                            <text fontsize="9">APPROVED</text>
                        </border>
                    </area>
                </areas>
            </template>
            """,
            StarterDocumentOptions);
}
