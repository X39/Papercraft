using System.Runtime.Versioning;
using System.Security;
using Microsoft.Win32;
using PdfSharp.Fonts;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

internal sealed class PdfSharpSystemFontResolver : IFontResolver
{
    internal const string GenericSansSerifFamily = "sans-serif";

    private readonly IReadOnlyList<FontFace>? _fonts;

    private static readonly EnumerationOptions FontEnumerationOptions = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        MatchCasing = MatchCasing.CaseInsensitive,
    };

    private static readonly Lazy<IReadOnlyList<FontFace>> DiscoveredFonts = new(DiscoverFonts, true);

    public static PdfSharpSystemFontResolver Instance { get; } = new();

    public PdfSharpSystemFontResolver()
    {
    }

    internal PdfSharpSystemFontResolver(IEnumerable<PdfSharpFontFaceDescriptor> fontFaces)
    {
        ArgumentNullException.ThrowIfNull(fontFaces);
        _fonts = fontFaces.Select(FontFace.FromDescriptor).ToArray();
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        => ResolveTypefaceDetailed(familyName, bold, italic, reportDiagnostics: true).FontResolverInfo;

    public byte[] GetFont(string faceName)
        => File.ReadAllBytes(faceName);

    internal FontResolutionResult ResolveTypefaceDetailed(
        string familyName,
        bool bold,
        bool italic,
        bool reportDiagnostics)
    {
        var diagnostics = new List<RenderDiagnostic>();
        if (TryCreateFontFaceFromPath(familyName, out var pathFontFace))
        {
            AddStyleSubstitutionDiagnostic(diagnostics, familyName, pathFontFace, bold, italic);
            ReportDiagnostics(diagnostics, reportDiagnostics);
            return new FontResolutionResult(ToResolverInfo(pathFontFace, italic), diagnostics);
        }

        var fonts = _fonts ?? DiscoveredFonts.Value;
        if (fonts.Count is 0)
        {
            diagnostics.Add(CreateMissingFontDiagnostic(familyName, null));
            ReportDiagnostics(diagnostics, reportDiagnostics);
            return new FontResolutionResult(null, diagnostics);
        }

        var normalizedFamily = NormalizeRequest(familyName);
        var candidates = GetCandidateFaces(fonts, normalizedFamily).ToArray();
        if (candidates.Length is not 0)
        {
            var match = SelectBestFace(candidates, bold, italic);
            if (match is not null)
            {
                AddStyleSubstitutionDiagnostic(diagnostics, familyName, match, bold, italic);
                ReportDiagnostics(diagnostics, reportDiagnostics);
                return new FontResolutionResult(ToResolverInfo(match, italic), diagnostics);
            }
        }

        var fallback = SelectDefaultFace(fonts, bold, italic);
        if (fallback is null)
        {
            diagnostics.Add(CreateMissingFontDiagnostic(familyName, null));
            ReportDiagnostics(diagnostics, reportDiagnostics);
            return new FontResolutionResult(null, diagnostics);
        }

        diagnostics.Add(CreateMissingFontDiagnostic(familyName, fallback));
        AddStyleSubstitutionDiagnostic(diagnostics, familyName, fallback, bold, italic);
        ReportDiagnostics(diagnostics, reportDiagnostics);
        return new FontResolutionResult(ToResolverInfo(fallback, italic), diagnostics);
    }

    internal RenderValidationResult ValidateDocumentFonts(PapercraftDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var diagnostics = new List<RenderDiagnostic>();
        var seen = new HashSet<FontRequest>();
        foreach (var text in document.Pages.SelectMany((q) => q.DisplayList.Commands).OfType<DrawTextCommand>())
        {
            var request = new FontRequest(
                text.TextStyle.FontFamily.Family,
                text.TextStyle.FontFamily.Weight >= 600,
                text.TextStyle.FontFamily.Style is DisplayFontStyle.Italic or DisplayFontStyle.Oblique);
            if (!seen.Add(request))
                continue;

            diagnostics.AddRange(
                ResolveTypefaceDetailed(
                        request.FamilyName,
                        request.Bold,
                        request.Italic,
                        reportDiagnostics: false)
                    .Diagnostics);
        }

        return diagnostics.Count is 0
            ? RenderValidationResult.Supported
            : new RenderValidationResult(diagnostics.Distinct());
    }

    private static IEnumerable<FontFace> GetCandidateFaces(IEnumerable<FontFace> fonts, string normalizedFamily)
    {
        if (IsGenericSansSerif(normalizedFamily))
            return fonts.Where((q) => q.GenericFamily is PdfSharpFontGenericFamily.SansSerif);
        if (normalizedFamily is "serif")
            return fonts.Where((q) => q.GenericFamily is PdfSharpFontGenericFamily.Serif);
        if (normalizedFamily is "monospace" or "mono")
            return fonts.Where((q) => q.GenericFamily is PdfSharpFontGenericFamily.Monospace);

        return fonts.Where((q) => q.MatchesFamily(normalizedFamily));
    }

    private static FontFace? SelectDefaultFace(IReadOnlyCollection<FontFace> fonts, bool bold, bool italic)
        => SelectBestFace(
               fonts.Where((q) => q.GenericFamily is PdfSharpFontGenericFamily.SansSerif),
               bold,
               italic)
           ?? SelectBestFace(fonts, bold, italic);

    private static FontFace? SelectBestFace(IEnumerable<FontFace> faces, bool bold, bool italic)
    {
        var targetWeight = bold ? 700 : 400;
        return faces
            .OrderBy((q) => q.IsItalic == italic ? 0 : 1)
            .ThenBy((q) => Math.Abs(q.Weight - targetWeight))
            .ThenBy((q) => Math.Abs(q.Weight - 400))
            .ThenBy((q) => q.DisplayFamilyName, StringComparer.OrdinalIgnoreCase)
            .ThenBy((q) => q.DisplayFaceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy((q) => q.Path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static FontResolverInfo ToResolverInfo(FontFace fontFace, bool italic)
        => new(fontFace.Path, false, italic && !fontFace.IsItalic);

    private static void AddStyleSubstitutionDiagnostic(
        ICollection<RenderDiagnostic> diagnostics,
        string requestedFamily,
        FontFace fontFace,
        bool bold,
        bool italic)
    {
        if (fontFace.IsItalic == italic
            && (bold ? fontFace.Weight >= 600 : fontFace.Weight < 600))
        {
            return;
        }

        diagnostics.Add(
            new RenderDiagnostic(
                RenderDiagnosticCodes.FontFaceSubstitution,
                RendererSupportLevel.Degraded,
                RendererFeatures.Fonts,
                $"Font family '{GetRequestLabel(requestedFamily)}' does not provide {GetStyleLabel(bold, italic)}. Substituted '{fontFace.DisplayFaceName}'.",
                "Install the requested font face or use a font family with the requested style."));
    }

    private static RenderDiagnostic CreateMissingFontDiagnostic(string requestedFamily, FontFace? fallback)
    {
        var substitute = fallback is null
            ? "No substitute font is available."
            : $"Substituted '{fallback.DisplayFamilyName}'.";
        return new RenderDiagnostic(
            RenderDiagnosticCodes.MissingFontSubstitution,
            fallback is null ? RendererSupportLevel.Unsupported : RendererSupportLevel.Degraded,
            RendererFeatures.Fonts,
            $"Font family '{GetRequestLabel(requestedFamily)}' was not found. {substitute}",
            "Install the requested font or choose an installed font family.");
    }

    private static void ReportDiagnostics(IEnumerable<RenderDiagnostic> diagnostics, bool reportDiagnostics)
    {
        if (reportDiagnostics)
            RenderDiagnosticScope.Report(diagnostics);
    }

    private static string GetStyleLabel(bool bold, bool italic)
        => (bold, italic) switch
        {
            (true, true)   => "a bold italic face",
            (true, false)  => "a bold face",
            (false, true)  => "an italic face",
            (false, false) => "a regular face",
        };

    private static string GetRequestLabel(string requestedFamily)
        => string.IsNullOrWhiteSpace(requestedFamily)
            ? GenericSansSerifFamily
            : requestedFamily;

    private static string NormalizeRequest(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "sansserif";

        var normalized = PdfSharpFontFaceClassifier.Normalize(value);
        return normalized.Length is 0 ? "sansserif" : normalized;
    }

    private static bool IsGenericSansSerif(string normalizedFamily)
        => normalizedFamily is "sansserif" or "sans";

    private static IReadOnlyList<FontFace> DiscoverFonts()
    {
        var fonts = new List<FontFace>();
        foreach (var path in GetFontPaths().Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                if (!File.Exists(path) || !IsFontFile(path))
                    continue;

                var descriptor = PdfSharpFontFaceClassifier.TryClassify(path);
                if (descriptor is not null)
                    fonts.Add(FontFace.FromDescriptor(descriptor));
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return fonts
            .GroupBy((q) => q.Path, StringComparer.OrdinalIgnoreCase)
            .Select((q) => q.First())
            .ToArray();
    }

    private static bool TryCreateFontFaceFromPath(string familyName, out FontFace fontFace)
    {
        fontFace = default!;
        if (string.IsNullOrWhiteSpace(familyName)
            || !IsFontFile(familyName))
        {
            return false;
        }

        try
        {
            var path = Path.GetFullPath(familyName);
            if (!File.Exists(path))
                return false;

            fontFace = FontFace.FromDescriptor(PdfSharpFontFaceClassifier.Classify(path));
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    private static IEnumerable<string> GetFontPaths()
    {
        foreach (var directory in GetFontDirectories())
        {
            if (!Directory.Exists(directory))
                continue;

            IEnumerable<string> paths;
            try
            {
                paths = Directory.EnumerateFiles(directory, "*", FontEnumerationOptions).ToArray();
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var path in paths)
            {
                if (IsFontFile(path))
                    yield return path;
            }
        }

        if (!OperatingSystem.IsWindows())
            yield break;

        foreach (var path in GetWindowsRegistryFontPaths())
        {
            if (IsFontFile(path))
                yield return path;
        }
    }

    private static IEnumerable<string> GetFontDirectories()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            yield return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft",
                "Windows",
                "Fonts");
        }

        if (OperatingSystem.IsLinux())
        {
            yield return "/usr/share/fonts";
            yield return "/usr/local/share/fonts";
            yield return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".fonts");
            yield return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local",
                "share",
                "fonts");

            var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (!string.IsNullOrWhiteSpace(xdgDataHome))
                yield return Path.Combine(xdgDataHome, "fonts");
        }

        if (OperatingSystem.IsMacOS())
        {
            yield return "/System/Library/Fonts";
            yield return "/System/Library/Fonts/Supplemental";
            yield return "/Library/Fonts";
            yield return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library",
                "Fonts");
        }
    }

    [SupportedOSPlatform("windows")]
    private static IReadOnlyCollection<string> GetWindowsRegistryFontPaths()
    {
        var paths = new List<string>();
        var windowsFontsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
        foreach (var root in new[] { Registry.LocalMachine, Registry.CurrentUser })
        {
            try
            {
                using var key = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");
                if (key is null)
                    continue;

                foreach (var valueName in key.GetValueNames())
                {
                    if (key.GetValue(valueName) is not string value || string.IsNullOrWhiteSpace(value))
                        continue;

                    paths.Add(Path.IsPathRooted(value) ? value : Path.Combine(windowsFontsDirectory, value));
                }
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return paths;
    }

    private static bool IsFontFile(string path)
    {
        var extension = Path.GetExtension(path);
        return string.Equals(extension, ".ttf", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extension, ".otf", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extension, ".ttc", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extension, ".otc", StringComparison.OrdinalIgnoreCase);
    }

    internal sealed record FontResolutionResult(
        FontResolverInfo? FontResolverInfo,
        IReadOnlyCollection<RenderDiagnostic> Diagnostics);

    private sealed record FontRequest(string FamilyName, bool Bold, bool Italic);

    private sealed record FontFace(
        string Path,
        string DisplayFamilyName,
        string DisplayFaceName,
        IReadOnlyCollection<string> NormalizedFamilyNames,
        IReadOnlyCollection<string> NormalizedFaceNames,
        int Weight,
        bool IsItalic,
        PdfSharpFontGenericFamily GenericFamily)
    {
        public static FontFace FromDescriptor(PdfSharpFontFaceDescriptor descriptor)
            => new(
                descriptor.Path,
                descriptor.DisplayFamilyName,
                descriptor.DisplayFaceName,
                descriptor.NormalizedFamilyNames,
                descriptor.NormalizedFaceNames,
                descriptor.Weight,
                descriptor.IsItalic,
                descriptor.GenericFamily);

        public bool MatchesFamily(string normalizedFamily)
            => NormalizedFamilyNames.Any((q) => string.Equals(q, normalizedFamily, StringComparison.OrdinalIgnoreCase))
               || NormalizedFaceNames.Any((q) => string.Equals(q, normalizedFamily, StringComparison.OrdinalIgnoreCase));
    }
}
