# Migration To Papercraft

Previous: [Developer appendix](developer-integration.md) | [Manual home](index.md)

Papercraft is the new product name for the template-driven rendering engine that started as
`X39.Solutions.PdfTemplate`.

The migration is additive. Existing applications can keep the current package and continue using:

```csharp
services.AddPdfTemplateService();
using var generator = serviceProvider.GetRequiredService<Generator>();
await generator.GeneratePdfAsync(output, reader, CultureInfo.CurrentUICulture);
```

New code can use the Papercraft facade from the same compatibility package:

```csharp
services.AddPapercraft();
var generator = serviceProvider.GetRequiredService<PapercraftGenerator>();
await generator.GeneratePdfAsync(output, reader, CultureInfo.CurrentUICulture);
```

## Package Direction

The current package remains `X39.Solutions.PdfTemplate` during the compatibility period.
The planned package split is:

| Package | Purpose |
|---------|---------|
| `X39.Papercraft` | Default facade for application developers. |
| `X39.Papercraft.Core` | Renderer-neutral template, layout, capability and validation contracts. |
| `X39.Papercraft.Rendering.SkiaSharp` | Default PDF and raster renderer. |
| `X39.Solutions.PdfTemplate` | Compatibility bridge for existing users. |

## API Mapping

| Existing API | Papercraft API |
|--------------|----------------|
| `AddPdfTemplateService()` | `AddPapercraft()` |
| `PdfTemplateServiceBuilder` | `PapercraftServiceBuilder` |
| `Generator.GeneratePdfAsync(...)` | `PapercraftGenerator.GeneratePdfAsync(...)` |
| `DocumentOptions` | `PapercraftRenderOptions.DocumentOptions` |
| implicit Skia renderer choice | renderer capability validation through `ValidateAsync(...)` |

## Validation

Papercraft renderers expose capabilities before rendering.
Call `ValidateAsync` when an application lets users choose output formats or renderer backends:

```csharp
var result = await generator.ValidateAsync(
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

## Compatibility Notes

`GenerateBitmapsAsync` still returns `SKBitmap` from the compatibility API.
Use it for existing raster workflows until the multi-page renderer-neutral raster API is finalized.

Custom controls, transformers and functions can be registered through either builder during the
compatibility period. New custom controls should avoid accepting Skia types directly so they can move
to renderer-neutral Papercraft APIs later.
