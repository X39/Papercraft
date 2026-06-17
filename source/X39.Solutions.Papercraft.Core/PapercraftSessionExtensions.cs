using System.Xml;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Convenience helpers for common Papercraft session render targets.
/// </summary>
public static class PapercraftSessionExtensions
{
    /// <summary>
    /// Renders a template to an in-memory PDF result.
    /// </summary>
    public static ValueTask<PapercraftRenderResult> RenderPdfAsync(
        this PapercraftSession session,
        XmlReader reader,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.RenderAsync(reader, RenderTarget.Pdf, cultureInfo, options, cancellationToken);
    }

    /// <summary>
    /// Renders a template as PDF to the supplied output stream.
    /// </summary>
    public static ValueTask RenderPdfAsync(
        this PapercraftSession session,
        XmlReader reader,
        Stream outputStream,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(outputStream);
        return session.RenderAsync(
            reader,
            new RenderOutput(RenderTarget.Pdf, outputStream),
            cultureInfo,
            options,
            cancellationToken);
    }

    /// <summary>
    /// Generates a PDF document from the supplied template.
    /// </summary>
    public static async Task GeneratePdfAsync(
        this PapercraftSession session,
        Stream outputStream,
        XmlReader reader,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await session.RenderPdfAsync(reader, outputStream, cultureInfo, options, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Renders a template to an in-memory lowered XML result.
    /// </summary>
    public static ValueTask<PapercraftRenderResult> RenderLoweredXmlAsync(
        this PapercraftSession session,
        XmlReader reader,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.RenderAsync(reader, RenderTarget.LoweredXml, cultureInfo, options, cancellationToken);
    }

    /// <summary>
    /// Renders a template as lowered XML to the supplied output stream.
    /// </summary>
    public static ValueTask RenderLoweredXmlAsync(
        this PapercraftSession session,
        XmlReader reader,
        Stream outputStream,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(outputStream);
        return session.RenderAsync(
            reader,
            new RenderOutput(RenderTarget.LoweredXml, outputStream),
            cultureInfo,
            options,
            cancellationToken);
    }

    /// <summary>
    /// Writes the lowered XML produced from the supplied template before backend rendering.
    /// </summary>
    public static async Task GenerateLoweredXmlAsync(
        this PapercraftSession session,
        Stream outputStream,
        XmlReader reader,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await session.RenderLoweredXmlAsync(reader, outputStream, cultureInfo, options, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Renders lowered XML and decodes it as text.
    /// </summary>
    public static async Task<string> ReadLoweredXmlAsync(
        this PapercraftSession session,
        XmlReader reader,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        var result = await session.RenderLoweredXmlAsync(reader, cultureInfo, options, cancellationToken)
            .ConfigureAwait(false);
        return result.ReadText();
    }
}
