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
                        margin="0 2mm 0 0"
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

    [Fact]
    public Task TemplateLanguage_SwitchStatus()
        => RenderDocumentationSampleAsync(
            "template-language-switch-status",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="14" weight="bold">Delivery status</text>
                    @switch Status {
                    @case "paid" {
                    <border background="#dcfce7" color="#16a34a" thickness="1pt" padding="2mm" verticalAlignment="top">
                        <text fontsize="10">Paid and ready to ship.</text>
                    </border>
                    }
                    @case "pending" {
                    <border background="#fef3c7" color="#d97706" thickness="1pt" padding="2mm" verticalAlignment="top">
                        <text fontsize="10">Payment is pending.</text>
                    </border>
                    }
                    @default {
                    <border background="#f1f5f9" color="#64748b" thickness="1pt" padding="2mm" verticalAlignment="top">
                        <text fontsize="10">Status needs review.</text>
                    </border>
                    }
                    }
                </body>
            </template>
            """,
            configureGenerator: (generator) =>
            {
                generator.TemplateData.SetVariable("Status", "pending");
            });

    [Fact]
    public Task TemplateLanguage_ForeachChecklist()
        => RenderDocumentationSampleAsync(
            "template-language-foreach-checklist",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="14" weight="bold">Review checklist</text>
                    @foreach TaskName in Tasks with Index {
                    <border
                        background="#f8fafc"
                        color="#cbd5e1"
                        thickness="1pt"
                        padding="1mm"
                        margin="0 1mm 0 0"
                        verticalAlignment="top">
                        <text fontsize="9">Item @Index: @TaskName</text>
                    </border>
                    }
                </body>
            </template>
            """,
            configureGenerator: (generator) =>
            {
                generator.TemplateData.SetVariable("Tasks", new[] {"Draft", "Review", "Approve"});
            });

    [Fact]
    public Task TemplateLanguage_ForNumberedSteps()
        => RenderDocumentationSampleAsync(
            "template-language-for-numbered-steps",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="14" weight="bold">Build steps</text>
                    @for Step from 1 to 4 {
                    <border
                        background="#ecfeff"
                        color="#0891b2"
                        thickness="1pt"
                        padding="1mm"
                        margin="0 1mm 0 0"
                        verticalAlignment="top">
                        <text fontsize="9">Step @Step</text>
                    </border>
                    }
                </body>
            </template>
            """);

    [Fact]
    public Task TemplateLanguage_TemporaryValue()
        => RenderDocumentationSampleAsync(
            "template-language-temporary-value",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="14" weight="bold">Customer summary</text>
                    @var Label = "Bill to", Name = CustomerName {
                    <border
                        background="#f8fafc"
                        color="#94a3b8"
                        thickness="1pt"
                        padding="2mm"
                        margin="0 0 0 2mm"
                        verticalAlignment="top">
                        <text fontsize="9" weight="bold">@Label</text>
                        <text fontsize="10">@Name</text>
                    </border>
                    }
                </body>
            </template>
            """,
            configureGenerator: (generator) =>
            {
                generator.TemplateData.SetVariable("CustomerName", "Mira Lane");
            });

    [Fact]
    public Task TemplateLanguage_AlternatingValues()
        => RenderDocumentationSampleAsync(
            "template-language-alternating-values",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="14" weight="bold">Alternating rows</text>
                    @alternate on RowBackground with ["#ffffff", "#f1f5f9"] {
                    <border
                        background="@RowBackground"
                        color="#cbd5e1"
                        thickness="1pt"
                        padding="1mm"
                        margin="0 1mm 0 0"
                        verticalAlignment="top">
                        <text fontsize="9">Draft</text>
                    </border>
                    }
                    @alternate on RowBackground {
                    <border
                        background="@RowBackground"
                        color="#cbd5e1"
                        thickness="1pt"
                        padding="1mm"
                        margin="0 1mm 0 0"
                        verticalAlignment="top">
                        <text fontsize="9">Review</text>
                    </border>
                    }
                    @alternate on RowBackground {
                    <border
                        background="@RowBackground"
                        color="#cbd5e1"
                        thickness="1pt"
                        padding="1mm"
                        margin="0 1mm 0 0"
                        verticalAlignment="top">
                        <text fontsize="9">Approve</text>
                    </border>
                    }
                </body>
            </template>
            """);
}
