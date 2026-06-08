# X39.Solutions.Papercraft.Controls.QrCode

`X39.Solutions.Papercraft.Controls.QrCode` adds a dedicated Papercraft `qrCode` XML control backed by `Net.Codecrete.QrCodeGenerator`.
The control renders QR modules as Papercraft vector rectangle commands, so it stays renderer-neutral and does not depend on SkiaSharp.

Use this package when templates need QR codes and you do not need the broader ZXing barcode package.

## Package Role

| Area | Provided by this package |
|------|--------------------------|
| DI entry point | `AddQrCodeControls()` |
| Core-only DI entry point | `services.AddPapercraftQrCodeControls()` |
| XML control | `<qrCode>` |
| Dependency | `Net.Codecrete.QrCodeGenerator` |

## Register Controls

For normal applications that already use the default Papercraft facade:

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Controls.QrCode;

services.AddPapercraft()
        .AddQrCodeControls();
```

For a core-only package or custom renderer setup:

```csharp
services.AddPapercraftCore()
        .AddQrCodeControls();
```

`services.AddPapercraftQrCodeControls()` is also available when you only want to register Core plus QR controls.
It does not add a renderer backend by itself.

## Template Usage

```xml
<template xmlns="X39.Solutions.PdfTemplate.Controls">
    <body>
        <qrCode size="30mm" errorCorrection="High">
            https://example.com/order/123
        </qrCode>
    </body>
</template>
```

Supported parameters:

| Parameter | Purpose | Default |
|-----------|---------|---------|
| `value` or content | QR code payload | Empty, invalid at render time |
| `size` | Rendered square size | `25mm` |
| `foreground` | Module color | `black` |
| `background` | Background color | `transparent` |
| `quietZone` | Quiet-zone module count | `4` |
| `errorCorrection` | `Low`, `Medium`, `Quartile` or `High` | `Medium` |

## Related Projects

- [`X39.Solutions.Papercraft.Core`](../X39.Solutions.Papercraft.Core/README.md): control contracts used by this package.
- [`X39.Solutions.Papercraft.Controls.ZXing`](../X39.Solutions.Papercraft.Controls.ZXing/README.md): optional package for general 1D and 2D barcode controls.
- [`../../docs/manual/controls-qrcode.md`](../../docs/manual/controls-qrcode.md): template-author QR code documentation.
