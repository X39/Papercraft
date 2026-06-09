namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class FlowDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Flow_BlockSpacerAndColumns()
        => RenderDocumentationSampleAsync(
            "flow-block-spacer-columns",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <block background="#f8fafc" minHeight="12px" padding="2px">
                        <text fontsize="9">Grouped content</text>
                    </block>

                    <spacer height="4mm"/>

                    <columns count="3" gap="7px" ruleThickness="2px" ruleColor="#dc2626">
                        <text fontsize="8">First item</text>
                        <text fontsize="8">Second item</text>
                        <text fontsize="8">Third item</text>
                    </columns>
                </body>
            </template>
            """);
}
