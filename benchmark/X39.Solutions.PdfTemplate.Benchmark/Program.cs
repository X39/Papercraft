using BenchmarkDotNet.Running;

namespace X39.Solutions.PdfTemplate.Benchmark;

public static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Any((q) => string.Equals(q, "--activityProfile", StringComparison.OrdinalIgnoreCase)))
        {
            await ActivityProfileRunner.RunAsync(args)
                .ConfigureAwait(false);
            return;
        }

        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, BenchmarkConfig.Create());
    }
}
