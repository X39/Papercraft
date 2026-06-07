using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;
using ZxBarcodeFormat = global::ZXing.BarcodeFormat;
using ZxingBarcodeWriter = global::ZXing.BarcodeWriterGeneric;
using ZxingBitMatrix = global::ZXing.Common.BitMatrix;
using ZxingEncodingOptions = global::ZXing.Common.EncodingOptions;

namespace X39.Solutions.Papercraft.Controls.ZXing.Controls;

[Control(Constants.ControlsNamespace, "barcode")]
public class BarcodeControl : AlignableControl
{
    public BarcodeControl() : this(ZxBarcodeFormat.CODE_128)
    {
    }

    protected BarcodeControl(ZxBarcodeFormat format)
    {
        Format = ToPapercraftFormat(format);
    }

    protected BarcodeControl(ZxBarcodeFormat format, bool gs1Format) : this(format)
    {
        Gs1Format = gs1Format;
    }

    [Parameter(IsContent = true)]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public Controls.ZxingBarcodeFormat Format { get; set; } = Controls.ZxingBarcodeFormat.Code128;

    [Parameter]
    public Length Width { get; set; } = new(50, ELengthUnit.Millimeters);

    [Parameter]
    public Length Height { get; set; } = new(15, ELengthUnit.Millimeters);

    [Parameter]
    public Color Foreground { get; set; } = Colors.Black;

    [Parameter]
    public Color Background { get; set; } = Colors.Transparent;

    [Parameter]
    public int QuietZone { get; set; }

    [Parameter]
    public bool Gs1Format { get; set; }

    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => ResolveSize(dpi, remainingSize);

    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => ResolveSize(dpi, remainingSize);

    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var matrix = EncodeMatrix();
        var availableWidth = ArrangementInner.Width;
        var availableHeight = ArrangementInner.Height;
        if (availableWidth <= 0 || availableHeight <= 0)
            return Size.Zero;

        canvas.DrawRect(new Rectangle(0, 0, availableWidth, availableHeight), Background);

        var preserveSquareModules = Format is Controls.ZxingBarcodeFormat.Aztec
            or Controls.ZxingBarcodeFormat.DataMatrix
            or Controls.ZxingBarcodeFormat.QrCode;
        var moduleWidth = availableWidth / matrix.Width;
        var moduleHeight = availableHeight / matrix.Height;
        var offsetX = 0F;
        var offsetY = 0F;
        if (preserveSquareModules)
        {
            var moduleSize = Math.Min(moduleWidth, moduleHeight);
            moduleWidth = moduleSize;
            moduleHeight = moduleSize;
            offsetX = (availableWidth - moduleSize * matrix.Width) / 2;
            offsetY = (availableHeight - moduleSize * matrix.Height) / 2;
        }

