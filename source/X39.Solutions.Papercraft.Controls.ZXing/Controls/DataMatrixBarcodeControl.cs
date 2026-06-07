using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "dataMatrix")]
public sealed class DataMatrixBarcodeControl : BarcodeControl
{
    public DataMatrixBarcodeControl() : base(global::ZXing.BarcodeFormat.DATA_MATRIX)
    {
        Width  = new Length(25, ELengthUnit.Millimeters);
        Height = new Length(25, ELengthUnit.Millimeters);
    }
}