using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Services;
using X39.Solutions.Papercraft.Xml;
using X39.Solutions.PdfTemplate;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.TemplateCreation)]
public class TemplateCreationBenchmarks
{
    private ServiceProvider _serviceProvider = null!;
    private IControlFactory _controlFactory = null!;
    private XmlNodeInformation _simpleTemplate = null!;
    private XmlNodeInformation _nestedTemplate = null!;
    private XmlNodeInformation _mediumTemplate = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _serviceProvider = BenchmarkServices.CreateServiceProvider();
        _controlFactory = _serviceProvider.GetRequiredService<IControlFactory>();
        BenchmarkServices.WarmBenchmarkControlCache(
            _serviceProvider,
            _serviceProvider.GetRequiredService<ControlActivationCache>());
        _simpleTemplate = Parse(BenchmarkTemplates.SimpleTemplateCreationTemplate);
        _nestedTemplate = Parse(BenchmarkTemplates.NestedTemplateCreationTemplate);
        _mediumTemplate = Parse(BenchmarkTemplates.MediumTemplateCreationTemplate);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _serviceProvider.Dispose();
    }

    [Benchmark(Baseline = true)]
    public async Task<int> CreateSimpleTemplate()
    {
        await using var template = await Template.CreateAsync(
                _simpleTemplate,
                _controlFactory,
                BenchmarkServices.Culture,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        return CountControls(template.BodyControls);
    }

    [Benchmark]
    public async Task<int> CreateNestedContentTemplate()
    {
        await using var template = await Template.CreateAsync(
                _nestedTemplate,
                _controlFactory,
                BenchmarkServices.Culture,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        return CountControls(template.BodyControls);
    }

    [Benchmark]
    public async Task<int> CreateMediumRepeatedTemplate()
    {
        await using var template = await Template.CreateAsync(
                _mediumTemplate,
                _controlFactory,
                BenchmarkServices.Culture,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        return CountControls(template.BodyControls);
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

    private static int CountControls(IEnumerable<IControl> controls)
    {
        var count = 0;
        foreach (var control in controls)
        {
            count++;
            if (control is IContentControl contentControl)
                count += CountControls(contentControl);
        }

        return count;
    }
}
