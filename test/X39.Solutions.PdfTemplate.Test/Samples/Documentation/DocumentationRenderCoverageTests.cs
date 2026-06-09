using System.Text.RegularExpressions;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

[Collection("Samples")]
public sealed class DocumentationRenderCoverageTests
{
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
    public void ManualSampleImageReferencesExist()
    {
        var repositoryRoot = GetRepositoryRoot();
        var samplesDirectory = Path.Combine(repositoryRoot, "docs", "assets", "samples");
        var missing = Directory.EnumerateFiles(GetManualDirectory(), "*.md")
            .SelectMany(
                (filePath) => SampleImageReferenceExpression
                    .Matches(File.ReadAllText(filePath))
                    .Select((match) => new
                    {
                        File = Path.GetRelativePath(repositoryRoot, filePath),
                        Image = match.Groups[1].Value,
                    }))
            .Where((q) => !File.Exists(Path.Combine(samplesDirectory, q.Image)))
            .ToArray();

        Assert.True(
            missing.Length is 0,
            "Manual sample image references without a checked-in PNG:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, missing.Select((q) => $"{q.File}: {q.Image}")));
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
            if (HasSampleImageReference(lines, fenceEnd + 1, nextXmlFence))
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

    private static bool HasSampleImageReference(string[] lines, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            if (SampleImageReferenceExpression.IsMatch(lines[index]))
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

    private sealed record MissingXmlRender(string File, int Line, string FirstLine);
}
