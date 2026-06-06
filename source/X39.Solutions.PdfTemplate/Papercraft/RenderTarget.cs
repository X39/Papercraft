namespace X39.Papercraft;

/// <summary>
/// Describes the desired render output target.
/// </summary>
/// <param name="MediaType">The output media type.</param>
/// <param name="OutputKind">The broad output kind.</param>
[PublicAPI]
public sealed record RenderTarget(string MediaType, RendererOutputKind OutputKind)
{
    /// <summary>
    /// A PDF render target.
    /// </summary>
    public static RenderTarget Pdf { get; } = new(PapercraftMediaTypes.ApplicationPdf, RendererOutputKind.Pdf);

    /// <summary>
    /// Creates a render target from a media type.
    /// </summary>
    /// <param name="mediaType">The output media type.</param>
    /// <returns>The inferred render target.</returns>
    public static RenderTarget FromMediaType(string mediaType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);
        var normalized = mediaType.Trim();
        if (string.Equals(normalized, PapercraftMediaTypes.ApplicationPdf, StringComparison.OrdinalIgnoreCase))
            return new RenderTarget(normalized, RendererOutputKind.Pdf);
        if (string.Equals(normalized, PapercraftMediaTypes.ImagePng, StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return new RenderTarget(normalized, RendererOutputKind.RasterImage);
        if (string.Equals(normalized, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
            return new RenderTarget(normalized, RendererOutputKind.VectorImage);
        return new RenderTarget(normalized, RendererOutputKind.Custom);
    }
}
