# X39.Solutions.PdfTemplate.Benchmark

`X39.Solutions.PdfTemplate.Benchmark` is the BenchmarkDotNet project for Papercraft and compatibility-path performance work.
It measures control activation, parameter binding, template parsing, transformer overhead, template creation and full document generation.
Normal BenchmarkDotNet output compares benchmark methods, but full generation benchmarks intentionally remain aggregate method timings. Use activity profile mode to see phase-level time inside a render.

This project is not packable.

## Benchmark Areas

| Area | Representative benchmark classes |
|------|----------------------------------|
| Control activation | `ControlActivationBenchmarks`, `DefaultControlActivationBenchmarks` |
| Parameter binding | `ParameterBindingBenchmarks` |
| Template creation | `TemplateCreationBenchmarks`, `DefaultControlTemplateCreationBenchmarks`, `CreationSurfaceBenchmarks` |
| Parsing and transformers | `ParsingBenchmarks`, `TransformerDirectBenchmarks`, `TransformerParsingBenchmarks`, `TransformerGenerationBenchmarks` |
| Full generation | `GenerationBenchmarks`, `ControlGenerationBenchmarks` |

Benchmark categories are defined in `BenchmarkCategories.cs`:

- `Activation`
- `Controls`
- `Transformers`
- `Parsing`
- `TemplateCreation`
- `Generation`

## Run Benchmarks

Run the full benchmark suite from the repository root:

```shell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj
```

List benchmarks:

```shell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --list tree
```

Run a short activation smoke test:

```shell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --anyCategories Activation --job Short --warmupCount 1 --iterationCount 1
```

BenchmarkDotNet writes reports under `BenchmarkDotNet.Artifacts/`.

Run a one-iteration renderer activity profile:

```shell
dotnet run -c Release --project benchmark/X39.Solutions.PdfTemplate.Benchmark/X39.Solutions.PdfTemplate.Benchmark.csproj -- --activityProfile --iterations 1
```

Activity profile mode attaches an `ActivityListener`, runs warmed render iterations, and writes aggregate phase timings to `BenchmarkDotNet.Artifacts/activity-profile/`.
It supports `--case`, `--target pdf|png`, `--backendId`, and `--iterations`; defaults are `RepresentativeInvoice`, `png`, `skiasharp`, and `5`.
Because it includes tracing overhead, use it for attribution rather than BenchmarkDotNet baselines.

## Configuration

`BenchmarkConfig.Create()` adds:

- memory diagnostics
- rank column
- CSV measurements exporter
- declared ordering for summaries and methods
- R plot exporter when `Rscript` is available on `PATH`

The benchmark project references SkiaSharp native assets for Windows and Linux because generation benchmarks execute renderer code.
The activity profile path is intentionally outside BenchmarkDotNet configuration so tracing overhead does not affect normal benchmark reports.

## Related Documentation

- [`../README.md`](../README.md): folder-level benchmark guide with report workflow details and caveats.
- [`../../source/X39.Solutions.Papercraft.Core/README.md`](../../source/X39.Solutions.Papercraft.Core/README.md): core runtime under measurement.
- [`../../source/X39.Solutions.PdfTemplate/README.md`](../../source/X39.Solutions.PdfTemplate/README.md): compatibility package measured by benchmark entry points.