        RenderMatrix(canvas, matrix, offsetX, offsetY, moduleWidth, moduleHeight);
        return Size.Zero;
    }

    private Size ResolveSize(float dpi, Size remainingSize)
        => new(
            Width.ToPixels(remainingSize.Width, dpi),
            Height.ToPixels(remainingSize.Height, dpi));

    private ZxingBitMatrix EncodeMatrix()
    {
        if (string.IsNullOrEmpty(Value))
            throw new InvalidOperationException("Barcode value must not be empty.");
        if (QuietZone < 0)
            throw new InvalidOperationException("Barcode quiet zone must not be negative.");

        var writer = new ZxingBarcodeWriter
        {
            Format = ToZxingFormat(Format),
            Options = new ZxingEncodingOptions
            {
                Width = 1,
                Height = 1,
                Margin = QuietZone,
                GS1Format = Gs1Format || Format is Controls.ZxingBarcodeFormat.Gs1128,
            },
        };
        return writer.Encode(Value);
    }

    private void RenderMatrix(
        IDeferredCanvas canvas,
        ZxingBitMatrix matrix,
        float offsetX,
        float offsetY,
        float moduleWidth,
        float moduleHeight)
    {
        for (var y = 0; y < matrix.Height; y++)
        {
            var x = 0;
            while (x < matrix.Width)
            {
                while (x < matrix.Width && !matrix[x, y])
                    x++;
                var runStart = x;
                while (x < matrix.Width && matrix[x, y])
                    x++;
                var runLength = x - runStart;
                if (runLength <= 0)
                    continue;

                canvas.DrawRect(
                    new Rectangle(
                        offsetX + runStart * moduleWidth,
                        offsetY + y * moduleHeight,
                        runLength * moduleWidth,
                        moduleHeight),
                    Foreground);
            }
        }
    }

    private static ZxBarcodeFormat ToZxingFormat(Controls.ZxingBarcodeFormat format)
        => format switch
        {
            Controls.ZxingBarcodeFormat.Aztec      => ZxBarcodeFormat.AZTEC,
            Controls.ZxingBarcodeFormat.Codabar    => ZxBarcodeFormat.CODABAR,
            Controls.ZxingBarcodeFormat.Code39     => ZxBarcodeFormat.CODE_39,
            Controls.ZxingBarcodeFormat.Code93     => ZxBarcodeFormat.CODE_93,
            Controls.ZxingBarcodeFormat.Code128    => ZxBarcodeFormat.CODE_128,
            Controls.ZxingBarcodeFormat.Gs1128     => ZxBarcodeFormat.CODE_128,
            Controls.ZxingBarcodeFormat.DataMatrix => ZxBarcodeFormat.DATA_MATRIX,
            Controls.ZxingBarcodeFormat.Ean8       => ZxBarcodeFormat.EAN_8,
            Controls.ZxingBarcodeFormat.Ean13      => ZxBarcodeFormat.EAN_13,
            Controls.ZxingBarcodeFormat.Itf        => ZxBarcodeFormat.ITF,
            Controls.ZxingBarcodeFormat.Pdf417     => ZxBarcodeFormat.PDF_417,
            Controls.ZxingBarcodeFormat.QrCode     => ZxBarcodeFormat.QR_CODE,
            Controls.ZxingBarcodeFormat.UpcA       => ZxBarcodeFormat.UPC_A,
            Controls.ZxingBarcodeFormat.UpcE       => ZxBarcodeFormat.UPC_E,
            _ => throw new InvalidOperationException($"Unsupported ZXing barcode format '{format}'."),
        };

    private static Controls.ZxingBarcodeFormat ToPapercraftFormat(ZxBarcodeFormat format)
        => format switch
        {
            ZxBarcodeFormat.AZTEC       => Controls.ZxingBarcodeFormat.Aztec,
            ZxBarcodeFormat.CODABAR     => Controls.ZxingBarcodeFormat.Codabar,
            ZxBarcodeFormat.CODE_39     => Controls.ZxingBarcodeFormat.Code39,
            ZxBarcodeFormat.CODE_93     => Controls.ZxingBarcodeFormat.Code93,
            ZxBarcodeFormat.CODE_128    => Controls.ZxingBarcodeFormat.Code128,
            ZxBarcodeFormat.DATA_MATRIX => Controls.ZxingBarcodeFormat.DataMatrix,
            ZxBarcodeFormat.EAN_8       => Controls.ZxingBarcodeFormat.Ean8,
            ZxBarcodeFormat.EAN_13      => Controls.ZxingBarcodeFormat.Ean13,
            ZxBarcodeFormat.ITF         => Controls.ZxingBarcodeFormat.Itf,
            ZxBarcodeFormat.PDF_417     => Controls.ZxingBarcodeFormat.Pdf417,
            ZxBarcodeFormat.QR_CODE     => Controls.ZxingBarcodeFormat.QrCode,
            ZxBarcodeFormat.UPC_A       => Controls.ZxingBarcodeFormat.UpcA,
            ZxBarcodeFormat.UPC_E       => Controls.ZxingBarcodeFormat.UpcE,
            _ => throw new InvalidOperationException($"Unsupported ZXing barcode format '{format}'."),
        };
}
