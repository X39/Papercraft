***Note for [NuGet.org](https://www.nuget.org/packages/X39.Solutions.PdfTemplate):***
*This README is intentionally short. For the full template-author documentation, use the
[GitHub Pages user manual](https://x39.github.io/X39.Solutions.PdfTemplate/manual/).*

![A sample output for reference](https://raw.githubusercontent.com/X39/X39.Solutions.PdfTemplate/master/.github/media/sample.png)

# Papercraft

Papercraft is a template-driven document rendering engine for XML templates in .NET.
The current default backend renders through SkiaSharp and supports PDF output, raster output,
and built-in controls for text, rich text, borders, images, lines, tables, lists, page numbers,
charts and related layout elements.
Papercraft also exposes a backend-free lowered XML target for diagnosing the template after data binding and
transformer expansion, before controls and layout run.

The existing `X39.Solutions.PdfTemplate` package remains the compatibility bridge during the
Papercraft migration. Existing users can keep `services.AddPdfTemplateService()` and `Generator`;
new code can start using `services.AddPapercraft()` and `PapercraftRenderer`.

## User Manual

Template authors should start with the
[GitHub Pages user manual](https://x39.github.io/X39.Solutions.PdfTemplate/manual/).
The manual explains document structure, template data, layout, controls, transformers,
complete examples and troubleshooting from the perspective of people who edit XML templates.
The Pages site includes a persistent manual table of contents so chapters can be reached
without returning to the landing page.

Useful manual entry points:

- [First document](https://x39.github.io/X39.Solutions.PdfTemplate/manual/first-document.html)
- [Template data](https://x39.github.io/X39.Solutions.PdfTemplate/manual/template-data.html)
- [Layout fundamentals](https://x39.github.io/X39.Solutions.PdfTemplate/manual/layout-fundamentals.html)
- [Quick reference](https://x39.github.io/X39.Solutions.PdfTemplate/manual/quick-reference.html)
- [Controls](https://x39.github.io/X39.Solutions.PdfTemplate/manual/controls.html)
- [Template language](https://x39.github.io/X39.Solutions.PdfTemplate/manual/template-language.html)
- [Complete examples](https://x39.github.io/X39.Solutions.PdfTemplate/manual/complete-examples.html)
- [Developer integration appendix](https://x39.github.io/X39.Solutions.PdfTemplate/manual/developer-integration.html)
- [Renderer backends](https://x39.github.io/X39.Solutions.PdfTemplate/manual/render-backends.html)

## Requirements

- .NET 10.0 or later
- A dependency injection container that can provide the services registered by `AddPapercraft`
- On Linux, the SkiaSharp native Linux assets package

The compatibility package is marked trim-compatible. The default rendering path uses the
SkiaSharp-backed Papercraft renderer and the dependency-injection abstractions.
Issues are tracked in the
[GitHub repository](https://github.com/X39/X39.Solutions.PdfTemplate/issues).

## Install

Install the current compatibility package:

```shell
dotnet add package X39.Solutions.PdfTemplate
```

The source tree now follows this partial Papercraft package split:

| Package | Use |
|---------|-----|
| `X39.Solutions.Papercraft` | Batteries-included facade for normal PDF users. |
| `X39.Solutions.Papercraft.Core` | Renderer-neutral contracts plus the current shared parsing, layout, control, data and validation runtime. |
| `X39.Solutions.Papercraft.Rendering.SkiaSharp` | SkiaSharp PDF/raster renderer and runtime services. |
| `X39.Solutions.Papercraft.Rendering.Svg` | Dependency-free SVG vector renderer. |
| `X39.Solutions.Papercraft.Rendering.PdfSharp` | PDFsharp-backed PDF renderer. |
| `X39.Solutions.Papercraft.Rendering.EscPos` | First-pass ESC/POS printer-command renderer. |
| `X39.Solutions.Papercraft.OpenTelemetry` | Optional host/OpenTelemetry integration for Papercraft renderer activity tracing. |
| `X39.Solutions.Papercraft.Controls.QrCode` | Optional QR code control package backed by `Net.Codecrete.QrCodeGenerator`. |
| `X39.Solutions.Papercraft.Controls.ZXing` | Optional general barcode control package backed by `ZXing.Net`. |
| `X39.Solutions.PdfTemplate` | Compatibility bridge for existing users and package metadata during the migration. |

## Project READMEs

The root README is the solution overview. Each project now has its own README for package-specific setup,
public entry points and contributor notes:

| Project | README |
|---------|--------|
| `X39.Solutions.Papercraft` | [`source/X39.Solutions.Papercraft/README.md`](source/X39.Solutions.Papercraft/README.md) |
| `X39.Solutions.Papercraft.Core` | [`source/X39.Solutions.Papercraft.Core/README.md`](source/X39.Solutions.Papercraft.Core/README.md) |
| `X39.Solutions.Papercraft.Rendering.SkiaSharp` | [`source/X39.Solutions.Papercraft.Rendering.SkiaSharp/README.md`](source/X39.Solutions.Papercraft.Rendering.SkiaSharp/README.md) |
| `X39.Solutions.Papercraft.Rendering.Svg` | [`source/X39.Solutions.Papercraft.Rendering.Svg/README.md`](source/X39.Solutions.Papercraft.Rendering.Svg/README.md) |
| `X39.Solutions.Papercraft.Rendering.PdfSharp` | [`source/X39.Solutions.Papercraft.Rendering.PdfSharp/README.md`](source/X39.Solutions.Papercraft.Rendering.PdfSharp/README.md) |
| `X39.Solutions.Papercraft.Rendering.EscPos` | [`source/X39.Solutions.Papercraft.Rendering.EscPos/README.md`](source/X39.Solutions.Papercraft.Rendering.EscPos/README.md) |
| `X39.Solutions.Papercraft.OpenTelemetry` | [`source/X39.Solutions.Papercraft.OpenTelemetry/README.md`](source/X39.Solutions.Papercraft.OpenTelemetry/README.md) |
| `X39.Solutions.Papercraft.Controls.QrCode` | [`source/X39.Solutions.Papercraft.Controls.QrCode/README.md`](source/X39.Solutions.Papercraft.Controls.QrCode/README.md) |
| `X39.Solutions.Papercraft.Controls.ZXing` | [`source/X39.Solutions.Papercraft.Controls.ZXing/README.md`](source/X39.Solutions.Papercraft.Controls.ZXing/README.md) |
| `X39.Solutions.PdfTemplate` | [`source/X39.Solutions.PdfTemplate/README.md`](source/X39.Solutions.PdfTemplate/README.md) |
| `X39.Solutions.PdfTemplate.Test` | [`test/X39.Solutions.PdfTemplate.Test/README.md`](test/X39.Solutions.PdfTemplate.Test/README.md) |
| `X39.Solutions.PdfTemplate.Benchmark` | [`benchmark/X39.Solutions.PdfTemplate.Benchmark/README.md`](benchmark/X39.Solutions.PdfTemplate.Benchmark/README.md) |

On Linux, also install the
[SkiaSharp native Linux assets](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux):

```shell
dotnet add package SkiaSharp.NativeAssets.Linux
```

## Minimal Template

Templates are XML documents:

```xml
<template>
    <body>
        <text>Hello, world!</text>
    </body>
</template>
```

For template-author guidance, use the
[manual](https://x39.github.io/X39.Solutions.PdfTemplate/manual/).
For application setup, use the
[developer integration appendix](https://x39.github.io/X39.Solutions.PdfTemplate/manual/developer-integration.html).

## Generate a PDF With Papercraft

Register the library services at startup:

```csharp
services.AddPapercraft();
```

Then resolve a `PapercraftRenderer` and render the template:

```csharp
using System.Globalization;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;

await using var scope = serviceProvider.CreateAsyncScope();
var renderer = scope.ServiceProvider.GetRequiredService<PapercraftRenderer>();

using var reader = XmlReader.Create(xmlTemplateStream);
await using var output = File.Create("document.pdf");

await renderer.GeneratePdfAsync(
    output,
    reader,
    CultureInfo.CurrentUICulture);
```

## Existing PdfTemplate Users

The existing API is still available:

```csharp
services.AddPdfTemplateService();

using var generator = scope.ServiceProvider.GetRequiredService<Generator>();
await generator.GeneratePdfAsync(output, reader, CultureInfo.CurrentUICulture);
```

`AddPdfTemplateService()` also registers the Papercraft renderer stack, so applications can migrate
call sites before changing their service setup.

Common extension points are documented in the
[developer integration appendix](https://x39.github.io/X39.Solutions.PdfTemplate/manual/developer-integration.html):

- Set template variables with `generator.TemplateData.SetVariable("Name", value)`.
- Diagnose generated template structure with `GenerateLoweredXmlAsync(...)` or `RenderTarget.LoweredXml`.
- Add custom functions with `services.AddPapercraft((builder) => builder.AddFunction<MyFunction>())`.
- Add custom controls with `services.AddPapercraft((builder) => builder.AddControl<TControl>())`.
- Add custom transformers with `services.AddPapercraft((builder) => builder.AddTransformer<TTransformer>())`.
- Configure document-level options through `PapercraftRenderOptions`.
- Use `ValidateAsync` to check renderer capabilities and diagnostics before rendering.

## Building and Testing

Restore and build locally:

```shell
dotnet restore
dotnet build --no-restore
```

Run tests:

```shell
dotnet test --framework net10.0 --no-build --verbosity normal
```

Run focused PDF comparison benchmarks:

```shell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --anyCategories Comparison --job Short --warmupCount 3 --iterationCount 10
```

The comparison benchmark intentionally measures a deterministic 28-row invoice against direct SkiaSharp PDF/A output,
QuestPDF PDF output and Papercraft XML template PDF/A output. Current local results are tracked in the benchmark README
rather than advertised as a package performance claim: Papercraft is close to the current SkiaSharp PDF/A backend path,
but still several times slower than QuestPDF on this shape. The current tuning target for this benchmark is below 12 ms
and below 2 MB allocated for Papercraft XML PDF/A generation on the maintainer Ryzen 9 5900X/.NET 10 harness.

Create a local package:

```shell
dotnet pack --configuration Release
```

The pull-request workflow is defined in `.github/workflows/run-dotnet-tests.yml`.
The publish workflow is defined in `.github/workflows/main.yml`.

## Documentation

The GitHub Pages source lives under [`docs`](docs/index.md).
The Pages table of contents is maintained in [`docs/_data/navigation.yml`](docs/_data/navigation.yml).
Executable documentation samples live under `test/X39.Solutions.PdfTemplate.Test/Samples`
and write generated preview assets to ignored test output by default.
Set `PAPERCRAFT_UPDATE_DOCUMENTATION_SAMPLE_ASSETS=true` when running those tests to regenerate checked-in assets under
`docs/assets/samples`.

## Contributing

Contributions are welcome.
Please submit a pull request or create a discussion to discuss changes.

Add yourself to [`CONTRIBUTORS`](CONTRIBUTORS.md) for your first pull request and include this agreement text:

```text
By contributing to this project, you agree to the following terms:
- You grant me and any other person who receives a copy of this project the right to use your contribution under the
  terms of the GNU Lesser General Public License v3.0.
- You grant me and any other person who receives a copy of this project the right to relicense your contribution under
  any other license.
- You grant me and any other person who receives a copy of this project the right to change your contribution.
- You waive your right to your contribution and transfer all rights to me and every user of this project.
- You agree that your contribution is free of any third-party rights.
- You agree that your contribution is given without any compensation.
- You agree that I may remove your contribution at any time for any reason.
- You confirm that you have the right to grant the above rights and that you are not violating any third-party rights
  by granting these rights.
- You confirm that your contribution is not subject to any license agreement or other agreement or obligation, which
  conflicts with the above terms.
```

Additional controls are welcome when they do not add dependencies to the core library.
Controls that need additional libraries should usually live in separate packages.

## Semantic Versioning

This library follows the principles of [Semantic Versioning](https://semver.org/).

| Change | Meaning |
|--------|---------|
| Patch | Backwards-compatible bug fixes or small internal changes. |
| Minor | Backwards-compatible features or additions. |
| Major | Breaking changes. |

## License

This project is licensed under the GNU Lesser General Public License v3.0.
See the [LICENSE](LICENSE) file for details.
