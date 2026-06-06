namespace X39.Papercraft;

/// <summary>
/// Describes the broad output category produced by a renderer.
/// </summary>
[Flags]
[PublicAPI]
public enum RendererOutputKind
{
    /// <summary>
    /// No output kind.
    /// </summary>
    None = 0,

    /// <summary>
    /// PDF output.
    /// </summary>
    Pdf = 1,

    /// <summary>
    /// Raster image output.
    /// </summary>
    RasterImage = 2,

    /// <summary>
    /// Vector image output.
    /// </summary>
    VectorImage = 4,

    /// <summary>
    /// Printer command output.
    /// </summary>
    PrinterCommands = 8,

    /// <summary>
    /// Backend-defined custom output.
    /// </summary>
    Custom = 16,
}
