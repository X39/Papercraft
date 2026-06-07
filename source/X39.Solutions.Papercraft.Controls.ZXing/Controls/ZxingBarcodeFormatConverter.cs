using System.ComponentModel;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

public sealed class ZxingBarcodeFormatConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is not string text)
            return base.ConvertFrom(context, culture, value);

        var normalized = Normalize(text);
        return normalized switch
        {
            "AZTEC"                       => ZxingBarcodeFormat.Aztec,
            "CODABAR"                     => ZxingBarcodeFormat.Codabar,
            "CODE39"                      => ZxingBarcodeFormat.Code39,
            "CODE93"                      => ZxingBarcodeFormat.Code93,
            "CODE128"                     => ZxingBarcodeFormat.Code128,
            "128"                         => ZxingBarcodeFormat.Code128,
            "GS1128" or "GS1CODE128"      => ZxingBarcodeFormat.Gs1128,
            "DATAMATRIX"                  => ZxingBarcodeFormat.DataMatrix,
            "EAN8"                        => ZxingBarcodeFormat.Ean8,
            "EAN13"                       => ZxingBarcodeFormat.Ean13,
            "ITF" or "INTERLEAVED2OF5"    => ZxingBarcodeFormat.Itf,
            "PDF417"                      => ZxingBarcodeFormat.Pdf417,
            "QRCODE" or "QR"              => ZxingBarcodeFormat.QrCode,
            "UPCA"                        => ZxingBarcodeFormat.UpcA,
            "UPCE"                        => ZxingBarcodeFormat.UpcE,
            _ => throw new FormatException($"The value '{text}' is not a supported ZXing barcode format."),
        };
    }

    private static string Normalize(string value)
        => new(value.Trim()
                    .Where((character) => character is not '-' and not '_' and not ' ')
                    .Select(char.ToUpperInvariant)
                    .ToArray());
}
