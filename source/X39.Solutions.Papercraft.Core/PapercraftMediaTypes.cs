namespace X39.Solutions.Papercraft;

/// <summary>
/// Common media type constants understood by Papercraft renderers.
/// </summary>
public static class PapercraftMediaTypes
{
    /// <summary>
    /// PDF document output.
    /// </summary>
    public const string ApplicationPdf = "application/pdf";

    /// <summary>
    /// PNG raster image output.
    /// </summary>
    public const string ImagePng = "image/png";

    /// <summary>
    /// Lowered Papercraft XML output.
    /// </summary>
    public const string ApplicationPapercraftLoweredXml = "application/vnd.papercraft.lowered+xml";
}
