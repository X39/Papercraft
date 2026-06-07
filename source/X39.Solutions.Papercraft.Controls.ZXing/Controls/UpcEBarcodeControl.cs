using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "upcE")]
public sealed class UpcEBarcodeControl : BarcodeControl
{
    public UpcEBarcodeControl() : base(global::ZXing.BarcodeFormat.UPC_E)
    {
    }
}