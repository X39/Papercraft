using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.Activation, BenchmarkCategories.Controls)]
public class DefaultControlActivationBenchmarks
{
    private ServiceProvider _serviceProvider = null!;
    private ControlExpressionCache _expressionCache = null!;
    private Type _controlType = null!;
    private IReadOnlyDictionary<string, string> _parameters = null!;
    private string? _content;

    [ParamsSource(nameof(ControlNames))]
    public string Control { get; set; } = "text";

    public IEnumerable<string> ControlNames => BenchmarkServices.BuiltInControlNames;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _serviceProvider = BenchmarkServices.CreateDefaultServiceProvider();
        _expressionCache = _serviceProvider.GetRequiredService<ControlExpressionCache>();
        BenchmarkServices.WarmDefaultControlCache(_serviceProvider, _expressionCache);
        _controlType = BenchmarkServices.GetBuiltInControlType(Control);
        _parameters = BenchmarkServices.GetBuiltInControlParameters(Control);
        _content = BenchmarkServices.GetBuiltInControlContent(Control);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _serviceProvider.Dispose();
    }

    [Benchmark]
    public IControl CreateBuiltInControl()
    {
        return _expressionCache.CreateControl(
            _serviceProvider,
            _controlType,
            _parameters,
            _content,
            BenchmarkServices.Culture);
    }
}
