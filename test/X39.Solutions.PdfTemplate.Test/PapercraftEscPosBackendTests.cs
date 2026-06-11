using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.Papercraft.Rendering.EscPos;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftEscPosBackendTests
{
    [Fact]
    public void EscPosRendererRegistrationAddsEscPosBackend()
    {
        var services = new ServiceCollection();

        services.AddPapercraftEscPosRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();

        var backend = Assert.Single(renderer.Backends, (q) => q.Capabilities.RendererId == "escpos");
        Assert.Equal(RendererOutputKind.PrinterCommands, backend.Capabilities.OutputKinds);
        Assert.Contains(EscPosRenderBackend.MediaType, backend.Capabilities.MediaTypes);
    }

    [Fact]
    public async Task EscPosBackendRendersSimpleTextAsPrinterCommands()
    {
        var backend = new EscPosRenderBackend();
        var document = CreateDocument(
            new DrawTextCommand(
                new DisplayTextStyle
                {
                    Foreground = DisplayColor.Black,
                    FontFamily = DisplayFont.Default,
                    FontSize = 12F,
                },
                72.272F,
                "Hello POS",
                0,
                0));
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(EscPosRenderBackend.Target, stream),
            CancellationToken.None);

        Assert.Equal(
            new byte[]
            {
                0x1B, 0x40,
                (byte) 'H', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o', (byte) ' ', (byte) 'P', (byte) 'O', (byte) 'S',
                0x0A,
            },
            stream.ToArray());
    }

    [Fact]
    public async Task EscPosBackendRendersEmphasisAndHorizontalRuleCommands()
    {
        var backend = new EscPosRenderBackend(
            new EscPosRenderOptions
            {
                CharactersPerLine = 10,
            });
        var document = CreateDocument(
            new DrawTextCommand(
                new DisplayTextStyle
                {
                    Foreground = DisplayColor.Black,
                    FontFamily = new DisplayFont("receipt")
                    {
                        Weight = 700,
                    },
                    FontSize = 18F,
                    Decoration = TextDecoration.Underline,
                },
                72.272F,
                "Total",
                0,
                0),
            new DrawLineCommand(DisplayColor.Black, 1, 0, 10, 50, 10));
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(EscPosRenderBackend.Target, stream),
            CancellationToken.None);

        Assert.Equal(
            new byte[]
            {
                0x1B, 0x40,
                0x1B, 0x45, 0x01,
                0x1D, 0x21, 0x11,
                0x1B, 0x2D, 0x01,
                (byte) 'T', (byte) 'o', (byte) 't', (byte) 'a', (byte) 'l',
                0x0A,
                0x1B, 0x2D, 0x00,
                0x1D, 0x21, 0x00,
                0x1B, 0x45, 0x00,
                (byte) '-', (byte) '-', (byte) '-', (byte) '-', (byte) '-',
                0x0A,
            },
            stream.ToArray());
    }

    [Fact]
    public async Task EscPosBackendValidationReportsUnsupportedTarget()
    {
        var backend = new EscPosRenderBackend();
        var document = CreateDocument();

        var validation = await backend.ValidateAsync(document, RenderTarget.Pdf, CancellationToken.None);

        Assert.Equal(RendererSupportLevel.Unsupported, validation.SupportLevel);
        Assert.Contains(
            validation.Diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.UnsupportedOutputKind
                   && q.Feature == RendererFeatures.PdfOutput);
        Assert.Contains(
            validation.Diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.UnsupportedMediaType
                   && q.Feature == RendererFeatures.PdfOutput);
    }

    [Fact]
    public async Task EscPosBackendValidationReportsDegradedAndUnsupportedDocumentFeatures()
    {
        var backend = new EscPosRenderBackend();
        var displayList = new DisplayList();
        displayList.Add(
            new DrawTextCommand(
                new DisplayTextStyle
                {
                    Foreground = new DisplayColor(0x20, 0x40, 0x60, 0x80),
                    FontFamily = new DisplayFont("Nunito"),
                },
                96,
                "Styled",
                0,
                0));
        displayList.Add(new DrawImageCommand(new byte[] { 1, 2, 3 }, new DisplayRectangle(0, 10, 1, 1)));
        displayList.Add(new ClipCommand(new DisplayRectangle(0, 0, 10, 10)));
        displayList.Add(new LinkAnnotationCommand("https://example.test", new DisplayRectangle(0, 0, 1, 1)));
        var document = CreateDocument(displayList);

        var validation = await backend.ValidateAsync(document, EscPosRenderBackend.Target, CancellationToken.None);

        Assert.Equal(RendererSupportLevel.Unsupported, validation.SupportLevel);
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == RendererFeatures.Color && q.Level == RendererSupportLevel.Degraded);
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == RendererFeatures.Fonts && q.Level == RendererSupportLevel.Degraded);
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == RendererFeatures.AbsolutePositioning && q.Level == RendererSupportLevel.Degraded);
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == RendererFeatures.Images && q.Level == RendererSupportLevel.Unsupported);
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == RendererFeatures.Clipping && q.Level == RendererSupportLevel.Unsupported);
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == RendererFeatures.Transparency && q.Level == RendererSupportLevel.Unsupported);
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == RendererFeatures.LinkAnnotations && q.Level == RendererSupportLevel.Unsupported);
    }

    [Fact]
    public async Task EscPosBackendValidationRejectsUnsupportedGeometry()
    {
        var backend = new EscPosRenderBackend();
        var document = CreateDocument(
            new DrawRectangleCommand(new DisplayRectangle(0, 0, 10, 10), DisplayColor.Black),
            new DrawLineCommand(DisplayColor.Black, 1, 0, 0, 0, 10));

        var validation = await backend.ValidateAsync(document, EscPosRenderBackend.Target, CancellationToken.None);

        Assert.Equal(RendererSupportLevel.Unsupported, validation.SupportLevel);
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == "drawing.rectangle");
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == "drawing.line.non-horizontal");
    }

    private static PapercraftDocument CreateDocument(params DisplayCommand[] commands)
    {
        var displayList = new DisplayList();
        foreach (var command in commands)
        {
            displayList.Add(command);
        }

        return CreateDocument(displayList);
    }

    private static PapercraftDocument CreateDocument(DisplayList displayList)
        => new(
            new[]
            {
                new PapercraftPage(
                    0,
                    1,
                    1,
                    new Size(100, 50),
                    DocumentOptions.Default.DotsPerMillimeter,
                    displayList),
            },
            CultureInfo.InvariantCulture,
            DocumentOptions.Default);
}
