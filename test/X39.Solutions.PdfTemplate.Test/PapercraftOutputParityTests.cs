using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftOutputParityTests
{
    private static readonly byte[] PdfHeader = "%PDF-"u8.ToArray();

    private static readonly byte[] PdfEofMarker = "%%EOF"u8.ToArray();

    private static readonly byte[] PngHeader =
    {
        0x89,
        0x50,
        0x4E,
        0x47,
        0x0D,
        0x0A,
        0x1A,
        0x0A,
    };

    [Fact]
    public async Task PapercraftFacadeAndLegacyGeneratorProducePdfForSameTemplate()
    {
        var templateXml = CreateSinglePageTemplateXml();

        var papercraftPdf = await RenderPapercraftPdfAsync(templateXml);
        var legacyPdf = await RenderLegacyPdfAsync(templateXml);

        AssertPdfDocument(papercraftPdf);
        AssertPdfDocument(legacyPdf);
    }

    [Fact]
    public async Task PapercraftRenderAsyncProducesPngForSinglePageTemplate()
    {
        var png = await RenderPapercraftAsync(
            CreateSinglePageTemplateXml(),
            PapercraftMediaTypes.ImagePng);

        AssertStartsWith(PngHeader, png, "Expected Papercraft RenderAsync to write a PNG signature.");
        Assert.True(png.Length > PngHeader.Length, "Expected Papercraft RenderAsync to write a non-empty PNG.");
    }

    [Fact]
    public async Task PapercraftRenderRasterPagesAsyncProducesPngPagesMatchingLegacyBitmapCount()
    {
        var templateXml = CreateMultiPageTemplateXml();
        var options = new PapercraftRenderOptions
        {
            DocumentOptions = new DocumentOptions
            {
                DotsPerMillimeter = 4,
                PageWidthInMillimeters = 50,
                PageHeightInMillimeters = 16,
            },
        };
        var legacyPageCount = await CountLegacyBitmapsAsync(templateXml, options.DocumentOptions);
        var rasterPages = await RenderPapercraftRasterPagesAsync(templateXml, options);

        Assert.True(legacyPageCount > 1, "Expected the representative template to produce multiple raster pages.");
        Assert.Equal(legacyPageCount, rasterPages.Count);
        for (var index = 0; index < rasterPages.Count; index++)
        {
            var (info, bytes) = rasterPages[index];
            Assert.Equal(index, info.PageIndex);
            Assert.Equal(index + 1, info.PageNumber);
            Assert.Equal(PapercraftMediaTypes.ImagePng, info.MediaType);
            Assert.True(info.PixelWidth > 0);
            Assert.True(info.PixelHeight > 0);
            Assert.Equal(options.DocumentOptions.DotsPerMillimeter, info.DotsPerMillimeter);
            AssertStartsWith(PngHeader, bytes, "Expected each raster page to be encoded as PNG.");
            Assert.True(bytes.Length > PngHeader.Length, "Expected each raster page to be non-empty.");
        }
    }

    private static async Task<byte[]> RenderPapercraftPdfAsync(string templateXml)
        => await RenderPapercraftAsync(templateXml, PapercraftMediaTypes.ApplicationPdf);

    private static async Task<byte[]> RenderPapercraftAsync(string templateXml, string mediaType)
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var output = new MemoryStream();
        using var reader = CreateReader(templateXml);

        await generator.RenderAsync(
            reader,
            new RenderOutput(mediaType, output),
            CultureInfo.InvariantCulture);

        return output.ToArray();
    }

    private static async Task<byte[]> RenderLegacyPdfAsync(string templateXml)
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<Generator>();
        await using var output = new MemoryStream();
        using var reader = CreateReader(templateXml);

        await generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);

        return output.ToArray();
    }

    private static async Task<IReadOnlyList<(RasterPageInfo Info, byte[] Bytes)>> RenderPapercraftRasterPagesAsync(
        string templateXml,
        PapercraftRenderOptions options)
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        var pages = new List<(RasterPageInfo Info, MemoryStream Stream)>();
        using var reader = CreateReader(templateXml);

        await generator.RenderRasterPagesAsync(
            reader,
            new RasterPageRenderOutput(
                PapercraftMediaTypes.ImagePng,
                (info, _) =>
                {
                    var stream = new MemoryStream();
                    pages.Add((info, stream));
                    return ValueTask.FromResult<Stream>(stream);
                },
                leaveStreamsOpen: true),
            CultureInfo.InvariantCulture,
            options);

        return pages.Select((q) => (q.Info, q.Stream.ToArray())).ToArray();
    }

    private static async Task<int> CountLegacyBitmapsAsync(string templateXml, DocumentOptions documentOptions)
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService();

        await using var serviceProvider = services.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var reader = CreateReader(templateXml);
        var bitmaps = await generator.GenerateBitmapsAsync(
            reader,
            CultureInfo.InvariantCulture,
            documentOptions);
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

    private static XmlReader CreateReader(string templateXml)
        => XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(templateXml)));

    private static string CreateSinglePageTemplateXml()
        => $$"""
             <?xml version="1.0" encoding="utf-8"?>
             <template xmlns="{{Constants.ControlsNamespace}}">
                 <body>
                     <text>Representative output parity smoke</text>
                 </body>
             </template>
             """;

    private static string CreateMultiPageTemplateXml()
    {
        var rows = string.Join(
            Environment.NewLine,
            Enumerable.Range(1, 36)
                      .Select((q) => $"""<text fontsize="8">Raster page {q:00}</text>"""));
        return $$"""
                 <?xml version="1.0" encoding="utf-8"?>
                 <template xmlns="{{Constants.ControlsNamespace}}">
                     <body>
                         {{rows}}
                     </body>
                 </template>
                 """;
    }

    private static void AssertPdfDocument(byte[] bytes)
    {
        AssertStartsWith(PdfHeader, bytes, "Expected PDF output to start with the PDF signature.");
        AssertContains(PdfEofMarker, bytes, "Expected PDF output to contain an EOF marker.");
        Assert.True(bytes.Length > PdfHeader.Length + PdfEofMarker.Length, "Expected a non-empty PDF document.");
    }

    private static void AssertStartsWith(byte[] expected, byte[] actual, string message)
        => Assert.True(actual.AsSpan().StartsWith(expected), message);

    private static void AssertContains(byte[] expected, byte[] actual, string message)
        => Assert.True(actual.AsSpan().IndexOf(expected) >= 0, message);
}
