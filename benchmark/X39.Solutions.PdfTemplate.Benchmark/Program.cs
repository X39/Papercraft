using BenchmarkDotNet.Running;

namespace X39.Solutions.PdfTemplate.Benchmark;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, BenchmarkConfig.Create());
    }
}
