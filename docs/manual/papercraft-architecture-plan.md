# Papercraft Architecture Plan

Previous: [Migration to Papercraft](migration-to-papercraft.md) | [Manual home](index.md)

This page is the maintainer plan for the additive migration from
`X39.Solutions.PdfTemplate` to Papercraft. It describes the current package split and the
remaining release work needed before the Papercraft packages are the primary distribution path.

## Current Architecture Assessment

- The compatibility package project remains `source/X39.Solutions.PdfTemplate`. It preserves
  `AddPdfTemplateService()` and legacy type identities while referencing the Papercraft projects.
- `source/X39.Solutions.Papercraft.Core` now contains renderer-neutral contracts and the current shared
  runtime: XML parsing, template data, functions, transformers, control registration, built-in
  controls, deferred canvas/display-list primitives, render targets, diagnostics, capabilities,
  `PapercraftGenerator`, `PapercraftRenderer`, `PapercraftInstrumentation`, `PapercraftServiceBuilder`, `AddPapercraftCore()`
  and related options.
  It must stay free of a SkiaSharp package reference.
- `source/X39.Solutions.Papercraft.Rendering.SkiaSharp` owns the SkiaSharp-backed runtime: text
  measurement, paint cache, Skia conversions, `SkiaSharpRenderBackend`,
  `SkiaSharpDisplayListRenderer` and `AddPapercraftSkiaSharpRenderer()`.
- `source/X39.Solutions.Papercraft` is the default facade package project. It references core plus the
  SkiaSharp renderer, exposes `AddPapercraft()`, and forwards the application-facing Papercraft
  types from core.
- The compatibility bridge forwards Papercraft and legacy types where possible. Core also keeps
  the legacy XML control namespace alias so existing templates continue to activate built-in controls
  during the migration.
- The default renderer supports PDF, single-page PNG stream output and renderer-neutral
  page-by-page PNG raster output. Legacy bitmap workflows can still use the SkiaSharp
  `GenerateBitmapsAsync(...)` compatibility path.

## Target Architecture

| Package | Target responsibility |
|---------|-----------------------|
| `X39.Solutions.Papercraft.Core` | Renderer-neutral contracts, parsing, template data, layout/control abstractions, validation, diagnostics and display-list primitives. No SkiaSharp dependency. |
| `X39.Solutions.Papercraft.Rendering.SkiaSharp` | SkiaSharp renderer, PDF/raster output, immediate drawing, image decoding, text measurement, font resolution, paint/cache services and Skia compatibility adapters. |
| `X39.Solutions.Papercraft` | Batteries-included facade for normal application use. Depends on core and the default SkiaSharp renderer. |
| `X39.Solutions.Papercraft.OpenTelemetry` | Optional host/OpenTelemetry integration that registers Papercraft's core `ActivitySource` without adding hosting dependencies to Core. |
| `X39.Solutions.PdfTemplate` | Compatibility bridge for existing users. Keeps old service registration, namespaces, templates and common extension points working while forwarding to Papercraft packages. |

Core should define what a renderer can do; renderer packages should define how output is produced.
The facade should remain small and opinionated. The compatibility bridge should eventually contain
only wrappers, type-forwarders and compatibility documentation.

## Migration Strategy

- Keep the migration additive until the package split is published and covered by consumption tests.
- Preserve existing XML templates, control names, transformers, functions and data formats.
- Keep `AddPdfTemplateService()`, `Generator`, `DocumentOptions`, custom controls and existing
  builder methods working through the bridge.
- Use `AddPapercraft()` and `PapercraftRenderer` as the recommended new render entry point, without
  forcing applications to change in the same release.
- Move or rename legacy-namespace types only when type forwarding or wrappers keep existing source
  and binary consumers working.
- Keep APIs that expose `SKBitmap`, `SKCanvas`, `SKPaint` or Skia native asset behavior in the
  SkiaSharp renderer or compatibility bridge, not in core.
- Add obsolete warnings only after the replacement package path is real, documented and tested. Each
  warning must name the replacement member and package.

## API Design Proposal

Recommended application path:

```csharp
services.AddPapercraft();
var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
await renderer.GeneratePdfAsync(output, reader, CultureInfo.CurrentUICulture);
```

Renderer-neutral setup path:

```csharp
services.AddPapercraftCore();
services.AddPapercraftSkiaSharpRenderer();
```

Compatibility path:

```csharp
services.AddPdfTemplateService();
var generator = serviceProvider.GetRequiredService<Generator>();
await generator.GeneratePdfAsync(output, reader, CultureInfo.CurrentUICulture);
```

| Existing API | Papercraft API |
|--------------|----------------|
| `AddPdfTemplateService()` | `AddPapercraft()` |
| `PdfTemplateServiceBuilder` | `PapercraftServiceBuilder` |
| `Generator.GeneratePdfAsync(...)` | `PapercraftRenderer.GeneratePdfAsync(...)` |
| `DocumentOptions` | `PapercraftRenderOptions.DocumentOptions` |
| implicit default renderer | backend selection by `PapercraftRenderOptions.BackendId` or target capability |

`PapercraftGenerator` generates backend-neutral `PapercraftDocument` pages. `PapercraftRenderer`
is the facade for validation, backend selection and rendering. `RenderTarget`, `RenderOutput`,
`RasterPageRenderOutput`, `PapercraftRenderOptions`, `PapercraftDocument` and `PapercraftPage`
are the stable contracts between callers, the generator and backends.

