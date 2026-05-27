using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.Activation)]
public class ControlActivationBenchmarks
{
    private static readonly object[] NoArguments = Array.Empty<object>();

    private ServiceProvider _serviceProvider = null!;
    private ControlExpressionCache _expressionCache = null!;
    private ObjectFactory _noDependencyFactory = null!;
    private ObjectFactory _dependencyFactory = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _serviceProvider = BenchmarkServices.CreateServiceProvider();
        _expressionCache = _serviceProvider.GetRequiredService<ControlExpressionCache>();
        BenchmarkServices.WarmBenchmarkControlCache(_serviceProvider, _expressionCache);
        _noDependencyFactory = ActivatorUtilities.CreateFactory(typeof(NoDependencyControl), Type.EmptyTypes);
        _dependencyFactory = ActivatorUtilities.CreateFactory(typeof(ServiceDependencyControl), Type.EmptyTypes);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _serviceProvider.Dispose();
    }

    [Benchmark(Baseline = true)]
    public IControl ExpressionCache_NoDependencies()
    {
        return _expressionCache.CreateControl(
            _serviceProvider,
            typeof(NoDependencyControl),
            BenchmarkServices.EmptyParameters,
            null,
            BenchmarkServices.Culture);
    }

    [Benchmark]
    public IControl ActivatorUtilitiesCreateInstance_NoDependencies()
    {
        return (IControl) ActivatorUtilities.CreateInstance(_serviceProvider, typeof(NoDependencyControl));
    }

    [Benchmark]
    public IControl ActivatorUtilitiesCreateFactory_NoDependencies()
    {
        return (IControl) _noDependencyFactory(_serviceProvider, NoArguments);
    }

    [Benchmark]
    public IControl ExpressionCache_WithDependency()
    {
        return _expressionCache.CreateControl(
            _serviceProvider,
            typeof(ServiceDependencyControl),
            BenchmarkServices.EmptyParameters,
            null,
            BenchmarkServices.Culture);
    }

    [Benchmark]
    public IControl ActivatorUtilitiesCreateInstance_WithDependency()
    {
        return (IControl) ActivatorUtilities.CreateInstance(_serviceProvider, typeof(ServiceDependencyControl));
    }

    [Benchmark]
    public IControl ActivatorUtilitiesCreateFactory_WithDependency()
    {
        return (IControl) _dependencyFactory(_serviceProvider, NoArguments);
    }

    [Benchmark]
    public IControl ExpressionCache_FirstUse_NoDependencies()
    {
        using var cache = new ControlExpressionCache();
        return cache.CreateControl(
            _serviceProvider,
            typeof(NoDependencyControl),
            BenchmarkServices.EmptyParameters,
            null,
            BenchmarkServices.Culture);
    }

    [Benchmark]
    public IControl ExpressionCache_FirstUse_WithDependency()
    {
        using var cache = new ControlExpressionCache();
        return cache.CreateControl(
            _serviceProvider,
            typeof(ServiceDependencyControl),
            BenchmarkServices.EmptyParameters,
            null,
            BenchmarkServices.Culture);
    }
}
