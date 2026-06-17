using System.Text;
using PdfSharp.Fonts;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

namespace X39.Solutions.PdfTemplate.Test.Rendering;

public sealed class PdfSharpSystemFontResolverTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(
        Path.GetTempPath(),
        "papercraft-font-tests",
        Guid.NewGuid().ToString("N"));

    public PdfSharpSystemFontResolverTests()
        => Directory.CreateDirectory(_tempDirectory);

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    [Theory]
    [InlineData("calibri.ttf", "Regular", 400, false)]
    [InlineData("calibrib.ttf", "Bold", 700, false)]
    [InlineData("calibrii.ttf", "Italic", 400, true)]
    [InlineData("calibriz.ttf", "Bold Italic", 700, true)]
    [InlineData("calibril.ttf", "Light", 300, false)]
    [InlineData("calibrili.ttf", "Light Italic", 300, true)]
    [InlineData("segoeui.ttf", "Regular", 400, false)]
    [InlineData("segoeuib.ttf", "Bold", 700, false)]
    [InlineData("segoeuii.ttf", "Italic", 400, true)]
    [InlineData("segoeuiz.ttf", "Bold Italic", 700, true)]
    [InlineData("arial.ttf", "Regular", 400, false)]
    [InlineData("arialbd.ttf", "Bold", 700, false)]
    [InlineData("ariali.ttf", "Italic", 400, true)]
    [InlineData("arialbi.ttf", "Bold Italic", 700, true)]
    public void ClassifierUsesFontMetadataForSyntheticWindowsNames(
        string fileName,
        string subfamily,
        int expectedWeight,
        bool expectedItalic)
    {
        var path = CreateFont(fileName, "Calibri", subfamily, expectedWeight, expectedItalic);

        var descriptor = PdfSharpFontFaceClassifier.Classify(path);

        Assert.Equal(expectedWeight, descriptor.Weight);
        Assert.Equal(expectedWeight >= 600, descriptor.IsBold);
        Assert.Equal(expectedItalic, descriptor.IsItalic);
        Assert.Contains("calibri", descriptor.NormalizedFamilyNames);
    }

    [Fact]
    public void ClassifierDoesNotInferStyleFromMisleadingFileName()
    {
        var path = CreateFont("calibrib.ttf", "Example Sans", "Regular", 400, italic: false);

        var descriptor = PdfSharpFontFaceClassifier.Classify(path);

        Assert.Equal(400, descriptor.Weight);
        Assert.False(descriptor.IsBold);
        Assert.False(descriptor.IsItalic);
        Assert.Contains("examplesans", descriptor.NormalizedFamilyNames);
    }

    [Fact]
    public void ResolverSelectsCalibriFacesByMetadata()
    {
        var resolver = new PdfSharpSystemFontResolver(
            new[]
            {
                CreateDescriptor("calibrib.ttf", "Calibri", "Bold", 700, false),
                CreateDescriptor("calibril.ttf", "Calibri", "Light", 300, false),
                CreateDescriptor("calibriz.ttf", "Calibri", "Bold Italic", 700, true),
                CreateDescriptor("calibrii.ttf", "Calibri", "Italic", 400, true),
                CreateDescriptor("calibri.ttf", "Calibri", "Regular", 400, false),
            });

        AssertFace("calibri.ttf", resolver.ResolveTypeface("Calibri", bold: false, italic: false));
        AssertFace("calibrib.ttf", resolver.ResolveTypeface("Calibri", bold: true, italic: false));
        AssertFace("calibrii.ttf", resolver.ResolveTypeface("Calibri", bold: false, italic: true));
        AssertFace("calibriz.ttf", resolver.ResolveTypeface("Calibri", bold: true, italic: true));
    }

    [Fact]
    public void ResolverSelectsArbitraryNonHardcodedFamilyByMetadata()
    {
        var resolver = new PdfSharpSystemFontResolver(
            new[]
            {
                CreateDescriptor("xqzb.ttf", "Acme Invoice Text", "Bold", 700, false),
                CreateDescriptor("xqzr.ttf", "Acme Invoice Text", "Regular", 400, false),
            });

        AssertFace("xqzr.ttf", resolver.ResolveTypeface("Acme Invoice Text", bold: false, italic: false));
        AssertFace("xqzb.ttf", resolver.ResolveTypeface("Acme Invoice Text", bold: true, italic: false));
    }

    [Fact]
    public void ResolverSelectsGenericSansSerifByMetadataClassification()
    {
        var resolver = new PdfSharpSystemFontResolver(
            new[]
            {
                CreateDescriptor("mono.ttf", "Example Mono", "Regular", 400, false, PdfSharpFontGenericFamily.Monospace),
                CreateDescriptor("sans.ttf", "Example Sans", "Regular", 400, false, PdfSharpFontGenericFamily.SansSerif),
            });

        AssertFace("sans.ttf", resolver.ResolveTypeface("sans-serif", bold: false, italic: false));
    }

    [Fact]
    public void ResolverSubstitutesMissingFamilyWithDiagnostic()
    {
        var resolver = new PdfSharpSystemFontResolver(
            new[] { CreateDescriptor("sans.ttf", "Example Sans", "Regular", 400, false) });

        var result = resolver.ResolveTypefaceDetailed(
            "Missing Family",
            bold: false,
            italic: false,
            reportDiagnostics: false);

        AssertFace("sans.ttf", result.FontResolverInfo);
        var diagnostic = Assert.Single(result.Diagnostics, (q) => q.Code == RenderDiagnosticCodes.MissingFontSubstitution);
        Assert.Equal(RendererSupportLevel.Degraded, diagnostic.Level);
        Assert.Contains("Missing Family", diagnostic.Message, StringComparison.Ordinal);
        Assert.Contains("Example Sans", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ResolverReportsStyleSubstitutionWithoutBoldSimulation()
    {
        var resolver = new PdfSharpSystemFontResolver(
            new[] { CreateDescriptor("regular.ttf", "Example Sans", "Regular", 400, false) });

        var result = resolver.ResolveTypefaceDetailed(
            "Example Sans",
            bold: true,
            italic: false,
            reportDiagnostics: false);

        AssertFace("regular.ttf", result.FontResolverInfo);
        Assert.NotNull(result.FontResolverInfo);
        Assert.False(result.FontResolverInfo.MustSimulateBold);
        Assert.Contains(result.Diagnostics, (q) => q.Code == RenderDiagnosticCodes.FontFaceSubstitution);
    }

    private PdfSharpFontFaceDescriptor CreateDescriptor(
        string fileName,
        string family,
        string subfamily,
        int weight,
        bool italic,
        PdfSharpFontGenericFamily genericFamily = PdfSharpFontGenericFamily.SansSerif)
        => PdfSharpFontFaceClassifier.Classify(CreateFont(fileName, family, subfamily, weight, italic, genericFamily));

    private string CreateFont(
        string fileName,
        string family,
        string subfamily,
        int weight,
        bool italic,
        PdfSharpFontGenericFamily genericFamily = PdfSharpFontGenericFamily.SansSerif)
    {
        var path = Path.Combine(_tempDirectory, fileName);
        File.WriteAllBytes(path, SyntheticOpenTypeFont.Create(family, subfamily, weight, italic, genericFamily));
        return path;
    }

    private static void AssertFace(string expectedFileName, FontResolverInfo? info)
    {
        Assert.NotNull(info);
        Assert.EndsWith(expectedFileName, info.FaceName, StringComparison.OrdinalIgnoreCase);
        Assert.False(info.MustSimulateBold);
    }

    private static class SyntheticOpenTypeFont
    {
        public static byte[] Create(
            string family,
            string subfamily,
            int weight,
            bool italic,
            PdfSharpFontGenericFamily genericFamily)
        {
            var tables = new Dictionary<string, byte[]>(StringComparer.Ordinal)
            {
                ["head"] = CreateHeadTable(weight >= 600, italic),
                ["name"] = CreateNameTable(family, subfamily),
                ["OS/2"] = CreateOs2Table(weight, italic, genericFamily),
                ["post"] = CreatePostTable(italic, genericFamily is PdfSharpFontGenericFamily.Monospace),
            };
            return CreateSfnt(tables);
        }

        private static byte[] CreateSfnt(IReadOnlyDictionary<string, byte[]> tables)
        {
            var orderedTables = tables.OrderBy((q) => q.Key, StringComparer.Ordinal).ToArray();
            using var stream = new MemoryStream();
            WriteUInt32(stream, 0x00010000);
            WriteUInt16(stream, (ushort)orderedTables.Length);
            WriteUInt16(stream, 0);
            WriteUInt16(stream, 0);
            WriteUInt16(stream, 0);

            var dataOffset = 12 + orderedTables.Length * 16;
            var offsets = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var (tag, data) in orderedTables)
            {
                offsets[tag] = dataOffset;
                dataOffset += Align4(data.Length);
            }

            foreach (var (tag, data) in orderedTables)
            {
                WriteTag(stream, tag);
                WriteUInt32(stream, 0);
                WriteUInt32(stream, (uint)offsets[tag]);
                WriteUInt32(stream, (uint)data.Length);
            }

            foreach (var (tag, data) in orderedTables)
            {
                while (stream.Position < offsets[tag])
                    stream.WriteByte(0);
                stream.Write(data, 0, data.Length);
                while (stream.Position % 4 is not 0)
                    stream.WriteByte(0);
            }

            return stream.ToArray();
        }

        private static byte[] CreateNameTable(string family, string subfamily)
        {
            var fullName = $"{family} {subfamily}";
            var postScriptName = $"{family}-{subfamily}".Replace(" ", string.Empty, StringComparison.Ordinal);
            var records = new[]
            {
                new NameEntry(1, family),
                new NameEntry(2, subfamily),
                new NameEntry(4, fullName),
                new NameEntry(6, postScriptName),
                new NameEntry(16, family),
                new NameEntry(17, subfamily),
            };
            var stringOffset = 6 + records.Length * 12;
            using var stream = new MemoryStream();
            using var stringData = new MemoryStream();
            WriteUInt16(stream, 0);
            WriteUInt16(stream, (ushort)records.Length);
            WriteUInt16(stream, (ushort)stringOffset);
            foreach (var record in records)
            {
                var bytes = Encoding.BigEndianUnicode.GetBytes(record.Value);
                WriteUInt16(stream, 3);
                WriteUInt16(stream, 1);
                WriteUInt16(stream, 0x0409);
                WriteUInt16(stream, record.NameId);
                WriteUInt16(stream, (ushort)bytes.Length);
                WriteUInt16(stream, (ushort)stringData.Length);
                stringData.Write(bytes, 0, bytes.Length);
            }

            stream.Write(stringData.ToArray());
            return stream.ToArray();
        }

        private static byte[] CreateOs2Table(
            int weight,
            bool italic,
            PdfSharpFontGenericFamily genericFamily)
        {
            var bytes = new byte[64];
            WriteUInt16(bytes, 4, (ushort)weight);
            WriteUInt16(bytes, 30, genericFamily is PdfSharpFontGenericFamily.SansSerif ? (ushort)0x0800 : (ushort)0x0100);
            bytes[32] = 2;
            bytes[33] = genericFamily is PdfSharpFontGenericFamily.Serif ? (byte)2 : (byte)11;
            bytes[35] = genericFamily is PdfSharpFontGenericFamily.Monospace ? (byte)9 : (byte)3;

            ushort selection = 0;
            if (italic)
                selection |= 0x0001;
            if (weight >= 600)
                selection |= 0x0020;
            if (weight == 400 && !italic)
                selection |= 0x0040;
            WriteUInt16(bytes, 62, selection);
            return bytes;
        }

        private static byte[] CreateHeadTable(bool bold, bool italic)
        {
            var bytes = new byte[54];
            ushort macStyle = 0;
            if (bold)
                macStyle |= 0x0001;
            if (italic)
                macStyle |= 0x0002;
            WriteUInt16(bytes, 44, macStyle);
            return bytes;
        }

        private static byte[] CreatePostTable(bool italic, bool fixedPitch)
        {
            var bytes = new byte[32];
            WriteUInt32(bytes, 4, italic ? 0x00010000U : 0U);
            WriteUInt32(bytes, 12, fixedPitch ? 1U : 0U);
            return bytes;
        }

        private static int Align4(int value)
            => (value + 3) & ~3;

        private static void WriteTag(Stream stream, string tag)
        {
            var bytes = Encoding.ASCII.GetBytes(tag);
            Assert.Equal(4, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteUInt16(Stream stream, ushort value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteUInt32(Stream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteUInt16(byte[] bytes, int offset, ushort value)
        {
            bytes[offset] = (byte)(value >> 8);
            bytes[offset + 1] = (byte)value;
        }

        private static void WriteUInt32(byte[] bytes, int offset, uint value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }

        private readonly record struct NameEntry(ushort NameId, string Value);
    }
}
