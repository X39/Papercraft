# X39.Solutions.Papercraft.Rendering.Svg

`X39.Solutions.Papercraft.Rendering.Svg` is a Papercraft render backend for SVG vector output.
It emits SVG XML directly from Papercraft display lists and does not add native rendering dependencies.

Use this package when you want `image/svg+xml` output from generated Papercraft documents.

## Package Role

| Area | Provided by this package |
|------|--------------------------|
| DI entry point | `services.AddPapercraftSvgRenderer()` |
| Backend implementation | `SvgRenderBackend` |
| Supported media types | `image/svg+xml` |

The backend declares support for vector image output, multipage documents, text drawing, images, clipping, transparency, fonts, color, absolute positioning and link annotations.

## Register The Renderer

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Rendering.Svg;

var services = new ServiceCollection();
services.AddPapercraftSvgRenderer();
```

`AddPapercraftSvgRenderer()` also registers Papercraft Core services.
After registration, resolve `PapercraftRenderer` from the service provider and render to `RenderTarget.FromMediaType("image/svg+xml")`.

## Output Notes

Multi-page documents are emitted as one SVG with page groups stacked vertically.
Images are embedded as data URIs when their encoded media type is recognized.
Fonts are referenced by family and style attributes rather than embedded.
The package includes a lightweight text measurement service for template generation; exact wrapping and metrics can differ from the SkiaSharp backend.

## Related Projects

- [`X39.Solutions.Papercraft.Core`](../X39.Solutions.Papercraft.Core/README.md): renderer-neutral contracts consumed by this backend.
- [`X39.Solutions.Papercraft`](../X39.Solutions.Papercraft/README.md): default facade that registers the SkiaSharp backend automatically.
