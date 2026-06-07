using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "code39")]
public sealed class Code39BarcodeControl : BarcodeControl
{
    public Code39BarcodeControl() : base(global::ZXing.BarcodeFormat.CODE_39)
    {
    }
}