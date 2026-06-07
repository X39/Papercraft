namespace X39.Solutions.Papercraft;

/// <summary>
/// Metadata for one rendered raster page.
/// </summary>
public sealed record RasterPageInfo
{
    /// <summary>
    /// Creates raster page metadata.
    /// </summary>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="mediaType">Encoded page media type.</param>
    /// <param name="pixelWidth">Encoded page width in pixels.</param>
    /// <param name="pixelHeight">Encoded page height in pixels.</param>
    /// <param name="dotsPerMillimeter">Raster resolution in dots per millimeter.</param>
    public RasterPageInfo(
        int pageIndex,
        int pageNumber,
        string mediaType,
        int pixelWidth,
        int pixelHeight,
        float dotsPerMillimeter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);
        if (pageIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "Page index must be zero or greater.");
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "Page number must be one or greater.");
        if (pixelWidth < 1)
            throw new ArgumentOutOfRangeException(nameof(pixelWidth), pixelWidth, "Pixel width must be one or greater.");
        if (pixelHeight < 1)
            throw new ArgumentOutOfRangeException(nameof(pixelHeight), pixelHeight, "Pixel height must be one or greater.");
        if (dotsPerMillimeter <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(dotsPerMillimeter),
                dotsPerMillimeter,
                "Dots per millimeter must be greater than zero.");

        PageIndex = pageIndex;
        PageNumber = pageNumber;
        MediaType = mediaType.Trim();
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        DotsPerMillimeter = dotsPerMillimeter;
    }

    /// <summary>
    /// Zero-based page index.
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// One-based page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Encoded page media type.
    /// </summary>
    public string MediaType { get; }

    /// <summary>
    /// Encoded page width in pixels.
    /// </summary>
    public int PixelWidth { get; }

    /// <summary>
    /// Encoded page height in pixels.
    /// </summary>
    public int PixelHeight { get; }

    /// <summary>
    /// Raster resolution in dots per millimeter.
    /// </summary>
    public float DotsPerMillimeter { get; }
}
