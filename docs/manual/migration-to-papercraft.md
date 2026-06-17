# Migration To Papercraft

Previous: [Renderer backends](render-backends.md) | [Manual home](index.md)

Papercraft is the new product name for the template-driven rendering engine that started as
`X39.Solutions.PdfTemplate`.

Maintainer implementation details are tracked in the
[Papercraft architecture plan](papercraft-architecture-plan.md).

The migration is additive. Existing applications can keep the compatibility package and continue using:

```csharp
services.AddPdfTemplateService();
using var generator = serviceProvider.GetRequiredService<Generator>();
await generator.GeneratePdfAsync(output, reader, CultureInfo.CurrentUICulture);
```

New code should use the Papercraft facade package:

```csharp
using X39.Solutions.Papercraft;

services.AddPapercraft();
var papercraft = serviceProvider.GetRequiredService<Papercraft>();
await using var session = papercraft.CreateSession();
await session.GeneratePdfAsync(output, reader, CultureInfo.CurrentUICulture);
```

The compatibility package also forwards the Papercraft facade entry points during the migration period,
but new application projects should reference `X39.Solutions.Papercraft` directly.

## Package Direction

The legacy compatibility package remains `X39.Solutions.PdfTemplate` during the compatibility period.
The source tree now has the Papercraft package split:

| Package | Purpose |
|---------|---------|
| `X39.Solutions.Papercraft` | Default facade for application developers. It depends on core plus the SkiaSharp renderer and exposes `AddPapercraft()`. |
| `X39.Solutions.Papercraft.Core` | Renderer-neutral contracts plus the current shared parsing, template data, control, transformer, layout and validation runtime. |
| `X39.Solutions.Papercraft.Rendering.SkiaSharp` | Default PDF and raster renderer, including Skia canvas, text, image, paint and bitmap services. |
| `X39.Solutions.Papercraft.Rendering.Svg` | Optional SVG vector renderer. |
| `X39.Solutions.Papercraft.Rendering.PdfSharp` | Optional PDFsharp-backed PDF renderer. |
| `X39.Solutions.Papercraft.Rendering.EscPos` | Optional first-pass ESC/POS printer-command renderer. |
| `X39.Solutions.Papercraft.Controls.QrCode` | Optional QR code controls. Depends on `X39.Solutions.Papercraft.Core` and `Net.Codecrete.QrCodeGenerator`. |
| `X39.Solutions.Papercraft.Controls.ZXing` | Optional broad barcode controls. Depends on `X39.Solutions.Papercraft.Core` and `ZXing.Net`. |
| `X39.Solutions.PdfTemplate` | Compatibility bridge for existing users. It keeps old service registration and type identities available through forwarding or wrappers. |

Core and the SkiaSharp renderer now own real runtime code, and the facade and bridge reference them.
Barcode packages are deliberately opt-in and are not referenced by the core, facade, SkiaSharp renderer or compatibility bridge.
Local package consumption, diagnostics, documentation alignment and PDF/PNG parity smoke coverage are
in place. Remaining migration work is about binary compatibility hardening, release publishing and
expanded renderer support.

## API Mapping

| Existing API | Papercraft API |
|--------------|----------------|
| `AddPdfTemplateService()` | `AddPapercraft()` |
| `PdfTemplateServiceBuilder` | `PapercraftServiceBuilder` |
| `Generator.GeneratePdfAsync(...)` | `PapercraftSession.GeneratePdfAsync(...)`, or `RenderAsync(...)` with `RenderTarget.Pdf` for custom output plumbing |
| `Generator.GenerateLoweredXmlAsync(...)` | `PapercraftSession.GenerateLoweredXmlAsync(...)`, `ReadLoweredXmlAsync(...)`, or `RenderAsync(..., RenderTarget.LoweredXml, ...)` |
| `DocumentOptions` | `PapercraftRenderOptions.DocumentOptions` |
| implicit Skia renderer choice | renderer capability validation through `ValidateAsync(...)` |

## Validation

Papercraft renderers expose capabilities before rendering.
Call `ValidateAsync` when an application lets users choose output formats or renderer backends:

```csharp
var result = await session.ValidateAsync(
    reader,
    RenderTarget.Pdf,
    CultureInfo.CurrentUICulture);

if (!result.IsSupported)
{
    // Show result.Diagnostics to the user.
}
```

Unsupported diagnostics block rendering. Degraded diagnostics are warnings unless
`PapercraftRenderOptions.TreatDegradedAsUnsupported` is enabled.
`RenderTarget.LoweredXml` is a diagnostic target that stops before backend rendering, so it is supported without a
registered render backend.

## Compatibility Notes

`RenderRasterPagesAsync` is the renderer-neutral multi-page raster API. It writes each encoded page
to a caller-provided stream callback and exposes neutral page metadata instead of SkiaSharp objects.
`GenerateBitmapsAsync` still returns `SKBitmap` from the SkiaSharp runtime available through the
compatibility path for existing raster workflows.

Custom controls, transformers and functions can be registered through either builder during the
compatibility period. New custom controls should avoid accepting Skia types directly so they can move
to renderer-neutral Papercraft APIs later.

Skia-specific shims and obsolete warnings stay out of `X39.Solutions.Papercraft.Core`. APIs that expose
`SKBitmap`, Skia canvas/paint types or Skia native asset behavior remain in the compatibility package
or the SkiaSharp renderer package until a documented neutral replacement exists.
