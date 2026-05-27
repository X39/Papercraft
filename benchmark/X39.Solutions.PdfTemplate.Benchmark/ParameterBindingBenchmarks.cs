using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.Activation, BenchmarkCategories.Controls)]
public class ParameterBindingBenchmarks
{
    private ServiceProvider _serviceProvider = null!;
    private ControlExpressionCache _expressionCache = null!;

    [Params(1, 50, 250)]
    public int ControlCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _serviceProvider = BenchmarkServices.CreateServiceProvider();
        _expressionCache = _serviceProvider.GetRequiredService<ControlExpressionCache>();
        BenchmarkServices.WarmBenchmarkControlCache(_serviceProvider, _expressionCache);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _serviceProvider.Dispose();
    }

    [Benchmark(Baseline = true)]
    public int LightParameters()
    {
        var checksum = 0;
        for (var i = 0; i < ControlCount; i++)
        {
            var control = (ParameterHeavyControl) _expressionCache.CreateControl(
                _serviceProvider,
                typeof(ParameterHeavyControl),
                BenchmarkServices.LightParameters,
                null,
                BenchmarkServices.Culture);
            checksum += control.Checksum;
        }

        return checksum;
    }

    [Benchmark]
    public int HeavyParameters()
    {
        var checksum = 0;
        for (var i = 0; i < ControlCount; i++)
        {
            var control = (ParameterHeavyControl) _expressionCache.CreateControl(
                _serviceProvider,
                typeof(ParameterHeavyControl),
                BenchmarkServices.HeavyParameters,
                null,
                BenchmarkServices.Culture);
            checksum += control.Checksum;
        }

        return checksum;
    }

    [Benchmark]
    public int ContentParameter()
    {
        var checksum = 0;
        for (var i = 0; i < ControlCount; i++)
        {
            var control = (ContentParameterControl) _expressionCache.CreateControl(
                _serviceProvider,
                typeof(ContentParameterControl),
                BenchmarkServices.EmptyParameters,
                BenchmarkServices.ContentText,
                BenchmarkServices.Culture);
            checksum += control.Text.Length;
        }

        return checksum;
    }
}
