# X39.Solutions.Papercraft

`X39.Solutions.Papercraft` is the default application-facing package for Papercraft.
It combines the renderer-neutral core runtime with the SkiaSharp renderer so normal consumers can register one package and render XML templates to PDF or raster output.

Use this package when you are writing an application and want the current default Papercraft stack.
Use `X39.Solutions.Papercraft.Core` instead when you are implementing a custom renderer or building a package that must not depend on SkiaSharp.

## Package Role

| Area | Provided by this package |
|------|--------------------------|
| Dependency injection entry point | `services.AddPapercraft()` |
| Main render facade | `PapercraftRenderer` |
| Default backend | `X39.Solutions.Papercraft.Rendering.SkiaSharp` |
| Core contracts | Type-forwarded from `X39.Solutions.Papercraft.Core` |

The package references:

- `X39.Solutions.Papercraft.Core`
- `X39.Solutions.Papercraft.Rendering.SkiaSharp`
- `Microsoft.Extensions.DependencyInjection.Abstractions`

## Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;

var services = new ServiceCollection();
services.AddPapercraft();
```

`AddPapercraft()` registers the core parser, default controls, default transformers, `PapercraftRenderer`, and the SkiaSharp render backend.

Use the overload when adding custom template behavior:

```csharp
services.AddPapercraft((builder) =>
{
    builder.AddFunction<MyFunction>();
    builder.ReplaceControl<MyTextControl>();
});
```

## Render A PDF

```csharp
using System.Globalization;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;

await using var provider = services.BuildServiceProvider();
var renderer = provider.GetRequiredService<PapercraftRenderer>();

using var reader = XmlReader.Create(templateStream);
await using var output = File.Create("document.pdf");

await renderer.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);
```

The same renderer can validate templates before rendering and can write page-by-page PNG raster output through `RenderRasterPagesAsync`.

## Template Data

Template data belongs to the resolved renderer instance:

```csharp
renderer.TemplateData.SetVariable("CustomerName", "Ada Lovelace");
```

The template can then read `@CustomerName` through the template language.

## Related Projects

- [`X39.Solutions.Papercraft.Core`](../X39.Solutions.Papercraft.Core/README.md): renderer-neutral contracts, parser, controls, data types and transformers.
- [`X39.Solutions.Papercraft.Rendering.SkiaSharp`](../X39.Solutions.Papercraft.Rendering.SkiaSharp/README.md): default PDF and raster backend.
- [`X39.Solutions.PdfTemplate`](../X39.Solutions.PdfTemplate/README.md): compatibility bridge for existing `X39.Solutions.PdfTemplate` consumers.

The template-author manual lives in [`../../docs/manual`](../../docs/manual/index.md).
