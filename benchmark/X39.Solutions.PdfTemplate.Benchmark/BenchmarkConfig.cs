using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal static class BenchmarkConfig
{
    public static IConfig Create()
    {
        return DefaultConfig.Instance
                            .AddDiagnoser(MemoryDiagnoser.Default)
                            .AddColumn(RankColumn.Arabic)
                            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared));
    }
}
