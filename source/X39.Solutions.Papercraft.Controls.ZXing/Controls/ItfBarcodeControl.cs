using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "itf")]
public sealed class ItfBarcodeControl : BarcodeControl
{
    public ItfBarcodeControl() : base(global::ZXing.BarcodeFormat.ITF)
    {
    }
}