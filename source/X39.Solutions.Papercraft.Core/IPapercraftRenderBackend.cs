namespace X39.Solutions.Papercraft;

/// <summary>
/// Render backend contract for generated Papercraft documents.
/// </summary>
public interface IPapercraftRenderBackend
{
    /// <summary>
    /// Backend capabilities.
    /// </summary>
    RendererCapabilities Capabilities { get; }

    /// <summary>
    /// Validates whether this backend can render the generated document to the target.
    /// </summary>
    ValueTask<RenderValidationResult> ValidateAsync(
        PapercraftDocument document,
        RenderTarget target,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders the generated document to the output.
    /// </summary>
    ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders the generated document as page-by-page raster output.
    /// </summary>
    ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        CancellationToken cancellationToken = default);
}
