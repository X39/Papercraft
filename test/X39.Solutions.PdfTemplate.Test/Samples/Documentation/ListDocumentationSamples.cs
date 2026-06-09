namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class ListDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Lists_BasicMarkers()
        => RenderDocumentationSampleAsync(
            "lists-basic-markers",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <ul marker="circle" indent="8mm" markerWidth="4mm" itemSpacing="1mm">
                        <li><text fontsize="9">First</text></li>
                        <li><text fontsize="9">Second</text></li>
                    </ul>

                    <spacer height="3mm"/>

                    <ol start="5" markerFormat="({0})" indent="10mm" markerWidth="7mm" itemSpacing="1mm">
                        <li><text fontsize="9">First</text></li>
                        <li><text fontsize="9">Second</text></li>
                    </ol>
                </body>
            </template>
            """);

    [Fact]
    public Task Lists_NestedItems()
        => RenderDocumentationSampleAsync(
            "lists-nested-items",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <ul>
                        <li>
                            <text fontsize="9">Parent</text>
                            <ol start="3">
                                <li><text fontsize="9">Nested</text></li>
                            </ol>
                        </li>
                    </ul>
                </body>
            </template>
            """);
}
