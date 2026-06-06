namespace X39.Papercraft;

/// <summary>
/// Renderer backend contract for Papercraft.
/// </summary>
[PublicAPI]
public interface IPapercraftRenderer
{
    /// <summary>
    /// Renderer capabilities.
    /// </summary>
    RendererCapabilities Capabilities { get; }

    /// <summary>
    /// Validates whether this renderer can render the prepared document to the target.
    /// </summary>
    ValueTask<RenderValidationResult> ValidateAsync(
        PreparedRenderDocument request,
        RenderTarget target,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders the prepared document to the output.
    /// </summary>
    ValueTask RenderAsync(
        PreparedRenderDocument request,
        RenderOutput output,
        CancellationToken cancellationToken = default);
}
