using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Controls.QrCode.Controls;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class QrCodeDocumentationSamples : DocumentationSampleBase
{
    private static DocumentOptions QrCodeDocumentOptions { get; } = new()
    {
        PageWidthInMillimeters = 100,
        PageHeightInMillimeters = 75,
        Margin = new Thickness(new Length(5, ELengthUnit.Millimeters)),
    };

    [Fact]
    public Task QrCode_Basic()
        => RenderDocumentationSampleAsync(
            "qrcode-basic",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <qrCode value="https://example.test/order/123" size="24mm" quietZone="4" errorCorrection="M"/>

                    <spacer height="4mm"/>

                    <qrCode size="24mm" foreground="#000000" background="#ffffff">
                        https://example.test/order/123
                    </qrCode>
                </body>
            </template>
            """,
            QrCodeDocumentOptions,
            configureServices: (builder) => builder.AddControl<QrCodeControl>());
}
