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

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererValidate);
        PapercraftActivity.SetRenderTarget(activity, target);
        try
        {
            var renderOptions = options ?? PapercraftRenderOptions.Default;
            var backend = SelectBackend(target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            var document = await _generator.GenerateAsync(
                    reader,
                    cultureInfo,
                    renderOptions.DocumentOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetDocument(activity, document);
            var validation = await ValidateBackendAsync(backend, document, target, cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetValidation(activity, validation);
            return validation;
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
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

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererRender);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        try
        {
            var renderOptions = options ?? PapercraftRenderOptions.Default;
            var backend = SelectBackend(output.Target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            var document = await _generator.GenerateAsync(
                    reader,
                    cultureInfo,
                    renderOptions.DocumentOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetDocument(activity, document);
            await RenderAsync(document, output, renderOptions, backend, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
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

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererRender);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        try
        {
            var renderOptions = options ?? PapercraftRenderOptions.Default;
            var backend = SelectBackend(output.Target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            await RenderAsync(document, output, renderOptions, backend, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
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

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererRenderRasterPages);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        try
        {
            var renderOptions = options ?? PapercraftRenderOptions.Default;
            var backend = SelectBackend(output.Target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            var document = await _generator.GenerateAsync(
                    reader,
                    cultureInfo,
                    renderOptions.DocumentOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetDocument(activity, document);
            await RenderRasterPagesAsync(document, output, renderOptions, backend, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
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

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererRenderRasterPages);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        try
        {
            var renderOptions = options ?? PapercraftRenderOptions.Default;
            var backend = SelectBackend(output.Target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            await RenderRasterPagesAsync(document, output, renderOptions, backend, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
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
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererBackendRender);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        PapercraftActivity.SetBackend(activity, backend);
        try
        {
            var validation = await ValidateBackendAsync(backend, document, output.Target, cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetValidation(activity, validation);
            validation.ThrowIfUnsupportedOrStrictDegraded(options.TreatDegradedAsUnsupported);
            await backend.RenderAsync(document, output, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private async ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        PapercraftRenderOptions options,
        IPapercraftRenderBackend backend,
        CancellationToken cancellationToken)
    {
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererBackendRenderRasterPages);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        PapercraftActivity.SetBackend(activity, backend);
        try
        {
            var validation = await ValidateBackendAsync(backend, document, output.Target, cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetValidation(activity, validation);
            validation.ThrowIfUnsupportedOrStrictDegraded(options.TreatDegradedAsUnsupported);
            await backend.RenderRasterPagesAsync(document, output, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private IPapercraftRenderBackend SelectBackend(RenderTarget target, PapercraftRenderOptions options)
    {
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererSelectBackend);
        PapercraftActivity.SetRenderTarget(activity, target);
        if (!IsNullOrWhiteSpace(options.BackendId))
            activity?.SetTag(PapercraftActivity.BackendIdTag, options.BackendId);
        try
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
                PapercraftActivity.SetBackend(activity, backend);
                return backend;
            }

            var selected = _backends.FirstOrDefault((q) => q.Capabilities.Supports(target))
                           ?? _backends.First();
            PapercraftActivity.SetBackend(activity, selected);
            return selected;
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private static async ValueTask<RenderValidationResult> ValidateBackendAsync(
        IPapercraftRenderBackend backend,
        PapercraftDocument document,
        RenderTarget target,
        CancellationToken cancellationToken)
    {
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererValidateBackend);
        PapercraftActivity.SetRenderTarget(activity, target);
        PapercraftActivity.SetDocument(activity, document);
        PapercraftActivity.SetBackend(activity, backend);
        try
        {
            var backendValidation = await backend.ValidateAsync(document, target, cancellationToken)
                .ConfigureAwait(false);
            var validation = RenderValidationResult.Combine(
                backendValidation,
                backend.Capabilities.ValidateDocument(document));
            PapercraftActivity.SetValidation(activity, validation);
            return validation;
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }
}
