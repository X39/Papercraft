using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "pdf417")]
public sealed class Pdf417BarcodeControl : BarcodeControl
{
    public Pdf417BarcodeControl() : base(global::ZXing.BarcodeFormat.PDF_417)
    {
        Width  = new Length(50, ELengthUnit.Millimeters);
        Height = new Length(20, ELengthUnit.Millimeters);
    }
}