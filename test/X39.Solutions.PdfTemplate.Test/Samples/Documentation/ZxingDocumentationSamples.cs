using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Controls.ZXing.Controls;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class ZxingDocumentationSamples : DocumentationSampleBase
{
    private static DocumentOptions BarcodeDocumentOptions { get; } = new()
    {
        PageWidthInMillimeters = 100,
        PageHeightInMillimeters = 75,
        Margin = new Thickness(new Length(5, ELengthUnit.Millimeters)),
    };

    [Fact]
    public Task Zxing_GenericBarcode()
        => RenderDocumentationSampleAsync(
            "zxing-generic-barcode",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <barcode format="Code128" value="ABC123" width="42mm" height="12mm"/>

                    <spacer height="4mm"/>

                    <barcode format="DataMatrix" width="22mm" height="22mm">ABC123</barcode>
                </body>
            </template>
            """,
            BarcodeDocumentOptions,
            configureServices: RegisterZxingControls);

    [Fact]
    public Task Zxing_AliasBarcodes()
        => RenderDocumentationSampleAsync(
            "zxing-alias-barcodes",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <template>
                <body>
                    <code128 value="ABC123" width="42mm" height="12mm"/>

                    <spacer height="3mm"/>

                    <ean13 value="4006381333931" width="42mm" height="14mm"/>

                    <spacer height="3mm"/>

                    <dataMatrix value="ABC123" width="22mm" height="22mm"/>
                </body>
            </template>
            """,
            BarcodeDocumentOptions,
            configureServices: RegisterZxingControls);

    private static void RegisterZxingControls(PdfTemplateServiceBuilder builder)
        => builder.AddControl<BarcodeControl>()
                  .AddControl<Code128BarcodeControl>()
                  .AddControl<Gs1128BarcodeControl>()
                  .AddControl<Code39BarcodeControl>()
                  .AddControl<Code93BarcodeControl>()
                  .AddControl<CodabarBarcodeControl>()
                  .AddControl<Ean13BarcodeControl>()
                  .AddControl<Ean8BarcodeControl>()
                  .AddControl<UpcABarcodeControl>()
                  .AddControl<UpcEBarcodeControl>()
                  .AddControl<ItfBarcodeControl>()
                  .AddControl<DataMatrixBarcodeControl>()
                  .AddControl<Pdf417BarcodeControl>()
                  .AddControl<AztecBarcodeControl>();
}
