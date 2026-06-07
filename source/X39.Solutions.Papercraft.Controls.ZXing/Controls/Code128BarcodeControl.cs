using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "code128")]
public sealed class Code128BarcodeControl : BarcodeControl
{
    public Code128BarcodeControl() : base(global::ZXing.BarcodeFormat.CODE_128)
    {
    }
}