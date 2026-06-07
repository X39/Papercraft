using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "code93")]
public sealed class Code93BarcodeControl : BarcodeControl
{
    public Code93BarcodeControl() : base(global::ZXing.BarcodeFormat.CODE_93)
    {
    }
}