using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Services;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.Activation, BenchmarkCategories.Controls)]
public class DefaultControlActivationBenchmarks
{
    private ServiceProvider _serviceProvider = null!;
    private ControlActivationCache _controlActivationCache = null!;
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
        _controlActivationCache = _serviceProvider.GetRequiredService<ControlActivationCache>();
        BenchmarkServices.WarmDefaultControlCache(_serviceProvider, _controlActivationCache);
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
        return _controlActivationCache.CreateControl(
            _serviceProvider,
            _controlType,
            _parameters,
            _content,
            BenchmarkServices.Culture);
    }
}
