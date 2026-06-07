using X39.Solutions.Papercraft.Attributes;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "upcA")]
public sealed class UpcABarcodeControl : BarcodeControl
{
    public UpcABarcodeControl() : base(global::ZXing.BarcodeFormat.UPC_A)
    {
    }
}