# X39.Solutions.Papercraft.Rendering.EscPos

`X39.Solutions.Papercraft.Rendering.EscPos` is a first-pass Papercraft render backend for ESC/POS printer command output.
It emits printer command bytes directly from Papercraft display lists and does not add SkiaSharp, PDF renderer, or printer transport dependencies.

Use this package when you want `application/vnd.papercraft.escpos` output from generated Papercraft documents and will handle printer transport elsewhere.

## Package Role

| Area | Provided by this package |
|------|--------------------------|
| DI entry point | `services.AddPapercraftEscPosRenderer()` |
| Backend implementation | `EscPosRenderBackend` |
| Backend id | `escpos` |
| Supported media types | `application/vnd.papercraft.escpos` |
| Output kind | `RendererOutputKind.PrinterCommands` |

## Register The Renderer

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Rendering.EscPos;

var services = new ServiceCollection();
services.AddPapercraftEscPosRenderer();
```

`AddPapercraftEscPosRenderer()` also registers Papercraft Core services.
Render with `EscPosRenderBackend.Target` or another explicit `RenderTarget` using `RendererOutputKind.PrinterCommands`.

## Output Notes

The backend emits ESC @ initialization by default, writes `DrawTextCommand` text using the configured encoding, appends LF after text, maps bold font weight and large text to common ESC/POS emphasis commands, and approximates horizontal lines as dashed text rules.

The default text encoding is Latin-1. Characters not representable by the configured encoding are handled by that encoding's fallback behavior.

The backend declares degraded support for text measurement, fonts, color, absolute positioning, and multipage output. It declares images, clipping, transparency, and link annotations unsupported. Filled rectangles, non-horizontal lines, and backend-specific drawing callbacks are reported by validation as unsupported.

This package only writes command bytes to `RenderOutput.Stream`. It does not open printer connections, spool jobs, discover devices, or perform PDF-to-printer conversion.

## Related Projects

- [`X39.Solutions.Papercraft.Core`](../X39.Solutions.Papercraft.Core/README.md): renderer-neutral contracts consumed by this backend.
- [`X39.Solutions.Papercraft.Rendering.Svg`](../X39.Solutions.Papercraft.Rendering.Svg/README.md): dependency-free SVG backend.
- [`Renderer backends`](../../docs/manual/render-backends.md): manual page for choosing and using renderer packages.
