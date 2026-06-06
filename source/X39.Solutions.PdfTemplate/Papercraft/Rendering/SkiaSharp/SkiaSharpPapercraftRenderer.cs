using SkiaSharp;
using X39.Solutions.PdfTemplate;
using X39.Solutions.PdfTemplate.Abstraction;

namespace X39.Papercraft.Rendering.SkiaSharp;

/// <summary>
/// Papercraft renderer backed by the existing SkiaSharp rendering engine.
/// </summary>
[PublicAPI]
public sealed class SkiaSharpPapercraftRenderer : IPapercraftRenderer, IPapercraftTemplateDataAccessor
{
    private static readonly RendererCapabilities StaticCapabilities = new(
        "skiasharp",
        "SkiaSharp",
        RendererOutputKind.Pdf | RendererOutputKind.RasterImage,
        new[] { PapercraftMediaTypes.ApplicationPdf, PapercraftMediaTypes.ImagePng },
        new Dictionary<string, RendererSupportLevel>(StringComparer.OrdinalIgnoreCase)
        {
            [RendererFeatures.PdfOutput] = RendererSupportLevel.Supported,
            [RendererFeatures.RasterImageOutput] = RendererSupportLevel.Supported,
            [RendererFeatures.Multipage] = RendererSupportLevel.Supported,
            [RendererFeatures.TextMeasurement] = RendererSupportLevel.Supported,
            [RendererFeatures.TextDrawing] = RendererSupportLevel.Supported,
            [RendererFeatures.Images] = RendererSupportLevel.Supported,
            [RendererFeatures.Clipping] = RendererSupportLevel.Supported,
            [RendererFeatures.Transparency] = RendererSupportLevel.Supported,
            [RendererFeatures.Fonts] = RendererSupportLevel.Supported,
            [RendererFeatures.Color] = RendererSupportLevel.Supported,
            [RendererFeatures.AbsolutePositioning] = RendererSupportLevel.Supported,
        },
        "Default Papercraft renderer. Supports PDF and single-page PNG stream output through SkiaSharp.");

    private readonly Generator _generator;

    /// <summary>
    /// Creates a new SkiaSharp renderer.
    /// </summary>
    public SkiaSharpPapercraftRenderer(Generator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
        _generator = generator;
    }

    /// <inheritdoc />
    public RendererCapabilities Capabilities => StaticCapabilities;

    /// <inheritdoc />
    public ITemplateData TemplateData => _generator.TemplateData;

    /// <inheritdoc />
    public ValueTask<RenderValidationResult> ValidateAsync(
        PreparedRenderDocument request,
        RenderTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(target);
        cancellationToken.ThrowIfCancellationRequested();

        if (Capabilities.Supports(target))
            return ValueTask.FromResult(RenderValidationResult.Supported);

        var diagnostic = new RenderDiagnostic(
            "PAPERCRAFT001",
            RendererSupportLevel.Unsupported,
            FeatureForTarget(target),
            $"Renderer '{Capabilities.DisplayName}' does not support media type '{target.MediaType}'.",
            $"Supported media types are: {string.Join(", ", Capabilities.MediaTypes)}.");
        return ValueTask.FromResult(new RenderValidationResult(new[] { diagnostic }));
    }

    /// <inheritdoc />
    public async ValueTask RenderAsync(
        PreparedRenderDocument request,
        RenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(output);
        var validation = await ValidateAsync(request, output.Target, cancellationToken)
            .ConfigureAwait(false);
        validation.ThrowIfUnsupported();

        using var reader = request.CreateReader();
        switch (output.Target.OutputKind)
        {
            case RendererOutputKind.Pdf:
                await _generator.GeneratePdfAsync(
                        output.Stream,
                        reader,
                        request.CultureInfo,
                        request.Options.DocumentOptions,
                        cancellationToken)
                    .ConfigureAwait(false);
                break;
            case RendererOutputKind.RasterImage:
                await RenderPngAsync(request, output, cancellationToken)
                    .ConfigureAwait(false);
                break;
            default:
                throw new RenderValidationException(
                    new RenderValidationResult(
                        new[]
                        {
                            new RenderDiagnostic(
                                "PAPERCRAFT001",
                                RendererSupportLevel.Unsupported,
                                FeatureForTarget(output.Target),
                                $"Renderer '{Capabilities.DisplayName}' does not support media type '{output.MediaType}'."),
                        }));
        }
    }

    private async ValueTask RenderPngAsync(
        PreparedRenderDocument request,
        RenderOutput output,
        CancellationToken cancellationToken)
    {
        using var reader = request.CreateReader();
        var bitmaps = await _generator.GenerateBitmapsAsync(
                reader,
                request.CultureInfo,
                request.Options.DocumentOptions,
                cancellationToken)
            .ConfigureAwait(false);
        try
        {
            if (bitmaps.Count is 0)
                return;
            if (bitmaps.Count > 1)
                throw new NotSupportedException(
                    "Single-stream PNG output supports one rendered page. Use the compatibility GenerateBitmapsAsync API for multi-page raster output.");
            using var image = SKImage.FromBitmap(bitmaps.First());
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(output.Stream);
        }
        finally
        {
            foreach (var bitmap in bitmaps)
            {
                bitmap.Dispose();
            }
        }
    }

    private static string FeatureForTarget(RenderTarget target)
        => target.OutputKind switch
        {
            RendererOutputKind.Pdf         => RendererFeatures.PdfOutput,
            RendererOutputKind.RasterImage => RendererFeatures.RasterImageOutput,
            _                              => "output.custom",
        };
}
