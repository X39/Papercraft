using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory("Generation")]
public class GenerationBenchmarks
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
            Margin = new Thickness(new Length(1, ELengthUnit.Centimeters)),
            Modified = new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc),
            Producer = "X39.Solutions.PdfTemplate.Benchmark",
        };

        GenerateRepresentativeInvoiceBitmaps().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _generator.Dispose();
        _serviceProvider.Dispose();
    }

    [Benchmark]
    public async Task<int> GenerateRepresentativeInvoiceBitmaps()
    {
        using var reader = BenchmarkTemplates.CreateXmlReader(BenchmarkTemplates.RepresentativeGenerationTemplate);
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
