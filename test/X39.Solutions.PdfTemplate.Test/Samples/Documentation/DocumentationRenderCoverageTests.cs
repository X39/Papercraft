using System.Text.RegularExpressions;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class DocumentationRenderCoverageTests
{
    private static readonly byte[] PdfHeader = "%PDF"u8.ToArray();

    private static readonly byte[] PngHeader =
    {
        0x89,
        0x50,
        0x4E,
        0x47,
        0x0D,
        0x0A,
        0x1A,
        0x0A,
    };

    private static readonly byte[] SvgHeader = "<?xml"u8.ToArray();

    private static readonly string[] RequiredSampleAssetSuffixes =
    {
        ".png",
        ".svg",
        "-skiasharp.pdf",
        "-pdfsharp.pdf",
    };

    private static readonly Regex SamplePreviewIncludeExpression = new(
        @"\{%\s*include\s+sample-preview\.html\s+sample=""([^""]+)""(?:\s+alt=""([^""]*)"")?\s*%\}",
        RegexOptions.Compiled);

    private static readonly Regex SampleImageReferenceExpression = new(
        @"!\[[^\]]*\]\(\.\./assets/samples/([^\)]+\.png)\)",
        RegexOptions.Compiled);

    [Fact]
    public void ControlManualXmlSamplesHaveFollowingRender()
    {
        var missing = Directory.EnumerateFiles(GetManualDirectory(), "controls-*.md")
            .SelectMany(FindXmlSamplesWithoutFollowingRender)
            .ToArray();

        Assert.True(
            missing.Length is 0,
            "Control manual XML samples without a following sample render:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, missing.Select((q) => $"{q.File}:{q.Line}: {q.FirstLine}")));
    }

    [Fact]
    public void ManualSamplePreviewReferencesHaveGeneratedAssets()
    {
        var repositoryRoot = GetRepositoryRoot();
        var samplesDirectory = Path.Combine(repositoryRoot, "docs", "assets", "samples");
        var missing = GetDocumentationMarkdownFiles()
            .SelectMany(
                (filePath) => SampleImageReferenceExpression
                    .Matches(File.ReadAllText(filePath))
                    .Select((match) => Path.GetFileNameWithoutExtension(match.Groups[1].Value))
                    .Concat(
                        SamplePreviewIncludeExpression
                            .Matches(File.ReadAllText(filePath))
                            .Select((match) => match.Groups[1].Value))
                    .SelectMany((sample) => RequiredSampleAssetSuffixes.Select((suffix) => new
                    {
                        File = Path.GetRelativePath(repositoryRoot, filePath),
                        Sample = sample,
                        Asset = $"{sample}{suffix}",
                    })))
            .Where((q) => !File.Exists(Path.Combine(samplesDirectory, q.Asset)))
            .ToArray();

        Assert.True(
            missing.Length is 0,
            "Manual sample preview references without checked-in generated assets:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, missing.Select((q) => $"{q.File}: {q.Asset}")));
    }

    [Fact]
    public void ManualPagesUseSamplePreviewIncludesForGeneratedSamples()
    {
        var repositoryRoot = GetRepositoryRoot();
        var legacyReferences = GetDocumentationMarkdownFiles()
            .Where((q) => !string.Equals(Path.GetFileName(q), "work-plan.md", StringComparison.OrdinalIgnoreCase))
            .SelectMany(
                (filePath) => SampleImageReferenceExpression
                    .Matches(File.ReadAllText(filePath))
                    .Select((match) => new
                    {
                        File = Path.GetRelativePath(repositoryRoot, filePath),
                        Image = match.Groups[1].Value,
                    }))
            .ToArray();

        Assert.True(
            legacyReferences.Length is 0,
            "Generated sample previews should use sample-preview.html includes instead of Markdown images:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, legacyReferences.Select((q) => $"{q.File}: {q.Image}")));
    }

    [Fact]
    public void RepresentativeDocumentationSampleArtifactsHaveExpectedFormats()
    {
        var samplesDirectory = Path.Combine(GetRepositoryRoot(), "docs", "assets", "samples");

        AssertStartsWith(PngHeader, File.ReadAllBytes(Path.Combine(samplesDirectory, "text-basic.png")));
        AssertStartsWith(SvgHeader, File.ReadAllBytes(Path.Combine(samplesDirectory, "text-basic.svg")));
        AssertStartsWith(PdfHeader, File.ReadAllBytes(Path.Combine(samplesDirectory, "text-basic-skiasharp.pdf")));
        AssertStartsWith(PdfHeader, File.ReadAllBytes(Path.Combine(samplesDirectory, "text-basic-pdfsharp.pdf")));
    }

    [Fact]
    public void DocumentationSampleOutputDirectoryDefaultsToTestResults()
    {
        using var _ = EnvironmentVariableScope.Set(
            DocumentationSampleBase.UpdateDocumentationSampleAssetsEnvironmentVariable,
            null);
        var repositoryRoot = GetRepositoryRoot();

        var outputDirectory = DocumentationSampleBase.GetSampleOutputDirectory();

        Assert.Equal(
            Path.Combine(
                repositoryRoot,
                "test",
                "X39.Solutions.PdfTemplate.Test",
                "TestResults",
                "documentation-samples"),
            outputDirectory);
        Assert.NotEqual(Path.Combine(repositoryRoot, "docs", "assets", "samples"), outputDirectory);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("yes")]
    public void DocumentationSampleOutputDirectoryCanTargetCheckedInAssetsWhenOptedIn(string value)
    {
        using var _ = EnvironmentVariableScope.Set(
            DocumentationSampleBase.UpdateDocumentationSampleAssetsEnvironmentVariable,
            value);

        Assert.Equal(
            Path.Combine(GetRepositoryRoot(), "docs", "assets", "samples"),
            DocumentationSampleBase.GetSampleOutputDirectory());
    }

    private static IEnumerable<MissingXmlRender> FindXmlSamplesWithoutFollowingRender(string filePath)
    {
        var repositoryRoot = GetRepositoryRoot();
        var relativePath = Path.GetRelativePath(repositoryRoot, filePath);
        var lines = File.ReadAllLines(filePath);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!IsXmlFenceStart(lines[index]))
                continue;

            var fenceStart = index;
            var fenceEnd = FindFenceEnd(lines, fenceStart + 1);
            var nextXmlFence = FindNextXmlFence(lines, fenceEnd + 1);
            if (HasSamplePreviewReference(lines, fenceEnd + 1, nextXmlFence))
            {
                index = fenceEnd;
                continue;
            }

            yield return new MissingXmlRender(
                relativePath,
                fenceStart + 1,
                FindFirstContentLine(lines, fenceStart + 1, fenceEnd));
            index = fenceEnd;
        }
    }

    private static bool HasSamplePreviewReference(string[] lines, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            if (SamplePreviewIncludeExpression.IsMatch(lines[index]))
                return true;
        }

        return false;
    }

    private static int FindFenceEnd(string[] lines, int start)
    {
        for (var index = start; index < lines.Length; index++)
        {
            if (lines[index].Trim() is "```")
                return index;
        }

        return lines.Length - 1;
    }

    private static int FindNextXmlFence(string[] lines, int start)
    {
        for (var index = start; index < lines.Length; index++)
        {
            if (IsXmlFenceStart(lines[index]))
                return index;
        }

        return lines.Length;
    }

    private static string FindFirstContentLine(string[] lines, int start, int end)
        => lines[start..end]
            .Select((q) => q.Trim())
            .FirstOrDefault((q) => q.Length > 0)
           ?? string.Empty;

    private static bool IsXmlFenceStart(string line)
        => line.Trim() is "```xml";

    private static string GetManualDirectory()
        => Path.Combine(GetRepositoryRoot(), "docs", "manual");

    private static IEnumerable<string> GetDocumentationMarkdownFiles()
    {
        var repositoryRoot = GetRepositoryRoot();
        yield return Path.Combine(repositoryRoot, "docs", "index.md");

        foreach (var filePath in Directory.EnumerateFiles(GetManualDirectory(), "*.md"))
        {
            yield return filePath;
        }
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "docs", "manual", "work-plan.md")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the repository root from the test output directory.");
    }

    private static void AssertStartsWith(byte[] expected, byte[] actual)
        => Assert.True(
            actual.AsSpan().StartsWith(expected),
            $"Expected file to start with '{Convert.ToHexString(expected)}'.");

    private sealed record MissingXmlRender(string File, int Line, string FirstLine);

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _previousValue;

        private EnvironmentVariableScope(string name, string? value)
        {
            _name = name;
            _previousValue = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public static EnvironmentVariableScope Set(string name, string? value)
            => new(name, value);

        public void Dispose()
            => Environment.SetEnvironmentVariable(_name, _previousValue);
    }
}
