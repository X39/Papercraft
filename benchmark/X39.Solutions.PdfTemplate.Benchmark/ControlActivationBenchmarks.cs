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
    private ControlActivationCache _controlActivationCache = null!;
    private ObjectFactory _noDependencyFactory = null!;
    private ObjectFactory _dependencyFactory = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _serviceProvider = BenchmarkServices.CreateServiceProvider();
        _controlActivationCache = _serviceProvider.GetRequiredService<ControlActivationCache>();
        BenchmarkServices.WarmBenchmarkControlCache(_serviceProvider, _controlActivationCache);
        _noDependencyFactory = ActivatorUtilities.CreateFactory(typeof(NoDependencyControl), Type.EmptyTypes);
        _dependencyFactory = ActivatorUtilities.CreateFactory(typeof(ServiceDependencyControl), Type.EmptyTypes);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _serviceProvider.Dispose();
    }

    [Benchmark(Baseline = true)]
    public IControl CachedControlFactory_NoDependencies()
    {
        return _controlActivationCache.CreateControl(
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
    public IControl CachedControlFactory_WithDependency()
    {
        return _controlActivationCache.CreateControl(
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
    public IControl CachedControlFactory_FirstUse_NoDependencies()
    {
        var cache = new ControlActivationCache();
        return cache.CreateControl(
            _serviceProvider,
            typeof(NoDependencyControl),
            BenchmarkServices.EmptyParameters,
            null,
            BenchmarkServices.Culture);
    }

    [Benchmark]
    public IControl CachedControlFactory_FirstUse_WithDependency()
    {
        var cache = new ControlActivationCache();
        return cache.CreateControl(
            _serviceProvider,
            typeof(ServiceDependencyControl),
            BenchmarkServices.EmptyParameters,
            null,
            BenchmarkServices.Culture);
    }
}
