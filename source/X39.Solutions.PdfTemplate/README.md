# X39.Solutions.PdfTemplate

`X39.Solutions.PdfTemplate` is the compatibility bridge for existing consumers of the original package.
It preserves the legacy `AddPdfTemplateService()` and `Generator` entry points while delegating the current implementation to Papercraft Core plus the SkiaSharp renderer.

Use this package when existing code already references `X39.Solutions.PdfTemplate`.
New applications should usually start with [`X39.Solutions.Papercraft`](../X39.Solutions.Papercraft/README.md).

## Package Role

| Area | Provided by this package |
|------|--------------------------|
| Legacy DI entry point | `services.AddPdfTemplateService()` |
| Legacy render facade | `Generator` |
| Compatibility builder | `PdfTemplateServiceBuilder` |
| Migration support | Type forwarders for Papercraft contracts and controls |
| Default backend | SkiaSharp through `X39.Solutions.Papercraft.Rendering.SkiaSharp` |

The package references `X39.Solutions.Papercraft`, `X39.Solutions.Papercraft.Core`, `X39.Solutions.Papercraft.Rendering.SkiaSharp`, `Microsoft.Extensions.DependencyInjection.Abstractions` and `X39.Util`.

## Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate;

var services = new ServiceCollection();
services.AddPdfTemplateService();
```

`AddPdfTemplateService()` registers the Papercraft SkiaSharp renderer and the legacy `Generator` wrapper.
It also returns `PdfTemplateServiceBuilder` so existing customization code can continue to add controls, functions and transformers.

```csharp
services.AddPdfTemplateService((builder) =>
{
    builder.AddFunction<MyFunction>();
    builder.ReplaceControl<MyControl>();
});
```

## Render Through The Legacy Generator

```csharp
using System.Globalization;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate;

await using var provider = services.BuildServiceProvider();
using var generator = provider.GetRequiredService<Generator>();

generator.TemplateData.SetVariable("CustomerName", "Ada Lovelace");

using var reader = XmlReader.Create(templateStream);
await using var output = File.Create("document.pdf");

await generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);
```

`Generator.GenerateLoweredXmlAsync()` writes backend-free lowered XML diagnostics for the same template data.
`Generator.GenerateBitmapsAsync()` is still available for callers that expect SkiaSharp bitmap output.

## Migration Notes

Existing code can keep using:

- `services.AddPdfTemplateService()`
- `Generator`
- `Generator.TemplateData`
- `Generator.GenerateLoweredXmlAsync(...)`
- `PdfTemplateServiceBuilder`

New code can move incrementally to:

- `services.AddPapercraft()`
- `PapercraftRenderer`
- `PapercraftRenderOptions`
- `RenderOutput` and `RasterPageRenderOutput`

The template language and default XML controls remain documented in the user manual.

## Related Projects

- [`X39.Solutions.Papercraft`](../X39.Solutions.Papercraft/README.md): default facade for new application code.
- [`X39.Solutions.Papercraft.Core`](../X39.Solutions.Papercraft.Core/README.md): renderer-neutral contracts and runtime.
- [`X39.Solutions.Papercraft.Rendering.SkiaSharp`](../X39.Solutions.Papercraft.Rendering.SkiaSharp/README.md): backend used by this compatibility bridge.
- [`../../docs/manual/migration-to-papercraft.md`](../../docs/manual/migration-to-papercraft.md): migration manual.
