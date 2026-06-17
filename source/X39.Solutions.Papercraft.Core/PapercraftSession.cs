using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Services;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Isolated Papercraft template rendering session.
/// </summary>
/// <remarks>
/// Sessions own mutable template data and are not thread safe. Create one session for each isolated render workflow.
/// </remarks>
public sealed class PapercraftSession : IDisposable, IAsyncDisposable
{
    private readonly AsyncServiceScope _scope;
    private readonly PapercraftRenderPipeline _pipeline;
    private bool _disposed;

    private PapercraftSession(AsyncServiceScope scope, PapercraftRenderPipeline pipeline)
    {
        _scope = scope;
        _pipeline = pipeline;
    }

    /// <summary>
    /// Template data used by this session.
    /// </summary>
    public ITemplateData TemplateData
    {
        get
        {
            ThrowIfDisposed();
            return _pipeline.TemplateData;
        }
    }

    /// <summary>
    /// Registered render backends visible to this session.
    /// </summary>
    public IReadOnlyCollection<IPapercraftRenderBackend> Backends
    {
        get
        {
            ThrowIfDisposed();
            return _pipeline.Backends;
        }
    }

    internal static PapercraftSession Create(AsyncServiceScope scope)
    {
        var serviceProvider = scope.ServiceProvider;
        var pipeline = new PapercraftRenderPipeline(
            serviceProvider.GetRequiredService<PapercraftGenerator>(),
            serviceProvider.GetServices<IPapercraftRenderBackend>(),
            serviceProvider,
            serviceProvider.GetRequiredService<ControlActivationCache>(),
            serviceProvider.GetRequiredService<ControlRegistry>());
        return new PapercraftSession(scope, pipeline);
    }

    /// <summary>
    /// Validates a template against a render target.
    /// </summary>
    public ValueTask<RenderValidationResult> ValidateAsync(
        XmlReader reader,
        RenderTarget target,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _pipeline.ValidateAsync(reader, target, cultureInfo, options, cancellationToken);
    }

    /// <summary>
    /// Renders a template to an in-memory result for the supplied target.
    /// </summary>
    public async ValueTask<PapercraftRenderResult> RenderAsync(
        XmlReader reader,
        RenderTarget target,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(target);
        using var output = new MemoryStream();
        await _pipeline.RenderAsync(
                reader,
                new RenderOutput(target, output),
                cultureInfo,
                options,
                cancellationToken)
            .ConfigureAwait(false);
        return new PapercraftRenderResult(target, output.ToArray());
    }

    /// <summary>
    /// Renders a template to the supplied output.
    /// </summary>
    public ValueTask RenderAsync(
        XmlReader reader,
        RenderOutput output,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _pipeline.RenderAsync(reader, output, cultureInfo, options, cancellationToken);
    }

    /// <summary>
    /// Renders a template as page-by-page raster output.
    /// </summary>
    public ValueTask RenderRasterPagesAsync(
        XmlReader reader,
        RasterPageRenderOutput output,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _pipeline.RenderRasterPagesAsync(reader, output, cultureInfo, options, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _scope.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        await _scope.DisposeAsync()
            .ConfigureAwait(false);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PapercraftSession));
    }
}
