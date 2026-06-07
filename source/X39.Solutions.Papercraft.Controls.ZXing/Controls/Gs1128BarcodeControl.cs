using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "gs1-128")]
public sealed class Gs1128BarcodeControl : BarcodeControl
{
    public Gs1128BarcodeControl() : base(global::ZXing.BarcodeFormat.CODE_128, gs1Format: true)
    {
        Format = ZxingBarcodeFormat.Gs1128;
    }
}