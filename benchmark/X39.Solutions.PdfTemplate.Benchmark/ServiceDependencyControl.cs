using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.PdfTemplate.Benchmark;

[Control(BenchmarkControlNames.Namespace, "service")]
public sealed class ServiceDependencyControl : IControl
{
    private readonly IBenchmarkDependency _dependency;

    [ControlConstructor]
    public ServiceDependencyControl(IBenchmarkDependency dependency)
    {
        _dependency = dependency;
    }

    public int DependencyValue => _dependency.Value;

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