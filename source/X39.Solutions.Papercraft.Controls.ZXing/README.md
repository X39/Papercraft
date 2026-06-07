# X39.Solutions.Papercraft.Controls.ZXing

`X39.Solutions.Papercraft.Controls.ZXing` adds Papercraft barcode controls backed by `ZXing.Net`.
The controls render ZXing `BitMatrix` output as Papercraft vector rectangle commands, so the package depends on `X39.Solutions.Papercraft.Core` but not on SkiaSharp or the compatibility package.

Use this package when templates need Code 128, EAN, UPC, Data Matrix, PDF417, Aztec or other ZXing-supported barcode formats.

## Package Role

| Area | Provided by this package |
|------|--------------------------|
| DI entry point | `AddZxingBarcodeControls()` |
| Core-only DI entry point | `services.AddPapercraftZxingBarcodeControls()` |
| Generic XML control | `<barcode>` |
| Dedicated XML controls | `<code128>`, `<gs1-128>`, `<code39>`, `<code93>`, `<codabar>`, `<ean13>`, `<ean8>`, `<upcA>`, `<upcE>`, `<itf>`, `<dataMatrix>`, `<pdf417>`, `<aztec>` |
| Dependency | `ZXing.Net` |

## Register Controls

For normal applications that already use the default Papercraft facade:

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Controls.ZXing;

services.AddPapercraft()
        .AddZxingBarcodeControls();
```

For a core-only package or custom renderer setup:

```csharp
services.AddPapercraftCore()
        .AddZxingBarcodeControls();
```

`services.AddPapercraftZxingBarcodeControls()` is also available when you only want to register Core plus barcode controls.
It does not add a renderer backend by itself.

## Template Usage

Dedicated Code 128 control:

```xml
<template xmlns="X39.Solutions.PdfTemplate.Controls">
    <body>
        <code128 width="60mm" height="18mm">ORDER-12345</code128>
    </body>
</template>
```

Generic barcode control with a format parameter:

```xml
<barcode format="DataMatrix" width="24mm" height="24mm">
    ORDER-12345
</barcode>
```

Supported generic `format` values are `Aztec`, `Codabar`, `Code39`, `Code93`, `Code128`, `Gs1128`, `DataMatrix`, `Ean8`, `Ean13`, `Itf`, `Pdf417`, `QrCode`, `UpcA` and `UpcE`.

Common parameters:

| Parameter | Purpose | Default |
|-----------|---------|---------|
| `value` or content | Barcode payload | Empty, invalid at render time |
| `format` | ZXing barcode format for `<barcode>` | `Code128` |
| `width` | Rendered width | `50mm` |
| `height` | Rendered height | `15mm` |
| `foreground` | Bar/module color | `black` |
| `background` | Background color | `transparent` |
| `quietZone` | ZXing margin | `0` |
| `gs1Format` | Enable GS1 encoding hints | `false`, forced for `Gs1128` |

## Related Projects

- [`X39.Solutions.Papercraft.Core`](../X39.Solutions.Papercraft.Core/README.md): control contracts used by this package.
- [`X39.Solutions.Papercraft.Controls.QrCode`](../X39.Solutions.Papercraft.Controls.QrCode/README.md): dedicated QR package backed by `Net.Codecrete.QrCodeGenerator`.
- [`../../docs/manual/controls-barcode.md`](../../docs/manual/controls-barcode.md): template-author barcode documentation.
