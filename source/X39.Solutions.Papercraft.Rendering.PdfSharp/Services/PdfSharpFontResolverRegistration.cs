using PdfSharp.Fonts;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

internal static class PdfSharpFontResolverRegistration
{
    private static int _configured;

    public static void EnsureConfigured()
    {
        if (Interlocked.Exchange(ref _configured, 1) is not 0)
            return;

        try
        {
            if (GlobalFontSettings.FontResolver is null
                && GlobalFontSettings.FallbackFontResolver is null)
            {
                GlobalFontSettings.FallbackFontResolver = PdfSharpSystemFontResolver.Instance;
            }
        }
        catch (InvalidOperationException)
        {
            // PDFsharp font settings become immutable after first font use.
        }
    }
}
