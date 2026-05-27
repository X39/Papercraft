using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.TemplateCreation, BenchmarkCategories.Controls)]
public class DefaultControlTemplateCreationBenchmarks
{
    private ServiceProvider _serviceProvider = null!;
    private IControlFactory _controlFactory = null!;
    private XmlNodeInformation _template = null!;

    [ParamsSource(nameof(ControlNames))]
    public string Control { get; set; } = "text";

    public IEnumerable<string> ControlNames => BenchmarkServices.BuiltInControlNames;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _serviceProvider = BenchmarkServices.CreateDefaultServiceProvider();
        _controlFactory = _serviceProvider.GetRequiredService<IControlFactory>();
        BenchmarkServices.WarmDefaultControlCache(
            _serviceProvider,
            _serviceProvider.GetRequiredService<ControlActivationCache>());
        _template = Parse(BenchmarkTemplates.GetBuiltInControlTemplate(Control));
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _serviceProvider.Dispose();
    }

    [Benchmark]
    public async Task<int> CreateBuiltInControlTemplate()
    {
        await using var template = await Template.CreateAsync(
                _template,
                _controlFactory,
                BenchmarkServices.Culture,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        return BenchmarkUtilities.CountControls(template.BodyControls);
    }

    private static XmlNodeInformation Parse(byte[] bytes)
    {
        using var reader = BenchmarkTemplates.CreateXmlReader(bytes);
        using var templateReader = new XmlTemplateReader(
            DocumentOptions.Default,
            BenchmarkServices.Culture,
            new TemplateData(),
            Array.Empty<ITransformer>());
        return templateReader.ReadAsync(reader).GetAwaiter().GetResult();
    }
}
