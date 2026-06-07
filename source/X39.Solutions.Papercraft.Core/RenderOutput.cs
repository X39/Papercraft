namespace X39.Solutions.Papercraft;

/// <summary>
/// Describes where a renderer should write its output.
/// </summary>
public sealed record RenderOutput
{
    /// <summary>
    /// Creates a render output and infers the target from the supplied media type.
    /// </summary>
    /// <param name="mediaType">The output media type.</param>
    /// <param name="stream">The destination stream.</param>
    public RenderOutput(string mediaType, Stream stream)
        : this(RenderTarget.FromMediaType(mediaType), stream)
    {
    }

    /// <summary>
    /// Creates a render output for an explicit target.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <param name="stream">The destination stream.</param>
    public RenderOutput(RenderTarget target, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(stream);
        Target = target;
        Stream = stream;
    }

    /// <summary>
    /// The render target.
    /// </summary>
    public RenderTarget Target { get; }

    /// <summary>
    /// The output media type.
    /// </summary>
    public string MediaType => Target.MediaType;

    /// <summary>
    /// The destination stream.
    /// </summary>
    public Stream Stream { get; }
}
