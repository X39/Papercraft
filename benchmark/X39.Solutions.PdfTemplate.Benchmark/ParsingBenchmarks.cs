using BenchmarkDotNet.Attributes;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Xml;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.Parsing)]
public class ParsingBenchmarks
{
    private readonly TemplateData _templateData = new();

    [Benchmark(Baseline = true)]
    public Task<XmlNodeInformation> ParseSmallTemplate()
    {
        return ParseAsync(BenchmarkTemplates.SmallParseTemplate);
    }

    [Benchmark]
    public Task<XmlNodeInformation> ParseMediumRepeatedControls()
    {
        return ParseAsync(BenchmarkTemplates.MediumParseTemplate);
    }

    [Benchmark]
    public Task<XmlNodeInformation> ParseLargeRepeatedControls()
    {
        return ParseAsync(BenchmarkTemplates.LargeParseTemplate);
    }

    private async Task<XmlNodeInformation> ParseAsync(byte[] bytes)
    {
        using var reader = BenchmarkTemplates.CreateXmlReader(bytes);
        using var templateReader = new XmlTemplateReader(
            DocumentOptions.Default,
            BenchmarkServices.Culture,
            _templateData,
            Array.Empty<ITransformer>());
        return await templateReader.ReadAsync(reader).ConfigureAwait(false);
    }
}
