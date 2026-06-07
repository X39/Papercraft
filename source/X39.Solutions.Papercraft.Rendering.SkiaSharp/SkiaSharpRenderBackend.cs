using SkiaSharp;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp;

/// <summary>
/// Papercraft render backend backed by SkiaSharp.
/// </summary>
public sealed class SkiaSharpRenderBackend : IPapercraftRenderBackend
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
        "Default Papercraft backend. Supports PDF, single-page PNG stream output and page-by-page PNG raster output through SkiaSharp.");

    private readonly SkiaSharpDisplayListRenderer _displayListRenderer;

    /// <summary>
    /// Creates a new SkiaSharp backend.
    /// </summary>
    public SkiaSharpRenderBackend(SkiaSharpDisplayListRenderer displayListRenderer)
    {
        ArgumentNullException.ThrowIfNull(displayListRenderer);
        _displayListRenderer = displayListRenderer;
    }

    /// <inheritdoc />
    public RendererCapabilities Capabilities => StaticCapabilities;

    /// <inheritdoc />
    public ValueTask<RenderValidationResult> ValidateAsync(
        PapercraftDocument document,
        RenderTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(target);
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(Capabilities.ValidateTarget(target));
    }

    /// <inheritdoc />
    public async ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);
        var validation = await ValidateAsync(document, output.Target, cancellationToken)
            .ConfigureAwait(false);
        validation.ThrowIfUnsupported();

        switch (output.Target.OutputKind)
        {
            case RendererOutputKind.Pdf:
                RenderPdf(document, output.Stream, cancellationToken);
                break;
            case RendererOutputKind.RasterImage:
                RenderPng(document, output.Stream, cancellationToken);
                break;
            default:
                throw new RenderValidationException(
                    new RenderValidationResult(
                        new[]
                        {
                            new RenderDiagnostic(
                                RenderDiagnosticCodes.UnsupportedOutputKind,
                                RendererSupportLevel.Unsupported,
                                RendererFeatures.ForOutputKind(output.Target.OutputKind),
                                $"Backend '{Capabilities.DisplayName}' does not have a render path for output kind '{output.Target.OutputKind}'."),
                        }));
        }
    }

    /// <inheritdoc />
    public async ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);
        var validation = await ValidateAsync(document, output.Target, cancellationToken)
            .ConfigureAwait(false);
        validation.ThrowIfUnsupported();

        foreach (var page in document.Pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var bitmap = RenderPageToBitmap(page, document.DocumentOptions);
            var pageInfo = new RasterPageInfo(
                page.PageIndex,
                page.PageNumber,
                output.MediaType,
                bitmap.Width,
                bitmap.Height,
                page.DotsPerMillimeter);
            var pageStream = await output.OpenPageStreamAsync(pageInfo, cancellationToken)
                .ConfigureAwait(false)
                             ?? throw new InvalidOperationException(
                                 "The raster page output callback must return a stream.");
            try
            {
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                data.SaveTo(pageStream);
                await pageStream.FlushAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                if (!output.LeaveStreamsOpen)
                    await pageStream.DisposeAsync()
                        .ConfigureAwait(false);
            }
        }
    }

    private void RenderPdf(
        PapercraftDocument document,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        var options = document.DocumentOptions;
        using var skDocument = SKDocument.CreatePdf(
            outputStream,
            new SKDocumentPdfMetadata
            {
                RasterDpi = options.DotsPerInch,
                Producer = options.Producer,
                Modified = options.Modified,
                PdfA = true,
            });

        if (document.Pages.Count is 0)
        {
            var size = GetFallbackPageSize(options);
            skDocument.BeginPage(size.Width, size.Height).Dispose();
            skDocument.EndPage();
            skDocument.Close();
            return;
        }

        foreach (var page in document.Pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var canvas = skDocument.BeginPage(page.PageSize.Width, page.PageSize.Height);
            _displayListRenderer.Render(canvas, page.DisplayList);
            skDocument.EndPage();
        }

        skDocument.Close();
    }

    private void RenderPng(
        PapercraftDocument document,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        if (document.Pages.Count is 0)
            return;
        if (document.Pages.Count > 1)
            throw new NotSupportedException(
                "Single-stream PNG output supports one rendered page. Use RenderRasterPagesAsync for multi-page raster output.");

        cancellationToken.ThrowIfCancellationRequested();
        using var bitmap = RenderPageToBitmap(document.Pages[0], document.DocumentOptions);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(outputStream);
    }

    private SKBitmap RenderPageToBitmap(PapercraftPage page, DocumentOptions options)
    {
        var bitmap = new SKBitmap(
            (int)Math.Ceiling(page.PageSize.Width),
            (int)Math.Ceiling(page.PageSize.Height));
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        _displayListRenderer.Render(canvas, page.DisplayList);
        return bitmap;
    }

    private static (float Width, float Height) GetFallbackPageSize(DocumentOptions options)
        => (
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);
}
