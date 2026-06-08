# Optional Control Packages

Previous: [Chart controls](controls-chart.md) | [Controls](controls.md) | Next: [Transformers](transformers.md)

Papercraft barcode controls live in optional control packages.
They are not registered by `AddPapercraftCore()`, `AddPapercraft()` or `AddPdfTemplateService()` unless the
application installs and registers the matching package.

This overview remains for older links. Use the package-specific pages for current setup and attribute details.

| Package | Controls | Documentation |
|---------|----------|---------------|
| `X39.Solutions.Papercraft.Controls.QrCode` | `qrCode` | [QR code control package](controls-qrcode.md) |
| `X39.Solutions.Papercraft.Controls.ZXing` | `barcode`, `code128`, `gs1-128`, `code39`, `code93`, `codabar`, `ean13`, `ean8`, `upcA`, `upcE`, `itf`, `dataMatrix`, `pdf417`, `aztec` | [ZXing barcode control package](controls-zxing.md) |

Use `X39.Solutions.Papercraft.Controls.QrCode` when templates only need QR codes and you want the dedicated
`Net.Codecrete.QrCodeGenerator` dependency.
Use `X39.Solutions.Papercraft.Controls.ZXing` when templates need general 1D and 2D barcode formats.

Both packages register controls in the normal Papercraft control namespace, so template XML uses the same
unprefixed style as built-in controls after the application registers the package.

Previous: [Chart controls](controls-chart.md) | [Controls](controls.md) | Next: [Transformers](transformers.md)
