using System.Xml;
using X39.Solutions.Papercraft.Abstraction;
using static System.String;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Application-facing Papercraft render facade.
/// </summary>
public sealed class PapercraftRenderer
{
    private readonly PapercraftGenerator _generator;
    private readonly IPapercraftRenderBackend[] _backends;

    /// <summary>
    /// Creates a new Papercraft render facade.
    /// </summary>
    public PapercraftRenderer(
        PapercraftGenerator generator,
        IEnumerable<IPapercraftRenderBackend> backends)
    {
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(backends);
        _generator = generator;
        _backends = backends.ToArray();
    }

    /// <summary>
    /// Template data used for generation.
    /// </summary>
    public ITemplateData TemplateData => _generator.TemplateData;

    /// <summary>
    /// Registered render backends.
    /// </summary>
    public IReadOnlyCollection<IPapercraftRenderBackend> Backends => _backends;

    /// <summary>
    /// Validates a template against a render target.
    /// </summary>
    public async ValueTask<RenderValidationResult> ValidateAsync(
        XmlReader reader,
        RenderTarget target,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        var renderOptions = options ?? PapercraftRenderOptions.Default;
        var backend = SelectBackend(target, renderOptions);
        var document = await _generator.GenerateAsync(
                reader,
                cultureInfo,
                renderOptions.DocumentOptions,
                cancellationToken)
            .ConfigureAwait(false);
        return await ValidateBackendAsync(backend, document, target, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Renders a template to the supplied output.
    /// </summary>
    public async ValueTask RenderAsync(
        XmlReader reader,
        RenderOutput output,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        var renderOptions = options ?? PapercraftRenderOptions.Default;
        var backend = SelectBackend(output.Target, renderOptions);
        var document = await _generator.GenerateAsync(
                reader,
                cultureInfo,
                renderOptions.DocumentOptions,
                cancellationToken)
            .ConfigureAwait(false);
        await RenderAsync(document, output, renderOptions, backend, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Renders a generated document to the supplied output.
    /// </summary>
    public async ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);

        var renderOptions = options ?? PapercraftRenderOptions.Default;
        var backend = SelectBackend(output.Target, renderOptions);
        await RenderAsync(document, output, renderOptions, backend, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Renders a template as page-by-page raster output.
    /// </summary>
    public async ValueTask RenderRasterPagesAsync(
        XmlReader reader,
        RasterPageRenderOutput output,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        var renderOptions = options ?? PapercraftRenderOptions.Default;
        var backend = SelectBackend(output.Target, renderOptions);
        var document = await _generator.GenerateAsync(
                reader,
                cultureInfo,
                renderOptions.DocumentOptions,
                cancellationToken)
            .ConfigureAwait(false);
        await RenderRasterPagesAsync(document, output, renderOptions, backend, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Renders a generated document as page-by-page raster output.
    /// </summary>
    public async ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);

        var renderOptions = options ?? PapercraftRenderOptions.Default;
        var backend = SelectBackend(output.Target, renderOptions);
        await RenderRasterPagesAsync(document, output, renderOptions, backend, cancellationToken)
            .ConfigureAwait(false);
    }

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

    private async ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        PapercraftRenderOptions options,
        IPapercraftRenderBackend backend,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateBackendAsync(backend, document, output.Target, cancellationToken)
            .ConfigureAwait(false);
        validation.ThrowIfUnsupportedOrStrictDegraded(options.TreatDegradedAsUnsupported);
        await backend.RenderAsync(document, output, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        PapercraftRenderOptions options,
        IPapercraftRenderBackend backend,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateBackendAsync(backend, document, output.Target, cancellationToken)
            .ConfigureAwait(false);
        validation.ThrowIfUnsupportedOrStrictDegraded(options.TreatDegradedAsUnsupported);
        await backend.RenderRasterPagesAsync(document, output, cancellationToken)
            .ConfigureAwait(false);
    }

    private IPapercraftRenderBackend SelectBackend(RenderTarget target, PapercraftRenderOptions options)
    {
        if (_backends.Length is 0)
            throw new InvalidOperationException("No Papercraft render backend is registered.");

        if (!IsNullOrWhiteSpace(options.BackendId))
        {
            var backend = _backends.FirstOrDefault(
                (q) => string.Equals(
                    q.Capabilities.RendererId,
                    options.BackendId,
                    StringComparison.OrdinalIgnoreCase));
            if (backend is null)
                throw new InvalidOperationException($"No Papercraft render backend with id '{options.BackendId}' is registered.");
            return backend;
        }

        return _backends.FirstOrDefault((q) => q.Capabilities.Supports(target))
               ?? _backends.First();
    }

    private static async ValueTask<RenderValidationResult> ValidateBackendAsync(
        IPapercraftRenderBackend backend,
        PapercraftDocument document,
        RenderTarget target,
        CancellationToken cancellationToken)
    {
        var backendValidation = await backend.ValidateAsync(document, target, cancellationToken)
            .ConfigureAwait(false);
        return RenderValidationResult.Combine(
            backendValidation,
            backend.Capabilities.ValidateDocument(document));
    }
}
