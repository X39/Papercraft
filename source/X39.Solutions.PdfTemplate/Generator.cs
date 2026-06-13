using System.Xml;
using SkiaSharp;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Compatibility wrapper for the legacy PDF template generator entry point.
/// </summary>
[PublicAPI]
public sealed class Generator : IDisposable, IAsyncDisposable
{
    private readonly PapercraftRenderer _renderer;
    private readonly Dictionary<string, object> _data = new();

    /// <summary>
    /// Creates a compatibility generator.
    /// </summary>
    public Generator(PapercraftRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _renderer = renderer;
    }

    /// <summary>
    /// The data available to the templates processed by this generator.
    /// </summary>
    public ITemplateData TemplateData => _renderer.TemplateData;

    /// <summary>
    /// Adds data to the generator, making it available for use in templates.
    /// </summary>
    public void AddData(string key, object data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(data);
        _data.Add(key, data);
        TemplateData.SetVariable(key, data);
    }

    /// <summary>
    /// Generates a PDF document from the given template reader.
    /// </summary>
    public Task GeneratePdfAsync(
        Stream outputStream,
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default,
        CancellationToken cancellationToken = default)
        => _renderer.GeneratePdfAsync(
            outputStream,
            reader,
            cultureInfo,
            CreateRenderOptions(documentOptions),
            cancellationToken);

    /// <summary>
    /// Writes the lowered XML produced from the supplied template before backend rendering.
    /// </summary>
    public Task GenerateLoweredXmlAsync(
        Stream outputStream,
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default,
        CancellationToken cancellationToken = default)
        => _renderer.GenerateLoweredXmlAsync(
            outputStream,
            reader,
            cultureInfo,
            CreateRenderOptions(documentOptions),
            cancellationToken);

    /// <summary>
    /// Generates SkiaSharp bitmaps from the given template reader.
    /// </summary>
    public async Task<IReadOnlyCollection<SKBitmap>> GenerateBitmapsAsync(
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        var pageStreams = new List<MemoryStream>();
        await _renderer.RenderRasterPagesAsync(
                reader,
                new RasterPageRenderOutput(
                    PapercraftMediaTypes.ImagePng,
                    (_, _) =>
                    {
                        var stream = new MemoryStream();
                        pageStreams.Add(stream);
                        return ValueTask.FromResult<Stream>(stream);
                    },
                    leaveStreamsOpen: true),
                cultureInfo,
                CreateRenderOptions(documentOptions),
                cancellationToken)
            .ConfigureAwait(false);

        var bitmaps = new List<SKBitmap>(pageStreams.Count);
        try
        {
            foreach (var pageStream in pageStreams)
            {
                pageStream.Position = 0;
                var bitmap = SKBitmap.Decode(pageStream)
                             ?? throw new InvalidOperationException("Papercraft raster output did not produce a decodable bitmap.");
                bitmaps.Add(bitmap);
            }

            return bitmaps.AsReadOnly();
        }
        catch
        {
            foreach (var bitmap in bitmaps)
            {
                bitmap.Dispose();
            }

            throw;
        }
        finally
        {
            foreach (var pageStream in pageStreams)
            {
                await pageStream.DisposeAsync()
                    .ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var (_, value) in _data)
        {
            if (value is IDisposable disposable)
                disposable.Dispose();
        }

        _data.Clear();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var (_, value) in _data)
        {
            switch (value)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync()
                        .ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        _data.Clear();
    }

    private static PapercraftRenderOptions CreateRenderOptions(DocumentOptions? documentOptions)
        => documentOptions is null
            ? PapercraftRenderOptions.Default
            : PapercraftRenderOptions.Default with { DocumentOptions = documentOptions.Value };
}
