using X39.Solutions.PdfTemplate.Attributes;

namespace X39.Solutions.PdfTemplate.Benchmark;

[Control(BenchmarkControlNames.Namespace, "yieldInit")]
public sealed class YieldInitializedControl : AsyncInitializedControlBase
{
    public override async Task InitializeControlAsync(object? context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
    }

    public override async ValueTask DisposeAsync()
    {
        await Task.Yield();
    }
}