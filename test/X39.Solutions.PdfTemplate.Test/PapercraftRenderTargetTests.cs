using X39.Solutions.Papercraft;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftRenderTargetTests
{
    [Fact]
    public void FromMediaTypeClassifiesSvgAsVectorImage()
    {
        var target = RenderTarget.FromMediaType("image/svg+xml");

        Assert.Equal(RendererOutputKind.VectorImage, target.OutputKind);
        Assert.Equal("image/svg+xml", target.MediaType);
    }

    [Fact]
    public void RenderOutputCanCarryExplicitOutputKindForCustomMediaTypes()
    {
        using var stream = new MemoryStream();
        var target = new RenderTarget("application/vnd.papercraft.escpos", RendererOutputKind.PrinterCommands);

        var output = new RenderOutput(target, stream);

        Assert.Same(target, output.Target);
        Assert.Same(stream, output.Stream);
        Assert.Equal("application/vnd.papercraft.escpos", output.MediaType);
        Assert.Equal(RendererOutputKind.PrinterCommands, output.Target.OutputKind);
    }

    [Fact]
    public void RendererCapabilitiesFeatureLookupIsCaseInsensitive()
    {
        var capabilities = new RendererCapabilities(
            "case-test",
            "Case Test",
            RendererOutputKind.Pdf,
            new[] { PapercraftMediaTypes.ApplicationPdf },
            new Dictionary<string, RendererSupportLevel>
            {
                [RendererFeatures.Color] = RendererSupportLevel.Degraded,
            });

        Assert.Equal(RendererSupportLevel.Degraded, capabilities.GetFeatureSupport("DRAWING.COLOR"));
    }
}
