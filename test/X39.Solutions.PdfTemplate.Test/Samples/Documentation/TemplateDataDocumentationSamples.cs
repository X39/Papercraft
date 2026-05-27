namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class TemplateDataDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task TemplateData_InsertVariable()
        => RenderDocumentationSampleAsync(
            "template-data-insert-variable",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="14">Order @OrderNumber</text>
                    <text>Hello @CustomerName</text>
                    <text>Delivery: @DeliveryDate</text>
                </body>
            </template>
            """,
            configureGenerator: (generator) =>
            {
                generator.TemplateData.SetVariable("OrderNumber", "A-1007");
                generator.TemplateData.SetVariable("CustomerName", "Mira Silva");
                generator.TemplateData.SetVariable("DeliveryDate", "Friday");
            });

    [Fact]
    public Task TemplateData_UseVariableInAttribute()
        => RenderDocumentationSampleAsync(
            "template-data-variable-attribute",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <border
                        thickness="1pt"
                        color="@AccentBorder"
                        background="@AccentBackground"
                        padding="2mm"
                        verticalAlignment="top">
                        <text fontsize="10">The box colors come from template data.</text>
                    </border>
                </body>
            </template>
            """,
            configureGenerator: (generator) =>
            {
                generator.TemplateData.SetVariable("AccentBorder", "#2f5597");
                generator.TemplateData.SetVariable("AccentBackground", "#eaf2ff");
            });
}
