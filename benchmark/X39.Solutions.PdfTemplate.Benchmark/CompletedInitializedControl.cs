using X39.Solutions.PdfTemplate.Attributes;

namespace X39.Solutions.PdfTemplate.Benchmark;

[Control(BenchmarkControlNames.Namespace, "completedInit")]
public sealed class CompletedInitializedControl : AsyncInitializedControlBase
{
    public override Task InitializeControlAsync(object? context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}