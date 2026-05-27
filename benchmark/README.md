# X39.Solutions.PdfTemplate benchmarks

This folder contains BenchmarkDotNet benchmarks focused on generator performance, especially control activation and parameter binding.

## Run

Full run:

```powershell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj
```

Short smoke run for activation benchmarks:

```powershell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --filter '*ControlActivationBenchmarks*' --job Short --warmupCount 1 --iterationCount 1
```

List available benchmarks:

```powershell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --list tree
```

BenchmarkDotNet writes reports under `BenchmarkDotNet.Artifacts/`, which is ignored by git.

## Benchmark groups

- `ControlActivationBenchmarks` compares the current `ControlExpressionCache.CreateControl` path with `ActivatorUtilities.CreateInstance` and cached `ActivatorUtilities.CreateFactory` prototypes. It covers no-dependency and constructor-injected controls, plus cold first-use expression-cache cases.
- `ParameterBindingBenchmarks` measures repeated control creation with light `[Parameter]` dictionaries, heavier conversion dictionaries, and `[Parameter(IsContent = true)]` content binding.
- `TemplateCreationBenchmarks` measures direct `Template.CreateAsync` from already parsed `XmlNodeInformation`, including simple controls, nested content controls, and a medium repeated-control template.
- `ParsingBenchmarks` measures XML parsing and template transformation for small, medium, and large deterministic templates.
- `GenerationBenchmarks` measures full `GenerateBitmapsAsync` for a representative invoice/table document and disposes all returned bitmaps inside the measured method.

## Caveats

- Run in `Release`; Debug results are not useful.
- Activation prototype benchmarks use benchmark-local controls. Real controls may have different constructor and parameter profiles.
- `ControlExpressionCache.CreateControl` always includes the empty parameter-binding pass, even in activation-only comparisons.
- `GenerateBitmapsAsync` includes rendering work and bitmap allocation, so compare it separately from parse/create microbenchmarks.
- Service provider construction is intentionally done in setup and is not part of the measured methods.
