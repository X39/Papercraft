using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;

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

    [Fact]
    public Task TemplateData_CallFunction()
        => RenderDocumentationSampleAsync(
            "template-data-call-function",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <text fontsize="14">Invoice status</text>
                    <text>Status: @statusLabel(PaymentStatus)</text>
                </body>
            </template>
            """,
            configureGenerator: (generator) =>
            {
                generator.TemplateData.SetVariable("PaymentStatus", "paid");
                generator.TemplateData.RegisterFunction(new StatusLabelFunction());
            });

    private sealed class StatusLabelFunction : IFunction
    {
        public string Name => "statusLabel";
        public int Arguments => 1;
        public bool IsVariadic => false;

        public ValueTask<object?> ExecuteAsync(
            CultureInfo cultureInfo,
            object?[] arguments,
            CancellationToken cancellationToken = default)
        {
            var status = Convert.ToString(arguments[0], cultureInfo);
            var label = status?.ToLowerInvariant() switch
            {
                "paid" => "Paid in full",
                "open" => "Open",
                _ => "Needs review",
            };
            return ValueTask.FromResult<object?>(label);
        }
    }
}
