using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "aztec")]
public sealed class AztecBarcodeControl : BarcodeControl
{
    public AztecBarcodeControl() : base(global::ZXing.BarcodeFormat.AZTEC)
    {
        Width  = new Length(25, ELengthUnit.Millimeters);
        Height = new Length(25, ELengthUnit.Millimeters);
    }
}