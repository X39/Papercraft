using PdfSharp.Fonts;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

internal sealed class PdfSharpSystemFontResolver : IFontResolver
{
    private static readonly EnumerationOptions FontEnumerationOptions = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        MatchCasing = MatchCasing.CaseInsensitive,
    };

    private static readonly Lazy<IReadOnlyList<FontFace>> DiscoveredFonts = new(DiscoverFonts, true);

    public static PdfSharpSystemFontResolver Instance { get; } = new();

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
    {
        if (TryCreateFontFaceFromPath(familyName, out var fontFace))
            return ToResolverInfo(fontFace, bold, italic);

        var fonts = DiscoveredFonts.Value;
        if (fonts.Count is 0)
            return null;

        var preferredFamilies = GetPreferredFamilies(familyName).ToArray();
        foreach (var preferredFamily in preferredFamilies)
        {
            var match = SelectBestFace(
                fonts.Where((q) => q.NormalizedName.Contains(preferredFamily, StringComparison.OrdinalIgnoreCase)),
                bold,
                italic);
            if (match is not null)
                return ToResolverInfo(match, bold, italic);
        }

        var fallback = SelectBestFace(fonts, bold, italic);
        return fallback is null
            ? null
            : ToResolverInfo(fallback, bold, italic);
    }

    public byte[] GetFont(string faceName)
        => File.ReadAllBytes(faceName);

    private static FontResolverInfo ToResolverInfo(FontFace fontFace, bool bold, bool italic)
        => new(fontFace.Path, bold && !fontFace.IsBold, italic && !fontFace.IsItalic);

    private static FontFace? SelectBestFace(IEnumerable<FontFace> faces, bool bold, bool italic)
        => faces
            .OrderBy((q) => q.IsBold == bold ? 0 : 1)
            .ThenBy((q) => q.IsItalic == italic ? 0 : 1)
            .ThenBy((q) => q.Preference)
            .FirstOrDefault();

    private static IEnumerable<string> GetPreferredFamilies(string familyName)
    {
        var normalized = Normalize(familyName);
        if (normalized.Length > 0
            && normalized is not "sansserif"
            && normalized is not "serif"
            && normalized is not "monospace"
            && normalized is not "mono")
        {
            yield return normalized;
        }

        foreach (var knownFamily in GetKnownFamilyAliases(normalized))
            yield return knownFamily;

        if (normalized is "serif")
        {
            yield return "times";
            yield return "dejavuserif";
            yield return "liberationserif";
            yield return "notoserif";
            yield break;
        }

        if (normalized is "monospace" or "mono")
        {
            yield return "courier";
            yield return "consola";
            yield return "dejavusansmono";
            yield return "liberationmono";
            yield return "notosansmono";
            yield break;
        }

        yield return "arial";
        yield return "segoeui";
        yield return "dejavusans";
        yield return "liberationsans";
        yield return "notosans";
        yield return "freesans";
    }

    private static IEnumerable<string> GetKnownFamilyAliases(string normalized)
    {
        if (normalized.Contains("arial", StringComparison.OrdinalIgnoreCase))
            yield return "arial";
        if (normalized.Contains("segoe", StringComparison.OrdinalIgnoreCase))
            yield return "segoeui";
        if (normalized.Contains("times", StringComparison.OrdinalIgnoreCase))
            yield return "times";
        if (normalized.Contains("courier", StringComparison.OrdinalIgnoreCase))
            yield return "courier";
        if (normalized.Contains("courier", StringComparison.OrdinalIgnoreCase))
            yield return "cour";
        if (normalized.Contains("consola", StringComparison.OrdinalIgnoreCase))
            yield return "consola";
        if (normalized.Contains("dejavusans", StringComparison.OrdinalIgnoreCase))
            yield return "dejavusans";
        if (normalized.Contains("dejavuserif", StringComparison.OrdinalIgnoreCase))
            yield return "dejavuserif";
        if (normalized.Contains("liberationsans", StringComparison.OrdinalIgnoreCase))
            yield return "liberationsans";
        if (normalized.Contains("liberationserif", StringComparison.OrdinalIgnoreCase))
            yield return "liberationserif";
        if (normalized.Contains("liberationmono", StringComparison.OrdinalIgnoreCase))
            yield return "liberationmono";
        if (normalized.Contains("notosans", StringComparison.OrdinalIgnoreCase))
            yield return "notosans";
        if (normalized.Contains("notoserif", StringComparison.OrdinalIgnoreCase))
            yield return "notoserif";
    }

    private static IReadOnlyList<FontFace> DiscoverFonts()
    {
        var fonts = new List<FontFace>();
        foreach (var directory in GetFontDirectories())
        {
            if (!Directory.Exists(directory))
                continue;

            try
            {
                foreach (var path in Directory.EnumerateFiles(directory, "*", FontEnumerationOptions))
                {
                    if (IsFontFile(path))
                        fonts.Add(FontFace.FromPath(path));
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return fonts
            .GroupBy((q) => q.Path, StringComparer.OrdinalIgnoreCase)
            .Select((q) => q.First())
            .OrderBy((q) => q.Preference)
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

            fontFace = FontFace.FromPath(path);
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

    private static IEnumerable<string> GetFontDirectories()
    {
        if (OperatingSystem.IsWindows())
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");

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
        }

        if (OperatingSystem.IsMacOS())
        {
            yield return "/System/Library/Fonts";
            yield return "/Library/Fonts";
            yield return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library",
                "Fonts");
        }
    }

    private static bool IsFontFile(string path)
    {
        var extension = Path.GetExtension(path);
        return string.Equals(extension, ".ttf", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extension, ".otf", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extension, ".ttc", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string value)
        => new(
            value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());

    private sealed record FontFace(
        string Path,
        string NormalizedName,
        bool IsBold,
        bool IsItalic,
        int Preference)
    {
        public static FontFace FromPath(string path)
        {
            var name = Normalize(System.IO.Path.GetFileNameWithoutExtension(path));
            return new FontFace(path, name, IsBoldName(name), IsItalicName(name), GetPreference(name));
        }

        private static bool IsBoldName(string name)
            => name.Contains("bold", StringComparison.OrdinalIgnoreCase)
               || name.Contains("black", StringComparison.OrdinalIgnoreCase)
               || name.Contains("semibold", StringComparison.OrdinalIgnoreCase)
               || name.EndsWith("bd", StringComparison.OrdinalIgnoreCase)
               || name.EndsWith("bi", StringComparison.OrdinalIgnoreCase);

        private static bool IsItalicName(string name)
            => name.Contains("italic", StringComparison.OrdinalIgnoreCase)
               || name.Contains("oblique", StringComparison.OrdinalIgnoreCase)
               || name.EndsWith("bi", StringComparison.OrdinalIgnoreCase)
               || (name.EndsWith("i", StringComparison.OrdinalIgnoreCase)
                   && !name.EndsWith("ui", StringComparison.OrdinalIgnoreCase));

        private static int GetPreference(string name)
        {
            if (name.Contains("arial", StringComparison.OrdinalIgnoreCase))
                return 0;
            if (name.Contains("segoeui", StringComparison.OrdinalIgnoreCase))
                return 1;
            if (name.Contains("dejavusans", StringComparison.OrdinalIgnoreCase))
                return 2;
            if (name.Contains("liberationsans", StringComparison.OrdinalIgnoreCase))
                return 3;
            if (name.Contains("notosans", StringComparison.OrdinalIgnoreCase))
                return 4;
            return 100;
        }
    }
}
