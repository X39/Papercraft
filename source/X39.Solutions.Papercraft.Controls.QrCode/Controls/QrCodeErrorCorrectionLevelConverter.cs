using System.ComponentModel;

namespace X39.Solutions.Papercraft.Controls.QrCode.Controls;

public sealed class QrCodeErrorCorrectionLevelConverter : TypeConverter
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

        return text.Trim()
                   .ToUpperInvariant() switch
        {
            "L" or "LOW"      => QrCodeErrorCorrectionLevel.Low,
            "M" or "MEDIUM"   => QrCodeErrorCorrectionLevel.Medium,
            "Q" or "QUARTILE" => QrCodeErrorCorrectionLevel.Quartile,
            "H" or "HIGH"     => QrCodeErrorCorrectionLevel.High,
            _ => throw new FormatException($"The value '{text}' is not a valid QR code error correction level."),
        };
    }
}
