# X39.Solutions.Papercraft.Core

`X39.Solutions.Papercraft.Core` contains the renderer-neutral Papercraft runtime.
It defines the public contracts for templates, controls, data conversion, display lists, render targets, renderer capabilities and validation.
It also contains the current XML parser, layout/runtime implementation, built-in controls and built-in template transformers during the Papercraft package split.

Use this package when you need Papercraft contracts without taking a dependency on SkiaSharp or the legacy compatibility package.

## Package Role

| Area | Provided by this package |
|------|--------------------------|
| Core DI entry point | `services.AddPapercraftCore()` |
| Backend-neutral generation | `PapercraftGenerator` |
| Render facade and contracts | `PapercraftRenderer`, `IPapercraftRenderBackend`, `RenderTarget`, `RenderOutput` |
| Capability validation | `RendererCapabilities`, `RenderValidationResult`, `RenderDiagnostic` |
| Activity tracing | `PapercraftInstrumentation.ActivitySource` |
| Built-in controls | Text, paragraph, border, image, line, table, lists, charts, page numbers, columns, blocks and related layout controls |
| Template language | `for`, `forEach`, `if`, `switch`, `alternate`, `var` and variable expansion transformers |

This package references `Microsoft.Extensions.DependencyInjection.Abstractions` and intentionally does not reference SkiaSharp, hosting, OpenTelemetry or `X39.Solutions.PdfTemplate`.

## Register Core Services

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;

var services = new ServiceCollection();
var builder = services.AddPapercraftCore();
```

`AddPapercraftCore()` registers parser/runtime services, default controls, default transformers, `PapercraftGenerator`, and `PapercraftRenderer`.
It does not register a render backend, so rendering through `PapercraftRenderer` requires an `IPapercraftRenderBackend` registration.

## Implement A Renderer

Custom renderers implement `IPapercraftRenderBackend`:

```csharp
public sealed class MyRenderer : IPapercraftRenderBackend
{
    public RendererCapabilities Capabilities { get; } = new(
        "my-renderer",
        "My Renderer",
        RendererOutputKind.Pdf,
        new[] { PapercraftMediaTypes.ApplicationPdf });

    public ValueTask<RenderValidationResult> ValidateAsync(
        PapercraftDocument document,
        RenderTarget target,
        CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Capabilities.ValidateTarget(target));

    public ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}
```

Register the backend in the application service collection:

```csharp
services.AddPapercraftCore();
services.AddTransient<IPapercraftRenderBackend, MyRenderer>();
```

## Extend Template Behavior

`PapercraftServiceBuilder` can add, remove or replace controls and transformers:

```csharp
services.AddPapercraftCore()
        .AddFunction<MyFunction>()
        .AddControl<MyControl>()
        .ReplaceTransformer<MyOldTransformer, MyNewTransformer>();
```

The source currently exposes the builder through `AddPapercraftCore()` and through facade packages such as `X39.Solutions.Papercraft`.

## Related Projects

- [`X39.Solutions.Papercraft`](../X39.Solutions.Papercraft/README.md): default app-facing facade with SkiaSharp already registered.
- [`X39.Solutions.Papercraft.Rendering.SkiaSharp`](../X39.Solutions.Papercraft.Rendering.SkiaSharp/README.md): built-in PDF and raster backend.
- [`X39.Solutions.Papercraft.OpenTelemetry`](../X39.Solutions.Papercraft.OpenTelemetry/README.md): optional host/OpenTelemetry registration for Core's activity source.
- [`X39.Solutions.Papercraft.Controls.QrCode`](../X39.Solutions.Papercraft.Controls.QrCode/README.md) and [`X39.Solutions.Papercraft.Controls.ZXing`](../X39.Solutions.Papercraft.Controls.ZXing/README.md): optional control packages that depend on Core only.
