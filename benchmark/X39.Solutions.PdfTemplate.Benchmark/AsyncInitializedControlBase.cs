using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Benchmark;

public abstract class AsyncInitializedControlBase : IControl, IInitializeControlAsync, IAsyncDisposable
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public int Count { get; set; }
    [Parameter] public float Ratio { get; set; }
    [Parameter] public Length Width { get; set; }
    [Parameter(IsContent = true)] public string Content { get; set; } = string.Empty;

    public int Checksum => Title.Length + Count + (int) Ratio + Content.Length;

    public abstract Task InitializeControlAsync(object? context, CancellationToken cancellationToken = default);

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Size Measure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => Size.Zero;

    public Size Arrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => Size.Zero;

    public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        => Size.Zero;
}