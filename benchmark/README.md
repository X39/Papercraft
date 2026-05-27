# X39.Solutions.PdfTemplate benchmarks

This folder contains BenchmarkDotNet benchmarks focused on generator performance, especially control activation and parameter binding.

## Run

Full run:

```powershell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj
```

Short smoke run for activation benchmarks:

```powershell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --anyCategories Activation --job Short --warmupCount 1 --iterationCount 1
```

List available benchmarks:

```powershell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --list tree
```

BenchmarkDotNet writes reports under `BenchmarkDotNet.Artifacts/`, which is ignored by git.

Category smoke runs:

```powershell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --anyCategories Controls --job Dry
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --anyCategories Transformers --job Dry
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --anyCategories Generation --job Dry
```

## Reports and graphs

BenchmarkDotNet writes HTML, Markdown, CSV, and raw measurement reports to `BenchmarkDotNet.Artifacts/results/`.
This benchmark project also enables BenchmarkDotNet's R plot exporter when `Rscript` is available on `PATH`.
Install R locally before running the benchmarks if you want PNG plot files in the same artifact folder.

The repository includes a manually triggered GitHub Actions workflow for report generation:

1. Open the repository's Actions tab.
2. Select `Benchmark reports`.
3. Choose a category preset, optional filter, and job size.
4. Run the workflow and download the `benchmarkdotnet-artifacts-*` artifact when it completes.

The workflow installs R, runs the benchmarks in `Release`, and uploads the generated `BenchmarkDotNet.Artifacts` folder.
The default workflow inputs use the `Short` BenchmarkDotNet job so the run is practical in CI; choose a category preset for targeted reports or `Default` and empty count overrides for a fuller run.

## Benchmark groups

- `ControlActivationBenchmarks` compares the current `ControlActivationCache.CreateControl` path with `ActivatorUtilities.CreateInstance` and cached `ActivatorUtilities.CreateFactory` prototypes. It covers no-dependency and constructor-injected controls, plus cold first-use cache cases.
- `ParameterBindingBenchmarks` measures repeated control creation with light `[Parameter]` dictionaries, heavier conversion dictionaries, and `[Parameter(IsContent = true)]` content binding.
- `DefaultControlActivationBenchmarks` creates each default registered control through `ControlActivationCache.CreateControl` with representative parameters.
- `DefaultControlTemplateCreationBenchmarks` creates templates for each default control, using valid parent containers for child-only controls such as `data`, `td`, `th`, and `tr`.
- `ControlGenerationBenchmarks` runs full `GenerateBitmapsAsync` for representative text, line, border, image, page-number, table, and chart documents.
- `TransformerDirectBenchmarks` calls each default transformer directly and consumes the resulting node stream.
- `TransformerParsingBenchmarks` compares transformed templates with equivalent hand-expanded templates for `for`, `forEach`, `if`, `alternate`, and `var` at small, medium, and large sizes.
- `TransformerGenerationBenchmarks` compares a transformer-heavy generated document with an equivalent expanded document.
- `TemplateCreationBenchmarks` measures direct `Template.CreateAsync` from already parsed `XmlNodeInformation`, including simple controls, nested content controls, and a medium repeated-control template.
- `CreationSurfaceBenchmarks` measures a more realistic creation surface: XML parse plus `Template.CreateAsync`, repeated parameter-heavy controls, controls with completed or yielding `IInitializeControlAsync` hooks, and transformer/function-heavy templates before control creation.
- `ParsingBenchmarks` measures XML parsing and template transformation for small, medium, and large deterministic templates.
- `GenerationBenchmarks` measures full `GenerateBitmapsAsync` for a representative invoice/table document and disposes all returned bitmaps inside the measured method.

## Caveats

- Run in `Release`; Debug results are not useful.
- Activation prototype benchmarks use benchmark-local controls. Real controls may have different constructor and parameter profiles.
- `ControlActivationCache.CreateControl` returns directly after activation when no parameters or content are supplied.
- Per-control activation/template benchmarks use built-in control types directly; full rendering uses valid parent/child compositions for controls that cannot render standalone.
- Transformer parsing baselines use equivalent hand-expanded XML so the comparison includes parser and transformation overhead.
- `GenerateBitmapsAsync` includes rendering work and bitmap allocation, so compare it separately from parse/create microbenchmarks.
- Service provider construction is intentionally done in setup and is not part of the measured methods.
