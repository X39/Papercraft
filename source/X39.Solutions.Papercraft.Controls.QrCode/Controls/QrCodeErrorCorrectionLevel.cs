using System.ComponentModel;

namespace X39.Solutions.Papercraft.Controls.QrCode.Controls;

[TypeConverter(typeof(QrCodeErrorCorrectionLevelConverter))]
public enum QrCodeErrorCorrectionLevel
{
    Low,
    Medium,
    Quartile,
    High,
}
