using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "ean13")]
public sealed class Ean13BarcodeControl : BarcodeControl
{
    public Ean13BarcodeControl() : base(global::ZXing.BarcodeFormat.EAN_13)
    {
    }
}