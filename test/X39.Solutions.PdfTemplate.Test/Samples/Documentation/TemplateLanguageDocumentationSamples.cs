namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class TemplateLanguageDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task TemplateLanguage_ConditionalSection()
        => RenderDocumentationSampleAsync(
            "template-language-conditional-section",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="14" weight="bold">Order status</text>
                    @if HasBalanceDue {
                    <border
                        background="#fef3c7"
                        color="#d97706"
                        thickness="1pt"
                        padding="2mm"
                        margin="0 0 0 2mm"
                        verticalAlignment="top">
                        <text fontsize="10">Payment is still due.</text>
                    </border>
                    }
                    @else {
                    <border
                        background="#dcfce7"
                        color="#16a34a"
                        thickness="1pt"
                        padding="2mm"
                        margin="0 0 0 2mm"
                        verticalAlignment="top">
                        <text fontsize="10">Paid in full.</text>
                    </border>
                    }
                </body>
            </template>
            """,
            configureGenerator: (generator) =>
            {
                generator.TemplateData.SetVariable("HasBalanceDue", true);
            });
}
