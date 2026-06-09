namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class FormDocumentationSamples : DocumentationSampleBase
{
    [Fact]
    public Task Forms_CheckboxAndSignature()
        => RenderDocumentationSampleAsync(
            "forms-checkbox-and-signature",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <checkbox
                        checked="true"
                        size="5mm"
                        label="Approved"
                        gap="2mm"
                        strokeColor="#dc2626"
                        fill="#dbeafe"
                        checkColor="#16a34a"
                        strokeThickness="1pt"/>

                    <spacer height="6mm"/>

                    <signature
                        height="16mm"
                        lineWidth="45mm"
                        lineThickness="1pt"
                        lineColor="#dc2626"
                        label="Signed"
                        subtext="Manager"
                        textPlacement="Above"/>
                </body>
            </template>
            """);

    [Fact]
    public Task Forms_CheckboxElementLabel()
        => RenderDocumentationSampleAsync(
            "forms-checkbox-element-label",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <checkbox>Element Label</checkbox>
                </body>
            </template>
            """);
}
