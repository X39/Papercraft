using System.Xml;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Services;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Application-facing Papercraft render facade.
/// </summary>
[Obsolete("Use Papercraft.CreateSession() and PapercraftSession for application-facing rendering.")]
public sealed class PapercraftRenderer
{
    private readonly PapercraftRenderPipeline _pipeline;

    /// <summary>
    /// Creates a new Papercraft render facade.
    /// </summary>
    public PapercraftRenderer(
        PapercraftGenerator generator,
        IEnumerable<IPapercraftRenderBackend> backends)
    {
        _pipeline = new PapercraftRenderPipeline(generator, backends);
    }

    /// <summary>
    /// Creates a new Papercraft render facade with backend-aware control activation.
    /// </summary>
    public PapercraftRenderer(
        PapercraftGenerator generator,
        IEnumerable<IPapercraftRenderBackend> backends,
        IServiceProvider serviceProvider,
        ControlActivationCache controlActivationCache,
        ControlRegistry controlRegistry)
    {
        _pipeline = new PapercraftRenderPipeline(
            generator,
            backends,
            serviceProvider,
            controlActivationCache,
            controlRegistry);
    }

    /// <summary>
    /// Template data used for generation.
    /// </summary>
    public ITemplateData TemplateData => _pipeline.TemplateData;

    /// <summary>
    /// Registered render backends.
    /// </summary>
    public IReadOnlyCollection<IPapercraftRenderBackend> Backends => _pipeline.Backends;

    /// <summary>
    /// Validates a template against a render target.
    /// </summary>
    public async ValueTask<RenderValidationResult> ValidateAsync(
        XmlReader reader,
        RenderTarget target,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
        => await _pipeline.ValidateAsync(reader, target, cultureInfo, options, cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Renders a template to the supplied output.
    /// </summary>
    public async ValueTask RenderAsync(
        XmlReader reader,
        RenderOutput output,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
        => await _pipeline.RenderAsync(reader, output, cultureInfo, options, cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Renders a generated document to the supplied output.
    /// </summary>
    public async ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
        => await _pipeline.RenderAsync(document, output, options, cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Renders a template as page-by-page raster output.
    /// </summary>
    public async ValueTask RenderRasterPagesAsync(
        XmlReader reader,
        RasterPageRenderOutput output,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
        => await _pipeline.RenderRasterPagesAsync(reader, output, cultureInfo, options, cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Renders a generated document as page-by-page raster output.
    /// </summary>
    public async ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
        => await _pipeline.RenderRasterPagesAsync(document, output, options, cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Generates a PDF document from the supplied template.
    /// </summary>
    public async Task GeneratePdfAsync(
        Stream outputStream,
        XmlReader reader,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outputStream);
        await RenderAsync(
                reader,
                new RenderOutput(PapercraftMediaTypes.ApplicationPdf, outputStream),
                cultureInfo,
                options,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Writes the lowered XML produced from the supplied template before backend rendering.
    /// </summary>
    public async Task GenerateLoweredXmlAsync(
        Stream outputStream,
        XmlReader reader,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outputStream);
        await RenderAsync(
                reader,
                new RenderOutput(RenderTarget.LoweredXml, outputStream),
                cultureInfo,
                options,
                cancellationToken)
            .ConfigureAwait(false);
    }
}
