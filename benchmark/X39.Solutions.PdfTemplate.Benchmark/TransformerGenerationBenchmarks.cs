using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.Generation, BenchmarkCategories.Transformers)]
public class TransformerGenerationBenchmarks
{
    private ServiceProvider _serviceProvider = null!;
    private Generator _generator = null!;
    private DocumentOptions _documentOptions;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _serviceProvider = BenchmarkServices.CreateDefaultServiceProvider();
        _generator = BenchmarkServices.CreateDefaultGenerator(_serviceProvider);
        _documentOptions = new DocumentOptions
        {
            DotsPerInch = 72,
            PageWidthInMillimeters = 120,
            PageHeightInMillimeters = 160,
            Margin = new Thickness(new Length(0.4F, ELengthUnit.Centimeters)),
            Modified = new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc),
            Producer = "X39.Solutions.PdfTemplate.Benchmark",
        };

        GenerateExpandedTemplateBitmaps().GetAwaiter().GetResult();
        GenerateTransformedTemplateBitmaps().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _generator.Dispose();
        _serviceProvider.Dispose();
    }

    [Benchmark(Baseline = true)]
    public Task<int> GenerateExpandedTemplateBitmaps()
    {
        return GenerateAsync(BenchmarkTemplates.TransformerHeavyExpandedGenerationTemplate);
    }

    [Benchmark]
    public Task<int> GenerateTransformedTemplateBitmaps()
    {
        return GenerateAsync(BenchmarkTemplates.TransformerHeavyGenerationTemplate);
    }

    private async Task<int> GenerateAsync(byte[] template)
    {
        using var reader = BenchmarkTemplates.CreateXmlReader(template);
        var bitmaps = await _generator.GenerateBitmapsAsync(
                reader,
                BenchmarkServices.Culture,
                _documentOptions,
                CancellationToken.None)
            .ConfigureAwait(false);
        return DisposeAndCount(bitmaps);
    }

    private static int DisposeAndCount(IReadOnlyCollection<SKBitmap> bitmaps)
    {
        try
        {
            return bitmaps.Count;
        }
        finally
        {
            foreach (var bitmap in bitmaps)
            {
                bitmap.Dispose();
            }
        }
    }
}
