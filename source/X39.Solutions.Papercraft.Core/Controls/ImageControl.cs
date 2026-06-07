using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services;
using X39.Solutions.Papercraft.Services.ResourceResolver;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// A control that draws an image.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class ImageControl : AlignableControl, IInitializeControlAsync, IDisposable
{
    private readonly IResourceResolver _resourceResolver;

    /// <summary>
    /// Creates a new <see cref="ImageControl"/>.
    /// </summary>
    /// <param name="resourceResolver">The <see cref="IResourceResolver"/> to use.</param>
    public ImageControl(IResourceResolver resourceResolver)
    {
        _resourceResolver = resourceResolver;
    }

    /// <summary>
    /// The source of the image to draw.
    /// </summary>
    /// <remarks>
    /// This always has to be resolved, using a <see cref="IResourceResolver"/>.
    /// The default implementation of <see cref="IResourceResolver"/> is <see cref="DefaultResourceResolver"/>,
    /// accepting only accept base64 encoded images for security reasons!
    /// Make sure to provide your own <see cref="IResourceResolver"/> if you
    /// want to use other sources, like a file path.
    /// </remarks>
    [Parameter]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// The width of the image.
    /// </summary>
    [Parameter]
    public Length Width { get; set; } = new();

    /// <summary>
    /// The height of the image.
    /// </summary>
    [Parameter]
    public Length Height { get; set; } = new();

    private byte[]? _imageBytes;
    private Size    _imageSize;

    /// <inheritdoc />
    public async Task InitializeControlAsync(object? context, CancellationToken cancellationToken = default)
    {
        _imageBytes = null;
        _imageSize  = Size.Zero;

        var image = await _resourceResolver.ResolveImageAsync(Source, context, cancellationToken)
            .ConfigureAwait(false);
        var imageSize = EncodedImageSizeReader.GetSize(image);
        _imageBytes = image.ToArray();
        _imageSize  = imageSize;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _imageBytes = null;
        _imageSize  = Size.Zero;
    }

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var width = Width.ToPixels(framedPageSize.Width, dpi);
        var bitmapWidth = _imageSize.Width;
        var height = Height.ToPixels(framedPageSize.Height, dpi);
        var bitmapHeight = _imageSize.Height;
        return new Size(
            Width.Unit is ELengthUnit.Auto
                ? Height.Unit is ELengthUnit.Auto
                    ? bitmapWidth
                    : bitmapWidth / bitmapHeight * height
                : width,
            Height.Unit is ELengthUnit.Auto
                ? Width.Unit is ELengthUnit.Auto
                    ? bitmapHeight
                    : bitmapHeight / bitmapWidth * width
                : height
        );
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var width = Width.ToPixels(framedPageSize.Width, dpi);
        var bitmapWidth = _imageSize.Width;
        var height = Height.ToPixels(framedPageSize.Height, dpi);
        var bitmapHeight = _imageSize.Height;
        return new Size(
            Width.Unit is ELengthUnit.Auto
                ? Height.Unit is ELengthUnit.Auto
                    ? bitmapWidth
                    : bitmapWidth / bitmapHeight * height
                : width,
            Height.Unit is ELengthUnit.Auto
                ? Width.Unit is ELengthUnit.Auto
                    ? bitmapHeight
                    : bitmapHeight / bitmapWidth * width
                : height
        );
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        if (_imageBytes is null)
            return Size.Zero;
        canvas.DrawImage(_imageBytes, new Rectangle(0, 0, ArrangementInner.Width, ArrangementInner.Height));
        return Size.Zero;
    }
}
