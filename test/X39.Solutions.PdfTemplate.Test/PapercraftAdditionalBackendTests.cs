using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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

    [Fact]
    public async Task PdfSharpRendererReportsMissingFontThroughRenderOutput()
    {
        var services = new ServiceCollection();
        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        var diagnostics = new List<RenderDiagnostic>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateMissingFontTemplate()));

        await renderer.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream, diagnostics.Add),
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions { BackendId = PdfSharpRenderBackend.RendererId });

        Assert.Contains(
            diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.MissingFontSubstitution
                   && q.Level is RendererSupportLevel.Degraded);
        var bytes = stream.ToArray();
        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task PdfSharpRendererTreatsMissingFontAsUnsupportedInStrictDegradedMode()
    {
        var services = new ServiceCollection();
        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateMissingFontTemplate()));

        var exception = await Assert.ThrowsAsync<RenderValidationException>(
            async () => await renderer.RenderAsync(
                reader,
                new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
                CultureInfo.InvariantCulture,
                new PapercraftRenderOptions
                {
                    BackendId = PdfSharpRenderBackend.RendererId,
                    TreatDegradedAsUnsupported = true,
                }));

        Assert.Contains(
            exception.ValidationResult.Diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.MissingFontSubstitution);
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public async Task PdfSharpRendererKeepsCalibriRegularAndBoldFacesDistinct()
    {
        var fontsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
        if (!OperatingSystem.IsWindows()
            || !File.Exists(Path.Combine(fontsDirectory, "calibri.ttf"))
            || !File.Exists(Path.Combine(fontsDirectory, "calibrib.ttf")))
        {
            return;
        }

        var services = new ServiceCollection();
        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateCalibriTableTemplate()));

        await renderer.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions
            {
                BackendId = PdfSharpRenderBackend.RendererId,
                DocumentOptions = new DocumentOptions
                {
                    DotsPerInch = 96,
                    PageWidthInMillimeters = 90,
                    PageHeightInMillimeters = 45,
                },
            });

        var baseFonts = ExtractBaseFontNames(stream.ToArray());
        var calibriFonts = baseFonts
            .Where((q) => q.Contains("Calibri", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Contains(calibriFonts, (q) => q.Contains("Calibri,Bold", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            calibriFonts,
            (q) => q.Contains("Calibri", StringComparison.OrdinalIgnoreCase)
                   && !q.Contains("Bold", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task PdfSharpBackendRendersMixedRegularAndBoldTextAsPdf()
    {
        var backend = new PdfSharpRenderBackend();
        var displayList = new DisplayList();
        displayList.Add(
            new DrawTextCommand(
                new DisplayTextStyle
                {
                    Foreground = DisplayColor.Black,
                    FontFamily = DisplayFont.Default with { Weight = 599 },
                    FontSize = 12F,
                },
                72.272F,
                "Regular",
                4,
                20));
        displayList.Add(
            new DrawTextCommand(
                new DisplayTextStyle
                {
                    Foreground = DisplayColor.Black,
                    FontFamily = DisplayFont.Default with { Weight = 600 },
                    FontSize = 12F,
                },
                72.272F,
                "Bold",
                4,
                40));
        var document = new PapercraftDocument(
            new[]
            {
                new PapercraftPage(
                    0,
                    1,
                    1,
                    new Size(120, 80),
                    DocumentOptions.Default.DotsPerMillimeter,
                    displayList),
            },
            CultureInfo.InvariantCulture,
            DocumentOptions.Default);
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
            CancellationToken.None);

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

    private static IReadOnlyCollection<string> ExtractBaseFontNames(byte[] pdfBytes)
    {
        var pdfText = Encoding.Latin1.GetString(pdfBytes);
        return Regex.Matches(pdfText, @"/BaseFont\s*/(?<name>[^\s/<>\[\]\(\)]+)")
            .Select((q) => DecodePdfName(q.Groups["name"].Value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string DecodePdfName(string name)
        => Regex.Replace(
            name,
            "#(?<hex>[0-9A-Fa-f]{2})",
            (match) => ((char)Convert.ToByte(match.Groups["hex"].Value, 16)).ToString());

    private static string CreateTextTemplate(string text)
        => $$"""
             <?xml version="1.0" encoding="utf-8"?>
             <template xmlns="X39.Solutions.PdfTemplate.Controls">
                 <body>
                     <text>{{text}}</text>
                 </body>
             </template>
             """;

    private static string CreateCalibriTableTemplate()
        => """
           <?xml version="1.0" encoding="utf-8"?>
           <template xmlns="X39.Solutions.PdfTemplate.Controls">
               <body>
                   <table margin="2mm">
                       <tr>
                           <td width="25mm">
                               <text fontFamily="Calibri" fontSize="9" horizontalAlignment="right">11,11 EUR</text>
                           </td>
                           <td width="25mm">
                               <text fontFamily="Calibri" fontSize="9" weight="bold" horizontalAlignment="right">22,22 EUR</text>
                           </td>
                       </tr>
                   </table>
               </body>
           </template>
           """;

    private static string CreateMissingFontTemplate()
        => """
           <?xml version="1.0" encoding="utf-8"?>
           <template xmlns="X39.Solutions.PdfTemplate.Controls">
               <body>
                   <text fontFamily="Papercraft Missing Font 7B2ED63A3D53415C">Missing font</text>
               </body>
           </template>
           """;
}
