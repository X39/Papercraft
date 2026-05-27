using System.Collections;
using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Benchmark;

[Control(BenchmarkControlNames.Namespace, "container")]
public sealed class BenchmarkContainerControl : IContentControl
{
    private readonly List<IControl> _children = new();

    public int Count => _children.Count;
    public bool IsReadOnly => false;

    public IEnumerator<IControl> GetEnumerator() => _children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(IControl item) => _children.Add(item);

    public void Clear() => _children.Clear();

    public bool Contains(IControl item) => _children.Contains(item);

    public void CopyTo(IControl[] array, int arrayIndex) => _children.CopyTo(array, arrayIndex);

    public bool Remove(IControl item) => _children.Remove(item);

    public bool CanAdd(Type type) => type.IsAssignableTo(typeof(IControl));

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