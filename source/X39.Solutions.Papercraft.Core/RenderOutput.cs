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
        : this(RenderTarget.FromMediaType(mediaType), stream, null)
    {
    }

    /// <summary>
    /// Creates a render output and infers the target from the supplied media type.
    /// </summary>
    /// <param name="mediaType">The output media type.</param>
    /// <param name="stream">The destination stream.</param>
    /// <param name="diagnosticSink">Receives non-fatal diagnostics produced during rendering.</param>
    public RenderOutput(string mediaType, Stream stream, Action<RenderDiagnostic> diagnosticSink)
        : this(RenderTarget.FromMediaType(mediaType), stream, diagnosticSink)
    {
    }

    /// <summary>
    /// Creates a render output for an explicit target.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <param name="stream">The destination stream.</param>
    public RenderOutput(RenderTarget target, Stream stream)
        : this(target, stream, null)
    {
    }

    /// <summary>
    /// Creates a render output for an explicit target.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <param name="stream">The destination stream.</param>
    /// <param name="diagnosticSink">Receives non-fatal diagnostics produced during rendering.</param>
    public RenderOutput(RenderTarget target, Stream stream, Action<RenderDiagnostic>? diagnosticSink)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(stream);
        Target = target;
        Stream = stream;
        DiagnosticSink = diagnosticSink;
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

    /// <summary>
    /// Receives non-fatal diagnostics produced during rendering.
    /// </summary>
    public Action<RenderDiagnostic>? DiagnosticSink { get; }
}
