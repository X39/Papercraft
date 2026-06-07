using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls.QrCode.Controls;

[Control(Constants.ControlsNamespace, "qrCode")]
public sealed class QrCodeControl : AlignableControl
{
    [Parameter(IsContent = true)]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public Length Size { get; set; } = new(25, ELengthUnit.Millimeters);

    [Parameter]
    public Color Foreground { get; set; } = Colors.Black;

    [Parameter]
    public Color Background { get; set; } = Colors.Transparent;

    [Parameter]
    public int QuietZone { get; set; } = 4;

    [Parameter]
    public QrCodeErrorCorrectionLevel ErrorCorrection { get; set; } = QrCodeErrorCorrectionLevel.Medium;

    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var size = ResolveSize(dpi, remainingSize);
        return new Size(size, size);
    }

    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var size = ResolveSize(dpi, remainingSize);
        return new Size(size, size);
    }

    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(Value))
            throw new InvalidOperationException("QR code value must not be empty.");
        if (QuietZone < 0)
            throw new InvalidOperationException("QR code quiet zone must not be negative.");

        var qrCode = global::Net.Codecrete.QrCodeGenerator.QrCode.EncodeText(
            Value,
            ToCodecreteErrorCorrection(ErrorCorrection));
        var availableWidth = ArrangementInner.Width;
        var availableHeight = ArrangementInner.Height;
        var availableSize = Math.Min(availableWidth, availableHeight);
        if (availableSize <= 0)
            return Data.Size.Zero;

        var moduleCount = qrCode.Size + QuietZone * 2;
        var moduleSize = availableSize / moduleCount;
        var renderedSize = moduleSize * moduleCount;
        var offsetX = (availableWidth - renderedSize) / 2;
        var offsetY = (availableHeight - renderedSize) / 2;

        canvas.DrawRect(new Rectangle(offsetX, offsetY, renderedSize, renderedSize), Background);
        foreach (var rectangle in qrCode.ToRectangles())
        {
            canvas.DrawRect(
                new Rectangle(
                    offsetX + (rectangle.X + QuietZone) * moduleSize,
                    offsetY + (rectangle.Y + QuietZone) * moduleSize,
                    rectangle.Width * moduleSize,
                    rectangle.Height * moduleSize),
                Foreground);
        }

        return Data.Size.Zero;
    }

    private float ResolveSize(float dpi, Size remainingSize)
    {
        var bounds = Math.Min(remainingSize.Width, remainingSize.Height);
        return Size.Unit is ELengthUnit.Auto
            ? bounds
            : Size.ToPixels(bounds, dpi);
    }

    private static global::Net.Codecrete.QrCodeGenerator.QrCode.Ecc ToCodecreteErrorCorrection(
        QrCodeErrorCorrectionLevel errorCorrection)
        => errorCorrection switch
        {
            QrCodeErrorCorrectionLevel.Low      => global::Net.Codecrete.QrCodeGenerator.QrCode.Ecc.Low,
            QrCodeErrorCorrectionLevel.Medium   => global::Net.Codecrete.QrCodeGenerator.QrCode.Ecc.Medium,
            QrCodeErrorCorrectionLevel.Quartile => global::Net.Codecrete.QrCodeGenerator.QrCode.Ecc.Quartile,
            QrCodeErrorCorrectionLevel.High     => global::Net.Codecrete.QrCodeGenerator.QrCode.Ecc.High,
            _ => throw new InvalidOperationException($"Unsupported QR code error correction level '{errorCorrection}'."),
        };
}
