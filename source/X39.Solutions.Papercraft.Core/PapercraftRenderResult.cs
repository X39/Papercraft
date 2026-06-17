using System.Text;

namespace X39.Solutions.Papercraft;

/// <summary>
/// In-memory output produced by a Papercraft render target.
/// </summary>
public sealed class PapercraftRenderResult
{
    private readonly byte[] _content;

    /// <summary>
    /// Creates a new render result.
    /// </summary>
    /// <param name="target">The render target that produced the output.</param>
    /// <param name="content">The rendered output bytes.</param>
    public PapercraftRenderResult(RenderTarget target, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(content);
        Target = target;
        _content = (byte[]) content.Clone();
    }

    /// <summary>
    /// The render target that produced the output.
    /// </summary>
    public RenderTarget Target { get; }

    /// <summary>
    /// The output media type.
    /// </summary>
    public string MediaType => Target.MediaType;

    /// <summary>
    /// The rendered output bytes.
    /// </summary>
    public ReadOnlyMemory<byte> Content => _content;

    /// <summary>
    /// The output byte length.
    /// </summary>
    public int Length => _content.Length;

    /// <summary>
    /// Opens the output bytes for reading.
    /// </summary>
    /// <returns>A readable stream over the rendered output.</returns>
    public Stream OpenRead()
        => new MemoryStream(_content, 0, _content.Length, writable: false, publiclyVisible: false);

    /// <summary>
    /// Copies the rendered output bytes into a new array.
    /// </summary>
    /// <returns>A copy of the rendered output.</returns>
    public byte[] ToArray()
        => _content.ToArray();

    /// <summary>
    /// Decodes the output bytes as text.
    /// </summary>
    /// <param name="encoding">The text encoding. Defaults to UTF-8.</param>
    /// <returns>The decoded text.</returns>
    public string ReadText(Encoding? encoding = null)
        => (encoding ?? Encoding.UTF8).GetString(_content);
}
