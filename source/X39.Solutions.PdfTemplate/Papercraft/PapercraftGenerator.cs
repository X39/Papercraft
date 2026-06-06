using System.Xml;
using X39.Solutions.PdfTemplate.Abstraction;

namespace X39.Papercraft;

/// <summary>
/// Primary Papercraft document generator.
/// </summary>
/// <remarks>
/// This facade keeps the existing simple PDF generation path while adding renderer selection and validation.
/// </remarks>
[PublicAPI]
public sealed class PapercraftGenerator
{
    private readonly IPapercraftRenderer[] _renderers;

    /// <summary>
    /// Creates a new Papercraft generator.
    /// </summary>
    /// <param name="renderers">Registered renderer backends.</param>
    public PapercraftGenerator(IEnumerable<IPapercraftRenderer> renderers)
    {
        ArgumentNullException.ThrowIfNull(renderers);
        _renderers = renderers.ToArray();
        if (_renderers.Length is 0)
            throw new InvalidOperationException("No Papercraft renderer backend is registered.");
    }

    /// <summary>
    /// Template data used by the default renderer.
    /// </summary>
    public ITemplateData TemplateData
    {
        get
        {
            var renderer = _renderers.OfType<IPapercraftTemplateDataAccessor>().FirstOrDefault();
            return renderer?.TemplateData
                   ?? throw new InvalidOperationException(
                       "The selected Papercraft renderer does not expose template data.");
        }
    }

    /// <summary>
    /// Registered renderer backends.
    /// </summary>
    public IReadOnlyCollection<IPapercraftRenderer> Renderers => _renderers;

    /// <summary>
    /// Prepares a template for validation and rendering.
    /// </summary>
    public static Task<PreparedRenderDocument> PrepareAsync(
        XmlReader reader,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(cultureInfo);
        cancellationToken.ThrowIfCancellationRequested();

        var document = new XmlDocument { PreserveWhitespace = true };
        document.Load(reader);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            new PreparedRenderDocument(
                document.OuterXml,
                cultureInfo,
                options ?? PapercraftRenderOptions.Default));
    }

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
        ArgumentNullException.ThrowIfNull(target);
        var request = await PrepareAsync(reader, cultureInfo, options, cancellationToken)
            .ConfigureAwait(false);
        var renderer = SelectRenderer(target, request.Options);
        return await renderer.ValidateAsync(request, target, cancellationToken)
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
        ArgumentNullException.ThrowIfNull(output);
        var request = await PrepareAsync(reader, cultureInfo, options, cancellationToken)
            .ConfigureAwait(false);
        await RenderAsync(request, output, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Renders a prepared template to the supplied output.
    /// </summary>
    public async ValueTask RenderAsync(
        PreparedRenderDocument request,
        RenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(output);
        var renderer = SelectRenderer(output.Target, request.Options);
        var validation = await renderer.ValidateAsync(request, output.Target, cancellationToken)
            .ConfigureAwait(false);
        validation.ThrowIfUnsupportedOrStrictDegraded(request.Options.TreatDegradedAsUnsupported);
        await renderer.RenderAsync(request, output, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Generates a PDF document from the given template.
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

    private IPapercraftRenderer SelectRenderer(RenderTarget target, PapercraftRenderOptions options)
    {
        if (!options.RendererId.IsNullOrWhiteSpace())
        {
            var renderer = _renderers.FirstOrDefault(
                (q) => string.Equals(
                    q.Capabilities.RendererId,
                    options.RendererId,
                    StringComparison.OrdinalIgnoreCase));
            if (renderer is null)
                throw new InvalidOperationException($"No Papercraft renderer with id '{options.RendererId}' is registered.");
            return renderer;
        }

        return _renderers.FirstOrDefault((q) => q.Capabilities.Supports(target))
               ?? _renderers.First();
    }
}
