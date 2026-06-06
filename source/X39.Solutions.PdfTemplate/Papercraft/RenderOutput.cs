namespace X39.Papercraft;

/// <summary>
/// Describes where a renderer should write its output.
/// </summary>
/// <param name="MediaType">The output media type.</param>
/// <param name="Stream">The destination stream.</param>
[PublicAPI]
public sealed record RenderOutput(string MediaType, Stream Stream)
{
    /// <summary>
    /// The render target inferred from <see cref="MediaType"/>.
    /// </summary>
    public RenderTarget Target => RenderTarget.FromMediaType(MediaType);
}
