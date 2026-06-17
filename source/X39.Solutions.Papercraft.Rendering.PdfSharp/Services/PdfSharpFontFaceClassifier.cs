using System.Text;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

internal static class PdfSharpFontFaceClassifier
{
    private const int RegularWeight = 400;
    private const int BoldWeight = 700;

    public static PdfSharpFontFaceDescriptor Classify(string path)
        => TryClassify(path)
           ?? new PdfSharpFontFaceDescriptor(
               path,
               Path.GetFileNameWithoutExtension(path),
               Path.GetFileNameWithoutExtension(path),
               Array.Empty<string>(),
               Array.Empty<string>(),
               RegularWeight,
               false,
               PdfSharpFontGenericFamily.Unknown);

    public static PdfSharpFontFaceDescriptor? TryClassify(string path)
    {
        var metadata = TryReadMetadata(path);
        if (metadata is null)
            return null;

        var familyNames = metadata.TypographicFamilies.Count is not 0
            ? metadata.TypographicFamilies.Concat(metadata.Families)
            : metadata.Families;
        var displayFamilyName = familyNames.FirstOrDefault((q) => !string.IsNullOrWhiteSpace(q))
                                ?? metadata.FullNames.FirstOrDefault((q) => !string.IsNullOrWhiteSpace(q))
                                ?? Path.GetFileNameWithoutExtension(path);
        var displayFaceName = metadata.FullNames.FirstOrDefault((q) => !string.IsNullOrWhiteSpace(q))
                              ?? displayFamilyName;
        var styleTexts = metadata.TypographicSubfamilies
            .Concat(metadata.Subfamilies)
            .Concat(metadata.FullNames)
            .ToArray();
        var weight = NormalizeWeight(
            metadata.Os2?.Weight
            ?? GetWeightFromStyleTexts(styleTexts)
            ?? (metadata.Head?.IsBold is true ? BoldWeight : RegularWeight));
        var isItalic = metadata.Os2?.IsItalic is true
                       || metadata.Os2?.IsOblique is true
                       || metadata.Head?.IsItalic is true
                       || metadata.Post?.IsItalic is true
                       || ContainsItalicText(styleTexts);

        var normalizedFamilyNames = NormalizeDistinct(familyNames);
        var normalizedFaceNames = NormalizeDistinct(metadata.FullNames.Concat(metadata.PostScriptNames));
        if (normalizedFamilyNames.Count is 0 && normalizedFaceNames.Count is 0)
            return null;

        return new PdfSharpFontFaceDescriptor(
            path,
            displayFamilyName,
            displayFaceName,
            normalizedFamilyNames,
            normalizedFaceNames,
            weight,
            isItalic,
            GetGenericFamily(metadata.Os2, metadata.Post));
    }

    internal static string Normalize(string value)
        => new(
            value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());

