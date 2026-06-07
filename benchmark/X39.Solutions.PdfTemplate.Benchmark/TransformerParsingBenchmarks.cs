using BenchmarkDotNet.Attributes;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Xml;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.Parsing, BenchmarkCategories.Transformers)]
public class TransformerParsingBenchmarks
{
    private readonly ITransformer[] _transformers =
    [
        new Papercraft.Transformers.ForTransformer(),
        new Papercraft.Transformers.IfTransformer(),
        new Papercraft.Transformers.ForEachTransformer(),
        new Papercraft.Transformers.AlternateTransformer(),
        new Papercraft.Transformers.VariableTransformer(),
    ];

    private byte[] _expandedTemplate = null!;
    private byte[] _transformedTemplate = null!;

    [ParamsSource(nameof(Transformers))]
    public string Transformer { get; set; } = "For";

    [Params(BenchmarkTemplateSize.Small, BenchmarkTemplateSize.Medium, BenchmarkTemplateSize.Large)]
    public BenchmarkTemplateSize Size { get; set; }

    public IEnumerable<string> Transformers => BenchmarkTemplates.TransformerCases;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _expandedTemplate = BenchmarkTemplates.GetTransformerTemplate(Transformer, Size, expanded: true);
        _transformedTemplate = BenchmarkTemplates.GetTransformerTemplate(Transformer, Size, expanded: false);
    }

    [Benchmark(Baseline = true)]
    public Task<XmlNodeInformation> ParseExpandedTemplate()
    {
        return ParseAsync(_expandedTemplate, useTransformers: false);
    }

    [Benchmark]
    public Task<XmlNodeInformation> ParseTransformedTemplate()
    {
        return ParseAsync(_transformedTemplate, useTransformers: true);
    }

    private async Task<XmlNodeInformation> ParseAsync(byte[] bytes, bool useTransformers)
    {
        using var reader = BenchmarkTemplates.CreateXmlReader(bytes);
        using var templateReader = new XmlTemplateReader(
            DocumentOptions.Default,
            BenchmarkServices.Culture,
            CreateTemplateData(),
            useTransformers ? _transformers : Array.Empty<ITransformer>());
        return await templateReader.ReadAsync(reader).ConfigureAwait(false);
    }

    private TemplateData CreateTemplateData()
    {
        var templateData = new TemplateData();
        if (Transformer == "ForEach")
            templateData.SetVariable("items", Enumerable.Range(0, (int) Size).ToArray());
        return templateData;
    }
}
