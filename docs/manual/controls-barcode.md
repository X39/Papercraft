# Optional Barcode Controls

Previous: [Chart controls](controls-chart.md) | [Controls](controls.md) | Next: [Transformers](transformers.md)

Barcode controls are optional Papercraft packages. They are not registered by `AddPapercraftCore()`,
`AddPapercraft()` or `AddPdfTemplateService()` unless the application installs and registers the matching package.

Use `X39.Solutions.Papercraft.Controls.QrCode` when you only need QR codes.
Use `X39.Solutions.Papercraft.Controls.ZXing` when you need common 1D or 2D barcode formats.

## Register The Packages

QR-only:

```shell
dotnet add package X39.Solutions.Papercraft.Controls.QrCode
```

```csharp
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Controls.QrCode;

services.AddPapercraft()
        .AddQrCodeControls();
```

For a renderer-neutral or custom renderer setup, use `services.AddPapercraftCore().AddQrCodeControls()`.
`services.AddPapercraftQrCodeControls()` is also available when you only want Core plus QR controls.

ZXing barcodes:

```shell
dotnet add package X39.Solutions.Papercraft.Controls.ZXing
```

```csharp
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Controls.ZXing;

services.AddPapercraft()
        .AddZxingBarcodeControls();
```

For a renderer-neutral or custom renderer setup, use `services.AddPapercraftCore().AddZxingBarcodeControls()`.
`services.AddPapercraftZxingBarcodeControls()` is also available when you only want Core plus barcode controls.

Both packages register controls in the normal Papercraft control namespace, so templates use the same
unprefixed XML style as built-in controls.

## QR Code

Use `qrCode` for QR symbols. The value can be an attribute or element content.

```xml
<qrCode value="https://example.test/order/123" size="24mm" quietZone="4" errorCorrection="M"/>

<qrCode size="24mm" foreground="#000000" background="#ffffff">
    https://example.test/order/123
</qrCode>
```

| Attribute | Values | Default |
|-----------|--------|---------|
| `value` | Text to encode. Element content can also supply the value. | Empty |
| `size` | [Length](quick-reference.md#value-formats). | `25mm` |
| `foreground` | [Color](quick-reference.md#value-formats). | `black` |
| `background` | [Color](quick-reference.md#value-formats). | `transparent` |
| `quietZone` | Non-negative module count. | `4` |
| `errorCorrection` | `L`, `M`, `Q`, `H` or `Low`, `Medium`, `Quartile`, `High`. | `Medium` |

The QR package uses `Net.Codecrete.QrCodeGenerator` and draws module rectangles directly; it does not
insert a raster image into the template.

## ZXing Barcode

Use `barcode` when the format should be selected by an attribute.

```xml
<barcode format="Code128" value="ABC123" width="42mm" height="12mm"/>
<barcode format="DataMatrix" width="22mm" height="22mm">ABC123</barcode>
```

| Attribute | Values | Default |
|-----------|--------|---------|
| `value` | Text to encode. Element content can also supply the value. | Empty |
| `format` | `Aztec`, `Codabar`, `Code39`, `Code93`, `Code128`, `GS1-128`, `DataMatrix`, `EAN8`, `EAN13`, `ITF`, `PDF417`, `QRCode`, `UPCA`, `UPCE`. | `Code128` |
| `width` | [Length](quick-reference.md#value-formats). | `50mm` |
| `height` | [Length](quick-reference.md#value-formats). | `15mm` |
| `foreground` | [Color](quick-reference.md#value-formats). | `black` |
| `background` | [Color](quick-reference.md#value-formats). | `transparent` |
| `quietZone` | Non-negative module count passed to ZXing. | `0` |
| `gs1Format` | `true` or `false`. Enables ZXing GS1 mode for compatible formats. | `false` |

The ZXing package uses `ZXing.Net` and renders the generated `BitMatrix` as vector rectangles.
Matrix formats such as QR Code, Data Matrix and Aztec keep square modules when the arranged control is not square.

## Alias Controls

Use aliases when the template format is fixed and should be visible from the XML element name:

```xml
<code128 value="ABC123" width="42mm" height="12mm"/>
<ean13 value="4006381333931" width="42mm" height="14mm"/>
<dataMatrix value="ABC123" width="22mm" height="22mm"/>
```

Available aliases:

| Element | Format |
|---------|--------|
| `code128` | Code 128 |
| `gs1-128` | GS1 Code 128 with GS1 mode enabled |
| `code39` | Code 39 |
| `code93` | Code 93 |
| `codabar` | Codabar |
| `ean13` | EAN-13 |
| `ean8` | EAN-8 |
| `upcA` | UPC-A |
| `upcE` | UPC-E |
| `itf` | Interleaved 2 of 5 |
| `dataMatrix` | Data Matrix |
| `pdf417` | PDF417 |
| `aztec` | Aztec |

Alias controls support the same `value`, sizing, color, `quietZone` and `gs1Format` attributes as `barcode`.
Most aliases inherit the generic `50mm` by `15mm` default size; `dataMatrix` and `aztec` default to `25mm` by
`25mm`, and `pdf417` defaults to `50mm` by `20mm`.

Invalid payloads are rejected by the underlying encoder. For example, EAN and UPC formats require
numeric values with the expected length and checksum behavior for that barcode type.

Previous: [Chart controls](controls-chart.md) | [Controls](controls.md) | Next: [Transformers](transformers.md)
