namespace X39.Papercraft;

/// <summary>
/// Well-known renderer feature names used by validation diagnostics.
/// </summary>
[PublicAPI]
public static class RendererFeatures
{
    /// <summary>
    /// PDF output.
    /// </summary>
    public const string PdfOutput = "output.pdf";

    /// <summary>
    /// Raster image output.
    /// </summary>
    public const string RasterImageOutput = "output.raster-image";

    /// <summary>
    /// Multiple rendered pages in one document.
    /// </summary>
    public const string Multipage = "document.multipage";

    /// <summary>
    /// Text measuring support.
    /// </summary>
    public const string TextMeasurement = "text.measurement";

    /// <summary>
    /// Text drawing support.
    /// </summary>
    public const string TextDrawing = "text.drawing";

    /// <summary>
    /// Image drawing support.
    /// </summary>
    public const string Images = "image.drawing";

    /// <summary>
    /// Rectangular clipping support.
    /// </summary>
    public const string Clipping = "drawing.clipping";

    /// <summary>
    /// Alpha/transparency support.
    /// </summary>
    public const string Transparency = "drawing.transparency";

    /// <summary>
    /// Font family, style, width, and weight support.
    /// </summary>
    public const string Fonts = "text.fonts";

    /// <summary>
    /// Color output support.
    /// </summary>
    public const string Color = "drawing.color";

    /// <summary>
    /// Absolute positioning support.
    /// </summary>
    public const string AbsolutePositioning = "layout.absolute-positioning";
}
