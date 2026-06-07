using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal sealed class BenchmarkYieldFormatFunction : IFunction
{
    public string Name => "benchmarkYieldFormat";
    public int Arguments => 1;
    public bool IsVariadic => false;

    public async ValueTask<object?> ExecuteAsync(
        CultureInfo cultureInfo,
        object?[] arguments,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        var value = Convert.ToInt32(arguments[0], cultureInfo);
        return $"Item {value:000}";
    }
}