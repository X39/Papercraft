# X39.Solutions.PdfTemplate.Test

`X39.Solutions.PdfTemplate.Test` is the xUnit test project for the Papercraft split and the legacy `X39.Solutions.PdfTemplate` compatibility bridge.
It verifies package boundaries, parser behavior, template transformations, controls, rendering output, documentation samples and package-consumption scenarios.

This project is not packable.

## Test Coverage

| Area | Files |
|------|-------|
| Package split and type ownership | `PapercraftCoreContractTests.cs`, `PapercraftPackageConsumptionTests.cs` |
| Compatibility facade | `PapercraftTests.cs`, `ServiceCollectionExtensionsTests.cs`, `PapercraftOutputParityTests.cs` |
| Rendering targets and validation | `PapercraftRenderTargetTests.cs`, `PapercraftPackageScaffoldTests.cs` |
| Template language | `ExpressionTests/*`, `Xml/*` |
| Built-in controls | `Controls/*` |
| Parsing and value conversion | `Parsing/*`, `Data/*` |
| Documentation samples | `Samples/Documentation/*` |

The package-consumption tests create temporary consumer projects, build them with `dotnet build`, and inspect generated NuGet asset files to verify dependency boundaries.

## Run Tests

From the repository root:

```shell
dotnet test test/X39.Solutions.PdfTemplate.Test/X39.Solutions.PdfTemplate.Test.csproj
```

Run a focused package-boundary check:

```shell
dotnet test test/X39.Solutions.PdfTemplate.Test/X39.Solutions.PdfTemplate.Test.csproj --filter FullyQualifiedName~PapercraftPackageConsumptionTests
```

Run a focused control test:

```shell
dotnet test test/X39.Solutions.PdfTemplate.Test/X39.Solutions.PdfTemplate.Test.csproj --filter FullyQualifiedName~QrCodeControlTests
```

## Generated Documentation Assets

Tests under `Samples/Documentation` render XML snippets into preview assets under an ignored test-output folder by default:

```text
test/X39.Solutions.PdfTemplate.Test/TestResults/documentation-samples
```

To regenerate the checked-in documentation assets, opt in explicitly:

```powershell
./scripts/Render-DocumentationSamples.ps1
```

The opt-in path intentionally overwrites stale PNG, SVG and PDF assets under `docs/assets/samples` for the sample being rendered.
Review asset diffs when changing layout, controls, renderer output or sample XML.

## Runtime Notes

The test project references `SkiaSharp.NativeAssets.Linux` because renderer tests execute SkiaSharp-backed output.
Tests also use bundled fonts and images from:

- `test/fonts/Nunito_Sans`
- `test/images`

## Related Projects

- [`../../source/X39.Solutions.Papercraft.Core/README.md`](../../source/X39.Solutions.Papercraft.Core/README.md): contracts and runtime under test.
- [`../../source/X39.Solutions.Papercraft/README.md`](../../source/X39.Solutions.Papercraft/README.md): default facade under test.
- [`../../source/X39.Solutions.PdfTemplate/README.md`](../../source/X39.Solutions.PdfTemplate/README.md): compatibility bridge under test.