    private static FontMetadata? TryReadMetadata(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            using var stream = File.OpenRead(path);
            var fontOffset = GetFirstFontOffset(stream);
            var tables = ReadTableDirectory(stream, fontOffset);
            if (!tables.TryGetValue("name", out var nameTable))
                return null;

            var names = ReadNameTable(stream, nameTable);
            return new FontMetadata(
                names.Families,
                names.Subfamilies,
                names.FullNames,
                names.TypographicFamilies,
                names.TypographicSubfamilies,
                names.PostScriptNames,
                tables.TryGetValue("OS/2", out var os2Table) ? ReadOs2Table(stream, os2Table) : null,
                tables.TryGetValue("head", out var headTable) ? ReadHeadTable(stream, headTable) : null,
                tables.TryGetValue("post", out var postTable) ? ReadPostTable(stream, postTable) : null);
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (EndOfStreamException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static uint GetFirstFontOffset(Stream stream)
    {
        stream.Position = 0;
        var tag = ReadTag(stream);
        if (!string.Equals(tag, "ttcf", StringComparison.Ordinal))
            return 0;

        _ = ReadUInt32(stream);
        var fontCount = ReadUInt32(stream);
        return fontCount is 0 ? 0 : ReadUInt32(stream);
    }

    private static Dictionary<string, TableRecord> ReadTableDirectory(Stream stream, uint fontOffset)
    {
        stream.Position = fontOffset + 4;
        var tableCount = ReadUInt16(stream);
        stream.Position += 6;

        var tables = new Dictionary<string, TableRecord>(StringComparer.Ordinal);
        for (var i = 0; i < tableCount; i++)
        {
            var tag = ReadTag(stream);
            _ = ReadUInt32(stream);
            var offset = ReadUInt32(stream);
            var length = ReadUInt32(stream);
            tables[tag] = new TableRecord(offset, length);
        }

        return tables;
    }

    private static NameTable ReadNameTable(Stream stream, TableRecord table)
    {
        stream.Position = table.Offset;
        _ = ReadUInt16(stream);
        var count = ReadUInt16(stream);
        var stringOffset = ReadUInt16(stream);
        var records = new NameRecord[count];
        for (var i = 0; i < count; i++)
        {
            records[i] = new NameRecord(
                ReadUInt16(stream),
                ReadUInt16(stream),
                ReadUInt16(stream),
                ReadUInt16(stream),
                ReadUInt16(stream),
                ReadUInt16(stream));
        }

        var families = new List<string>();
        var subfamilies = new List<string>();
        var fullNames = new List<string>();
        var typographicFamilies = new List<string>();
        var typographicSubfamilies = new List<string>();
        var postScriptNames = new List<string>();
        foreach (var record in records)
        {
            if (record.NameId is not (1 or 2 or 4 or 6 or 16 or 17))
                continue;

            var text = ReadNameString(stream, table.Offset + stringOffset + record.Offset, record.Length, record);
            if (string.IsNullOrWhiteSpace(text))
                continue;

            switch (record.NameId)
            {
                case 1:
                    families.Add(text);
                    break;
                case 2:
                    subfamilies.Add(text);
                    break;
                case 4:
                    fullNames.Add(text);
                    break;
                case 6:
                    postScriptNames.Add(text);
                    break;
                case 16:
                    typographicFamilies.Add(text);
                    break;
                case 17:
                    typographicSubfamilies.Add(text);
                    break;
            }
        }

        return new NameTable(
            Distinct(families),
            Distinct(subfamilies),
            Distinct(fullNames),
            Distinct(typographicFamilies),
            Distinct(typographicSubfamilies),
            Distinct(postScriptNames));
    }

    private static Os2Table? ReadOs2Table(Stream stream, TableRecord table)
    {
        if (table.Length < 64)
            return null;

        stream.Position = table.Offset + 4;
        var weight = ReadUInt16(stream);
        _ = ReadUInt16(stream);

        stream.Position = table.Offset + 30;
        var familyClass = ReadUInt16(stream);
        var panose = new byte[10];
        var read = stream.Read(panose, 0, panose.Length);
        if (read != panose.Length)
            throw new EndOfStreamException();

        stream.Position = table.Offset + 62;
        var selection = ReadUInt16(stream);
        return new Os2Table(
            weight,
            familyClass,
            panose,
            (selection & 0x0001) is not 0,
            (selection & 0x0020) is not 0,
            (selection & 0x0040) is not 0,
            (selection & 0x0200) is not 0);
    }

    private static HeadTable? ReadHeadTable(Stream stream, TableRecord table)
    {
        if (table.Length < 46)
            return null;

        stream.Position = table.Offset + 44;
        var macStyle = ReadUInt16(stream);
        return new HeadTable(
            (macStyle & 0x0001) is not 0,
            (macStyle & 0x0002) is not 0);
    }

    private static PostTable? ReadPostTable(Stream stream, TableRecord table)
    {
        if (table.Length < 16)
            return null;

        stream.Position = table.Offset + 4;
        var italicAngle = ReadInt32(stream);
        stream.Position = table.Offset + 12;
        var isFixedPitch = ReadUInt32(stream) is not 0;
        return new PostTable(italicAngle is not 0, isFixedPitch);
    }

    private static PdfSharpFontGenericFamily GetGenericFamily(Os2Table? os2, PostTable? post)
    {
        if (post?.IsFixedPitch is true)
            return PdfSharpFontGenericFamily.Monospace;

        if (os2 is null)
            return PdfSharpFontGenericFamily.Unknown;

        if (os2.Panose.Length >= 4 && os2.Panose[3] is 9)
            return PdfSharpFontGenericFamily.Monospace;

        if (os2.Panose.Length >= 2 && os2.Panose[0] is 2)
        {
            var serifStyle = os2.Panose[1];
            if (serifStyle is >= 2 and <= 10)
                return PdfSharpFontGenericFamily.Serif;
            if (serifStyle is >= 11 and <= 15)
                return PdfSharpFontGenericFamily.SansSerif;
        }

        var ibmClass = (byte)(os2.FamilyClass >> 8);
        return ibmClass switch
        {
            >= 1 and <= 7 => PdfSharpFontGenericFamily.Serif,
            8             => PdfSharpFontGenericFamily.SansSerif,
            _             => PdfSharpFontGenericFamily.Unknown,
        };
    }

    private static int NormalizeWeight(int weight)
        => Math.Clamp(weight, 1, 1000);

    private static int? GetWeightFromStyleTexts(IEnumerable<string> values)
    {
        foreach (var value in NormalizeDistinct(values))
        {
            if (value.Contains("thin", StringComparison.OrdinalIgnoreCase))
                return 100;
            if (value.Contains("extralight", StringComparison.OrdinalIgnoreCase)
                || value.Contains("ultralight", StringComparison.OrdinalIgnoreCase))
            {
                return 200;
            }

            if (value.Contains("light", StringComparison.OrdinalIgnoreCase))
                return 300;
            if (value.Contains("regular", StringComparison.OrdinalIgnoreCase)
                || value.Contains("normal", StringComparison.OrdinalIgnoreCase)
                || value.Contains("book", StringComparison.OrdinalIgnoreCase)
                || value.Contains("roman", StringComparison.OrdinalIgnoreCase))
            {
                return RegularWeight;
            }

            if (value.Contains("medium", StringComparison.OrdinalIgnoreCase))
                return 500;
            if (value.Contains("semibold", StringComparison.OrdinalIgnoreCase)
                || value.Contains("demibold", StringComparison.OrdinalIgnoreCase))
            {
                return 600;
            }

            if (value.Contains("extrabold", StringComparison.OrdinalIgnoreCase)
                || value.Contains("ultrabold", StringComparison.OrdinalIgnoreCase))
            {
                return 800;
            }

            if (value.Contains("black", StringComparison.OrdinalIgnoreCase)
                || value.Contains("heavy", StringComparison.OrdinalIgnoreCase))
            {
                return 900;
            }

            if (value.Contains("bold", StringComparison.OrdinalIgnoreCase))
                return BoldWeight;
        }

        return null;
    }

    private static bool ContainsItalicText(IEnumerable<string> values)
        => NormalizeDistinct(values)
            .Any((q) => q.Contains("italic", StringComparison.OrdinalIgnoreCase)
                        || q.Contains("oblique", StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<string> NormalizeDistinct(IEnumerable<string> values)
        => values
            .Select(Normalize)
            .Where((q) => q.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static IReadOnlyList<string> Distinct(IEnumerable<string> values)
        => values
            .Where((q) => !string.IsNullOrWhiteSpace(q))
            .Select((q) => q.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string ReadNameString(Stream stream, long offset, ushort length, NameRecord record)
    {
        stream.Position = offset;
        var bytes = new byte[length];
        var read = stream.Read(bytes, 0, bytes.Length);
        if (read != bytes.Length)
            throw new EndOfStreamException();

        var encoding = record.PlatformId is 0 or 3
            ? Encoding.BigEndianUnicode
            : Encoding.Latin1;
        return encoding.GetString(bytes).Trim('\0', ' ', '\t', '\r', '\n');
    }

    private static string ReadTag(Stream stream)
    {
        var bytes = new byte[4];
        var read = stream.Read(bytes, 0, bytes.Length);
        if (read != bytes.Length)
            throw new EndOfStreamException();
        return Encoding.ASCII.GetString(bytes);
    }

    private static ushort ReadUInt16(Stream stream)
    {
        var high = stream.ReadByte();
        var low = stream.ReadByte();
        if (high < 0 || low < 0)
            throw new EndOfStreamException();
        return (ushort)((high << 8) | low);
    }

    private static uint ReadUInt32(Stream stream)
    {
        var b1 = stream.ReadByte();
        var b2 = stream.ReadByte();
        var b3 = stream.ReadByte();
        var b4 = stream.ReadByte();
        if (b1 < 0 || b2 < 0 || b3 < 0 || b4 < 0)
            throw new EndOfStreamException();
        return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
    }

    private static int ReadInt32(Stream stream)
        => unchecked((int)ReadUInt32(stream));

    private sealed record FontMetadata(
        IReadOnlyCollection<string> Families,
        IReadOnlyCollection<string> Subfamilies,
        IReadOnlyCollection<string> FullNames,
        IReadOnlyCollection<string> TypographicFamilies,
        IReadOnlyCollection<string> TypographicSubfamilies,
        IReadOnlyCollection<string> PostScriptNames,
        Os2Table? Os2,
        HeadTable? Head,
        PostTable? Post);

    private sealed record NameTable(
        IReadOnlyCollection<string> Families,
        IReadOnlyCollection<string> Subfamilies,
        IReadOnlyCollection<string> FullNames,
        IReadOnlyCollection<string> TypographicFamilies,
        IReadOnlyCollection<string> TypographicSubfamilies,
        IReadOnlyCollection<string> PostScriptNames);

    private sealed record Os2Table(
        int Weight,
        ushort FamilyClass,
        byte[] Panose,
        bool IsItalic,
        bool IsBold,
        bool IsRegular,
        bool IsOblique);

    private sealed record HeadTable(bool IsBold, bool IsItalic);

    private sealed record PostTable(bool IsItalic, bool IsFixedPitch);

    private readonly record struct NameRecord(
        ushort PlatformId,
        ushort EncodingId,
        ushort LanguageId,
        ushort NameId,
        ushort Length,
        ushort Offset);

    private readonly record struct TableRecord(uint Offset, uint Length);
}

internal sealed record PdfSharpFontFaceDescriptor(
    string Path,
    string DisplayFamilyName,
    string DisplayFaceName,
    IReadOnlyCollection<string> NormalizedFamilyNames,
    IReadOnlyCollection<string> NormalizedFaceNames,
    int Weight,
    bool IsItalic,
    PdfSharpFontGenericFamily GenericFamily)
{
    public bool IsBold => Weight >= 600;

    public bool MatchesFamily(string normalizedFamily)
        => NormalizedFamilyNames.Any((q) => string.Equals(q, normalizedFamily, StringComparison.OrdinalIgnoreCase))
           || NormalizedFaceNames.Any((q) => string.Equals(q, normalizedFamily, StringComparison.OrdinalIgnoreCase));
}

internal enum PdfSharpFontGenericFamily
{
    Unknown,
    SansSerif,
    Serif,
    Monospace,
}
