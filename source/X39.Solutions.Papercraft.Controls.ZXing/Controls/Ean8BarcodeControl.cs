using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "ean8")]
public sealed class Ean8BarcodeControl : BarcodeControl
{
    public Ean8BarcodeControl() : base(global::ZXing.BarcodeFormat.EAN_8)
    {
    }
}