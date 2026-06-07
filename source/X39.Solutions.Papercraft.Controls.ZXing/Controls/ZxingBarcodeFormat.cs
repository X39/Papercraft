using System.ComponentModel;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[TypeConverter(typeof(ZxingBarcodeFormatConverter))]
public enum ZxingBarcodeFormat
{
    Aztec,
    Codabar,
    Code39,
    Code93,
    Code128,
    Gs1128,
    DataMatrix,
    Ean8,
    Ean13,
    Itf,
    Pdf417,
    QrCode,
    UpcA,
    UpcE,
}
