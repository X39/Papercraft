using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Papercraft;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftTests
{
    [Fact]
    public void AddPapercraftRegistersDefaultRendererAndGenerator()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        using var serviceProvider = services.BuildServiceProvider();

        var generator = serviceProvider.GetRequiredService<PapercraftGenerator>();
        var renderer = Assert.Single(generator.Renderers);
        Assert.Equal("skiasharp", renderer.Capabilities.RendererId);
        Assert.True(renderer.Capabilities.OutputKinds.HasFlag(RendererOutputKind.Pdf));
        Assert.Contains(
            renderer.Capabilities.MediaTypes,
            (q) => string.Equals(q, PapercraftMediaTypes.ApplicationPdf, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AddPapercraftCoreDoesNotRegisterRendererRuntime()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();

        using var serviceProvider = services.BuildServiceProvider();

        Assert.Empty(serviceProvider.GetServices<IPapercraftRenderer>());
        Assert.Null(serviceProvider.GetService<PapercraftGenerator>());
    }

    [Fact]
    public async Task AddPdfTemplateServiceRegistersPapercraftCompatibilityFacade()
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService();

        await using var serviceProvider = services.BuildServiceProvider();

        var generator = serviceProvider.GetRequiredService<PapercraftGenerator>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>compatibility</text>");

        await generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);

        Assert.StartsWith("%PDF", Encoding.ASCII.GetString(output.ToArray(), 0, 4), StringComparison.Ordinal);
    }

    [Fact]
    public async Task PapercraftGeneratorRendersPdfThroughDefaultRenderer()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftGenerator>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>Hello, Papercraft!</text>");

        await generator.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, output),
            CultureInfo.InvariantCulture);

        Assert.StartsWith("%PDF", Encoding.ASCII.GetString(output.ToArray(), 0, 4), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateAsyncReportsUnsupportedOutputBeforeRendering()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftGenerator>();
        using var reader = CreateReader("<text>Hello, printer.</text>");

        var validation = await generator.ValidateAsync(
            reader,
            new RenderTarget("application/vnd.papercraft.escpos", RendererOutputKind.PrinterCommands),
            CultureInfo.InvariantCulture);

        Assert.Equal(RendererSupportLevel.Unsupported, validation.SupportLevel);
        var diagnostic = Assert.Single(validation.Diagnostics);
        Assert.Equal("PAPERCRAFT001", diagnostic.Code);
        Assert.Equal(RendererSupportLevel.Unsupported, diagnostic.Level);
        Assert.Contains("does not support", diagnostic.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static XmlReader CreateReader(string body)
    {
        var xml = $$"""
                    <?xml version="1.0" encoding="utf-8"?>
                    <template xmlns="{{Constants.ControlsNamespace}}">
                        <body>
                            {{body}}
                        </body>
                    </template>
                    """;
        return XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
    }
}
