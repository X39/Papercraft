using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.PdfTemplate.Benchmark;

[Control(BenchmarkControlNames.Namespace, "parameter")]
public sealed class ParameterHeavyControl : IControl
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public bool Enabled { get; set; }
    [Parameter] public int Count { get; set; }
    [Parameter] public float Ratio { get; set; }
    [Parameter] public EOrientation Orientation { get; set; }
    [Parameter] public Length Width { get; set; }
    [Parameter] public Thickness Padding { get; set; }
    [Parameter] public Color Color { get; set; }

    public int Checksum => Title.Length + Count + (Enabled ? 1 : 0) + (int) Ratio + (int) Orientation;

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