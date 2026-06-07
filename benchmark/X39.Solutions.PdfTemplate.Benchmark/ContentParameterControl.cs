using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.PdfTemplate.Benchmark;

[Control(BenchmarkControlNames.Namespace, "content")]
public sealed class ContentParameterControl : IControl
{
    [Parameter(IsContent = true)]
    public string Text { get; set; } = string.Empty;

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