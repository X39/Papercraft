namespace X39.Solutions.Papercraft;

/// <summary>
/// Describes how a renderer should write page-by-page raster output.
/// </summary>
public sealed record RasterPageRenderOutput
{
    /// <summary>
    /// Creates raster page output and infers the target from the supplied media type.
    /// </summary>
    /// <param name="mediaType">The raster page media type.</param>
    /// <param name="openPageStreamAsync">Callback used to open the destination stream for each page.</param>
    /// <param name="leaveStreamsOpen">Whether renderer-owned disposal should leave callback streams open.</param>
    public RasterPageRenderOutput(
        string mediaType,
        Func<RasterPageInfo, CancellationToken, ValueTask<Stream>> openPageStreamAsync,
        bool leaveStreamsOpen = false)
        : this(RenderTarget.FromMediaType(mediaType), openPageStreamAsync, leaveStreamsOpen)
    {
    }

    /// <summary>
    /// Creates raster page output for an explicit raster target.
    /// </summary>
    /// <param name="target">The raster render target.</param>
    /// <param name="openPageStreamAsync">Callback used to open the destination stream for each page.</param>
    /// <param name="leaveStreamsOpen">Whether renderer-owned disposal should leave callback streams open.</param>
    public RasterPageRenderOutput(
        RenderTarget target,
        Func<RasterPageInfo, CancellationToken, ValueTask<Stream>> openPageStreamAsync,
        bool leaveStreamsOpen = false)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(openPageStreamAsync);
        if (target.OutputKind is not RendererOutputKind.RasterImage)
            throw new ArgumentException("Raster page output requires a raster image target.", nameof(target));

        Target = target;
        OpenPageStreamAsync = openPageStreamAsync;
        LeaveStreamsOpen = leaveStreamsOpen;
    }

    /// <summary>
    /// The raster render target.
    /// </summary>
    public RenderTarget Target { get; }

    /// <summary>
    /// The raster page media type.
    /// </summary>
    public string MediaType => Target.MediaType;

    /// <summary>
    /// Callback used to open the destination stream for each page.
    /// </summary>
    public Func<RasterPageInfo, CancellationToken, ValueTask<Stream>> OpenPageStreamAsync { get; }

    /// <summary>
    /// Whether renderer-owned disposal should leave callback streams open.
    /// </summary>
    public bool LeaveStreamsOpen { get; }
}
