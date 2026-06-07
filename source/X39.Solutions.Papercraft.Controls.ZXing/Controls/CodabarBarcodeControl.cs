using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "codabar")]
public sealed class CodabarBarcodeControl : BarcodeControl
{
    public CodabarBarcodeControl() : base(global::ZXing.BarcodeFormat.CODABAR)
    {
    }
}