Current core-owned renderer-neutral contract inventory:
`IPapercraftRenderBackend`, `PapercraftDocument`, `PapercraftPage`, `PapercraftRenderOptions`,
`PapercraftInstrumentation`, `PapercraftMediaTypes`, `RenderTarget`, `RenderOutput`, `RasterPageInfo`,
`RasterPageRenderOutput`, `RendererOutputKind`, `RendererSupportLevel`, `RendererCapabilities`,
`RendererFeatures`, `RenderFeatureUse`, `RenderDiagnosticCodes`, `RenderDiagnostic`,
`RenderValidationResult`, `RenderValidationException` and `TemplateLocation`.

## Backend Capability Design

- `RendererCapabilities` is the canonical renderer descriptor. It carries renderer id, display name,
  output kinds, media types, feature support levels and notes.
- `RendererFeatures` names capabilities such as PDF output, raster output, multipage output, text,
  images, clipping, transparency, fonts, color and absolute positioning.
- `RendererSupportLevel` distinguishes supported, degraded and unsupported features.
- `RenderFeatureUse` records renderer-relevant template feature use; `RenderDiagnostic` and
  `RenderValidationResult` report stable `PAPERCRAFT###` codes, feature ids, support level,
  message, backend limitation and optional template location.
- Unsupported diagnostics block rendering. Degraded diagnostics warn by default and block only when
  `PapercraftRenderOptions.TreatDegradedAsUnsupported` is enabled.
- Validation checks target/output support and declared renderer feature constraints before render
  work starts. The current feature scan covers well-known text, image, color, font, transparency and
  explicit clipping use, and can grow as new backend constraints are introduced.
- The SkiaSharp renderer advertises PDF and raster support. Single-stream PNG output is limited to
  one rendered page; multi-page raster output uses `RenderRasterPagesAsync(...)`.

## Testing Strategy

- Keep the existing generator, control, transformer and documentation sample tests as parity
  coverage for the compatibility bridge.
- Keep package consumption tests that prove the new projects can be referenced through the intended
  dependency graph.
- Assert that `X39.Solutions.Papercraft.Core` has no SkiaSharp reference and that Skia-specific APIs live in
  `X39.Solutions.Papercraft.Rendering.SkiaSharp` or the bridge.
- Assert that OpenTelemetry and hosting references stay in `X39.Solutions.Papercraft.OpenTelemetry`, not Core.
- Cover `AddPapercraftCore()`, `AddPapercraftSkiaSharpRenderer()`, `AddPapercraft()` and
  `AddPdfTemplateService()` registration paths.
- Test renderer selection, missing renderer ids, unsupported targets, degraded strict mode and
  diagnostic code stability.
- Keep public API compatibility checks before moving or forwarding more legacy types.
- Keep representative PDF and raster smoke/parity checks on supported OS/native asset
  combinations; add binary golden assets only when output stability is intentional.

## Documentation Strategy

- Keep README focused on install, package roles, minimal usage and compatibility status.
- Keep the manual focused on template authors; put service setup and extension points in the
  developer appendix.
- Keep the migration page concise and user-facing: what changed, what still works and which API to
  prefer for new code.
- Keep this architecture page as maintainer-facing implementation guidance. Update it whenever type
  ownership changes between core, renderer, facade and bridge projects.
- Before publishing the split, verify README, migration page, developer appendix, package metadata
  and navigation all describe the same package roles.

## Risk Assessment

- Legacy namespace debt can obscure ownership. Moving files is not enough if public type identities
  still need forwarding.
- Existing custom controls may depend on old namespaces or Skia-specific drawing helpers.
- `GenerateBitmapsAsync(...)` remains SkiaSharp-specific and should stay on the compatibility path
  while new code uses renderer-neutral raster page output.
- Text measurement, font fallback and image decoding can change output when renderer internals move.
- DI registration can drift between core-only, facade, renderer and compatibility entry points.
- Type-forwarding mistakes can break binary consumers even when source builds still pass.
- SkiaSharp native assets remain a deployment risk for Linux, Windows and macOS consumers.

## Recommended Phase Breakdown

| Phase | Status | Acceptance criteria |
|-------|--------|---------------------|
| 1. Compatibility facade | Verified locally | `AddPdfTemplateService()` and `AddPapercraft()` both work, existing templates do not change, and docs show the additive migration path. |
| 2. Core/runtime split | Verified locally | Core owns contracts plus shared parsing/runtime services and builds without a SkiaSharp package dependency. |
| 3. Skia renderer extraction | Verified locally | Skia canvas, text, image, paint, bitmap and renderer registration live in `X39.Solutions.Papercraft.Rendering.SkiaSharp`. |
| 4. Facade/package consumption | Verified locally | `X39.Solutions.Papercraft` consumes core plus SkiaSharp renderer, exposes the default setup path and passes package-consumption tests. |
| 5. Diagnostics hardening | Verified locally | Validation reports stable diagnostics for unsupported and degraded targets/features before render output begins. |
| 6. Compatibility bridge cleanup | In progress | `X39.Solutions.PdfTemplate` mostly acts as old entry points, wrappers, type-forwarders and package metadata; legacy namespace debt remains intentionally additive. |
| 7. Migration release | In progress | Package readmes and manual pages agree, local packages pack, package-consumption tests pass and PDF/PNG/raster-page parity smoke checks pass; publishing and final obsolete guidance remain release work. |

Previous: [Migration to Papercraft](migration-to-papercraft.md) | [Manual home](index.md)
