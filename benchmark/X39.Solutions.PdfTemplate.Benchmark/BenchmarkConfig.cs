using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Order;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal static class BenchmarkConfig
{
    public static IConfig Create()
    {
        var config = DefaultConfig.Instance
                                  .AddDiagnoser(MemoryDiagnoser.Default)
                                  .AddColumn(RankColumn.Arabic)
                                  .AddExporter(CsvMeasurementsExporter.Default)
                                  .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared));

        if (IsRScriptAvailable())
            config = config.AddExporter(RPlotExporter.Default);

        return config;
    }

    private static bool IsRScriptAvailable()
    {
        var executableName = OperatingSystem.IsWindows() ? "Rscript.exe" : "Rscript";
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                   .Select((directory) => Path.Combine(directory, executableName))
                   .Any(File.Exists);
    }
}
