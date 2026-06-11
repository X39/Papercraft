using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.Papercraft.Rendering.PdfSharp;
using X39.Solutions.Papercraft.Rendering.PdfSharp.Services;
using X39.Solutions.Papercraft.Rendering.Svg;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftAdditionalBackendTests
{
    [Fact]
    public void SvgRendererRegistrationAddsSvgBackend()
    {
        var services = new ServiceCollection();

        services.AddPapercraftSvgRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();

        Assert.Contains(renderer.Backends, (q) => q.Capabilities.RendererId == "svg");
        Assert.NotNull(provider.GetService<ITextService>());
    }

    [Fact]
    public async Task SvgBackendRendersDisplayListAsSvg()
    {
        var backend = new SvgRenderBackend();
        var document = CreateSimpleDocument(includeText: true);
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(RenderTarget.FromMediaType(SvgRenderBackend.MediaType), stream),
            CancellationToken.None);

        stream.Position = 0;
        var svgDocument = XDocument.Load(stream);
        XNamespace svg = "http://www.w3.org/2000/svg";

        Assert.Equal(svg + "svg", svgDocument.Root?.Name);
        Assert.Equal("Hello Papercraft", svgDocument.Descendants(svg + "text").Single().Value);
        Assert.Contains(svgDocument.Descendants(svg + "rect"), (q) => q.Attribute("fill")?.Value == "#336699");
        Assert.Contains(svgDocument.Descendants(svg + "a"), (q) => q.Attribute("href")?.Value == "https://example.com");
    }

    [Fact]
    public async Task SvgRendererGeneratesXmlTemplateWithText()
    {
        var services = new ServiceCollection();
        services.AddPapercraftSvgRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateTextTemplate("Hello SVG")));

        await renderer.RenderAsync(
            reader,
            new RenderOutput(RenderTarget.FromMediaType(SvgRenderBackend.MediaType), stream),
            CultureInfo.InvariantCulture);

        stream.Position = 0;
        var svgDocument = XDocument.Load(stream);
        XNamespace svg = "http://www.w3.org/2000/svg";

        Assert.Contains(svgDocument.Descendants(svg + "text"), (q) => q.Value == "Hello SVG");
    }

    [Fact]
    public async Task SvgBackendFreezesClipBeforeLaterTranslation()
    {
        var backend = new SvgRenderBackend();
        var document = CreateTranslatedClipDocument();
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(RenderTarget.FromMediaType(SvgRenderBackend.MediaType), stream),
            CancellationToken.None);

        stream.Position = 0;
        var svgDocument = XDocument.Load(stream);
        XNamespace svg = "http://www.w3.org/2000/svg";

        Assert.Contains(
            svgDocument.Descendants(svg + "clipPath").SelectMany((q) => q.Elements(svg + "rect")),
            (q) => HasRectangle(q, 0, 20, 100, 10));

        var drawnRectangle = Assert.Single(
            svgDocument.Descendants(svg + "rect"),
            (q) => q.Attribute("fill")?.Value == "#112233");
        Assert.True(HasRectangle(drawnRectangle, 0, 25, 100, 10));
    }

    [Fact]
    public void PdfSharpRendererRegistrationAddsPdfSharpBackend()
    {
        var services = new ServiceCollection();

        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();

        Assert.Contains(renderer.Backends, (q) => q.Capabilities.RendererId == "pdfsharp");
        Assert.NotNull(provider.GetService<ITextService>());
    }

    [Fact]
    public async Task PdfSharpBackendRendersDisplayListAsPdf()
    {
        var backend = new PdfSharpRenderBackend();
        var document = CreateSimpleDocument(includeText: false);
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
            CancellationToken.None);

        var bytes = stream.ToArray();

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void PdfSharpDisplayListRendererFreezesClipBeforeLaterTranslation()
    {
        using var pdfDocument = new PdfDocument();
        pdfDocument.Options.NoCompression = true;
        var page = pdfDocument.AddPage();
        page.Width = XUnit.FromPoint(100);
        page.Height = XUnit.FromPoint(100);
        var displayList = CreateTranslatedClipDisplayList();

        using (var graphics = XGraphics.FromPdfPage(
                   page,
                   XGraphicsPdfPageOptions.Replace,
                   XGraphicsUnit.Point,
                   XPageDirection.Downwards))
        {
            new PdfSharpDisplayListRenderer().Render(graphics, page, displayList);
        }

        var content = GetPdfContent(page);

        Assert.Contains("0 80 m\n100 80 l\n100 70 l\n0 70 l\nh", content, StringComparison.Ordinal);
        Assert.Contains("0 65 100 10 re", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PdfSharpRendererGeneratesXmlTemplateWithText()
    {
        var services = new ServiceCollection();
        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateTextTemplate("Hello PDFsharp")));

        await renderer.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
            CultureInfo.InvariantCulture);

        var bytes = stream.ToArray();

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }

    private static PapercraftDocument CreateSimpleDocument(bool includeText)
    {
        var displayList = new DisplayList();
        displayList.Add(new DrawRectangleCommand(new DisplayRectangle(4, 6, 48, 20), new DisplayColor(0x33, 0x66, 0x99)));
        displayList.Add(new DrawLineCommand(new DisplayColor(0xCC, 0x22, 0x22), 2, 4, 34, 70, 34));
        displayList.Add(new LinkAnnotationCommand("https://example.com", new DisplayRectangle(4, 6, 48, 20)));
        if (includeText)
        {
            displayList.Add(
                new DrawTextCommand(
                    new DisplayTextStyle
                    {
                        Foreground = DisplayColor.Black,
                        FontFamily = DisplayFont.Default,
                        FontSize = 12F,
                    },
                    72.272F,
                    "Hello Papercraft",
                    4,
                    54));
        }

        return new PapercraftDocument(
            new[]
            {
                new PapercraftPage(
                    0,
                    1,
                    1,
                    new Size(96, 72),
                    DocumentOptions.Default.DotsPerMillimeter,
                    displayList),
            },
            CultureInfo.InvariantCulture,
            DocumentOptions.Default);
    }

    private static PapercraftDocument CreateTranslatedClipDocument()
        => new(
            new[]
            {
                new PapercraftPage(
                    0,
                    1,
                    1,
                    new Size(100, 100),
                    DocumentOptions.Default.DotsPerMillimeter,
                    CreateTranslatedClipDisplayList()),
            },
            CultureInfo.InvariantCulture,
            DocumentOptions.Default);

    private static DisplayList CreateTranslatedClipDisplayList()
    {
        var displayList = new DisplayList();
        displayList.Add(new PushStateCommand());
        displayList.Add(new TranslateCommand(new DisplayPoint(0, 20)));
        displayList.Add(new ClipCommand(new DisplayRectangle(0, 0, 100, 10)));
        displayList.Add(new TranslateCommand(new DisplayPoint(0, -20)));
        displayList.Add(new DrawRectangleCommand(new DisplayRectangle(0, 25, 100, 10), new DisplayColor(0x11, 0x22, 0x33)));
        displayList.Add(new PopStateCommand());
        return displayList;
    }

    private static bool HasRectangle(XElement element, float x, float y, float width, float height)
        => HasNumber(element, "x", x)
           && HasNumber(element, "y", y)
           && HasNumber(element, "width", width)
           && HasNumber(element, "height", height);

    private static bool HasNumber(XElement element, string attributeName, float expected)
        => float.TryParse(
               element.Attribute(attributeName)?.Value,
               NumberStyles.Float,
               CultureInfo.InvariantCulture,
               out var actual)
           && Math.Abs(actual - expected) < 0.001F;

    private static string GetPdfContent(PdfPage page)
    {
        var content = page.Contents.CreateSingleContent();
        var bytes = content.Stream?.UnfilteredValue ?? content.Stream?.Value ?? Array.Empty<byte>();
        return Encoding.ASCII.GetString(bytes);
    }

    private static string CreateTextTemplate(string text)
        => $$"""
             <?xml version="1.0" encoding="utf-8"?>
             <template xmlns="X39.Solutions.PdfTemplate.Controls">
                 <body>
                     <text>{{text}}</text>
                 </body>
             </template>
             """;
}
