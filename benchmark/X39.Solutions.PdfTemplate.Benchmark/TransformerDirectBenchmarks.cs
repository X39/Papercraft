using BenchmarkDotNet.Attributes;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Xml;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.Transformers)]
public class TransformerDirectBenchmarks
{
    private IReadOnlyCollection<XmlNode> _nodes = null!;
    private TemplateData _forEachData = null!;
    private TemplateData _ifData = null!;
    private TemplateData _alternateData = null!;
    private TemplateData _variableData = null!;

    private global::X39.Solutions.Papercraft.Transformers.ForTransformer       _forTransformer       = null!;
    private global::X39.Solutions.Papercraft.Transformers.ForEachTransformer   _forEachTransformer   = null!;
    private global::X39.Solutions.Papercraft.Transformers.IfTransformer        _ifTransformer        = null!;
    private global::X39.Solutions.Papercraft.Transformers.AlternateTransformer _alternateTransformer = null!;
    private global::X39.Solutions.Papercraft.Transformers.VariableTransformer  _variableTransformer  = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _nodes = CreateNodes();
        _forEachData = new TemplateData();
        _forEachData.SetVariable("items", Enumerable.Range(0, (int) BenchmarkTemplateSize.Medium).ToArray());
        _ifData = new TemplateData();
        _ifData.SetVariable("flag", true);
        _ifData.SetVariable("count", 42);
        _alternateData = new TemplateData();
        _variableData = new TemplateData();

        _forTransformer       = new global::X39.Solutions.Papercraft.Transformers.ForTransformer();
        _forEachTransformer   = new global::X39.Solutions.Papercraft.Transformers.ForEachTransformer();
        _ifTransformer        = new global::X39.Solutions.Papercraft.Transformers.IfTransformer();
        _alternateTransformer = new global::X39.Solutions.Papercraft.Transformers.AlternateTransformer();
        _variableTransformer  = new global::X39.Solutions.Papercraft.Transformers.VariableTransformer();

        ConsumeAsync(
                _alternateTransformer.TransformAsync(
                    BenchmarkServices.Culture,
                    _alternateData,
                    """on color with ["#FFFFFF", "#F5F7FA"]""",
                    _nodes))
            .GetAwaiter()
            .GetResult();
    }

    [Benchmark(Baseline = true)]
    public Task<int> For_Range()
    {
        return ConsumeAsync(
            _forTransformer.TransformAsync(
                BenchmarkServices.Culture,
                new TemplateData(),
                $"i from 0 to {(int) BenchmarkTemplateSize.Medium}",
                _nodes));
    }

    [Benchmark]
    public Task<int> ForEach_WithoutIndex()
    {
        return ConsumeAsync(
            _forEachTransformer.TransformAsync(
                BenchmarkServices.Culture,
                _forEachData,
                "item in items",
                _nodes));
    }

    [Benchmark]
    public Task<int> ForEach_WithIndex()
    {
        return ConsumeAsync(
            _forEachTransformer.TransformAsync(
                BenchmarkServices.Culture,
                _forEachData,
                "item in items with index",
                _nodes));
    }

    [Benchmark]
    public Task<int> If_BooleanTrue()
    {
        return ConsumeAsync(
            _ifTransformer.TransformAsync(
                BenchmarkServices.Culture,
                _ifData,
                "flag",
                _nodes));
    }

    [Benchmark]
    public Task<int> If_BooleanFalse()
    {
        return ConsumeAsync(
            _ifTransformer.TransformAsync(
                BenchmarkServices.Culture,
                new TemplateData(),
                "false",
                _nodes));
    }

    [Benchmark]
    public Task<int> If_ComparisonTrue()
    {
        return ConsumeAsync(
            _ifTransformer.TransformAsync(
                BenchmarkServices.Culture,
                _ifData,
                "count >= 40",
                _nodes));
    }

    [Benchmark]
    public Task<int> Alternate_Initial()
    {
        return ConsumeAsync(
            _alternateTransformer.TransformAsync(
                BenchmarkServices.Culture,
                _alternateData,
                """on color with ["#FFFFFF", "#F5F7FA"]""",
                _nodes));
    }

    [Benchmark]
    public Task<int> Alternate_Repeat()
    {
        return ConsumeAsync(
            _alternateTransformer.TransformAsync(
                BenchmarkServices.Culture,
                _alternateData,
                "repeat on color",
                _nodes));
    }

    [Benchmark]
    public Task<int> Var_SingleAssignment()
    {
        return ConsumeAsync(
            _variableTransformer.TransformAsync(
                BenchmarkServices.Culture,
                _variableData,
                "label = \"Benchmark\"",
                _nodes));
    }

    [Benchmark]
    public Task<int> Var_MultipleAssignments()
    {
        return ConsumeAsync(
            _variableTransformer.TransformAsync(
                BenchmarkServices.Culture,
                _variableData,
                "label = \"Benchmark\", amount = 42, enabled = true",
                _nodes));
    }

    private static IReadOnlyCollection<XmlNode> CreateNodes()
    {
        var first = new XmlNode(1, 1, Constants.ControlsNamespace, "text");
        first.AddChild(new XmlNode(1, 1, "Transformer direct benchmark"));
        var second = new XmlNode(1, 1, Constants.ControlsNamespace, "line");
        second.SetAttribute("thickness", "1px");
        second.SetAttribute("length", "100%");
        second.SetAttribute("color", "#336699");
        return new[]
        {
            first,
            second,
        };
    }

    private static async Task<int> ConsumeAsync(IAsyncEnumerable<XmlNode> nodes)
    {
        var count = 0;
        await foreach (var _ in nodes.ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }
}
