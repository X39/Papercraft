using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal static class BenchmarkServices
{
    public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    public static readonly IReadOnlyDictionary<string, string> EmptyParameters = new Dictionary<string, string>();

    public static readonly IReadOnlyDictionary<string, string> LightParameters = new Dictionary<string, string>
    {
        ["TITLE"] = "Benchmark",
        ["COUNT"] = "42",
    };

    public static readonly IReadOnlyDictionary<string, string> HeavyParameters = new Dictionary<string, string>
    {
        ["TITLE"]       = "Benchmark",
        ["ENABLED"]     = "true",
        ["COUNT"]       = "42",
        ["RATIO"]       = "1.5",
        ["ORIENTATION"] = "Horizontal",
        ["WIDTH"]       = "12px",
        ["PADDING"]     = "1px 2px 3px 4px",
        ["COLOR"]       = "#336699",
    };

    public const string ContentText = "Deterministic content parameter text.";

    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService(
            (builder) => builder
                .AddControl<NoDependencyControl>()
                .AddControl<ServiceDependencyControl>()
                .AddControl<ParameterHeavyControl>()
                .AddControl<ContentParameterControl>()
                .AddControl<BenchmarkContainerControl>());
        services.AddSingleton<IBenchmarkDependency, BenchmarkDependency>();
        return services.BuildServiceProvider();
    }

    public static ServiceProvider CreateDefaultServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService();
        return services.BuildServiceProvider();
    }

    public static void WarmBenchmarkControlCache(IServiceProvider serviceProvider, ControlExpressionCache cache)
    {
        cache.CreateControl(serviceProvider, typeof(NoDependencyControl), EmptyParameters, null, Culture);
        cache.CreateControl(serviceProvider, typeof(ServiceDependencyControl), EmptyParameters, null, Culture);
        cache.CreateControl(serviceProvider, typeof(ParameterHeavyControl), HeavyParameters, null, Culture);
        cache.CreateControl(serviceProvider, typeof(ContentParameterControl), EmptyParameters, ContentText, Culture);
        cache.CreateControl(serviceProvider, typeof(BenchmarkContainerControl), EmptyParameters, null, Culture);
    }

    public static Generator CreateDefaultGenerator(ServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<Generator>();
    }
}
