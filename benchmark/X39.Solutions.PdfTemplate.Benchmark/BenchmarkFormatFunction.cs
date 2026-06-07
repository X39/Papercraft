using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal sealed class BenchmarkFormatFunction : IFunction
{
    public string Name => "benchmarkFormat";
    public int Arguments => 1;
    public bool IsVariadic => false;

    public ValueTask<object?> ExecuteAsync(
        CultureInfo cultureInfo,
        object?[] arguments,
        CancellationToken cancellationToken = default)
    {
        var value = Convert.ToInt32(arguments[0], cultureInfo);
        return ValueTask.FromResult<object?>($"Item {value:000}");
    }
}