using System.Globalization;
using System.Text;
using System.Xml;
using SkiaSharp;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

public abstract class DocumentationSampleBase : SampleBase
{
    private const float DocumentationPreviewDotsPerInch = 192F;

    protected static DocumentOptions CompactDocumentOptions { get; } = new()
    {
        DotsPerInch = DocumentationPreviewDotsPerInch,
        PageWidthInMillimeters = 100,
        PageHeightInMillimeters = 45,
        Margin = new Thickness(new Length(5, ELengthUnit.Millimeters)),
    };

    protected async Task RenderDocumentationSampleAsync(
        string sampleName,
        string xml,
        DocumentOptions? documentOptions = null,
        Action<Generator>? configureGenerator = null,
        CancellationToken cancellationToken = default,
        Action<PdfTemplateServiceBuilder>? configureServices = null)
    {
        var outputDirectory = GetSampleOutputDirectory();
        Directory.CreateDirectory(outputDirectory);
        DeleteStaleSampleFiles(outputDirectory, sampleName);

        using var generator = configureServices is null
            ? CreateGenerator()
            : CreateGenerator(configureServices);
        configureGenerator?.Invoke(generator);
        await using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var xmlReader = XmlReader.Create(xmlStream);

        var bitmaps = await generator.GenerateBitmapsAsync(
            xmlReader,
            CultureInfo.InvariantCulture,
            WithDocumentationPreviewDensity(documentOptions ?? CompactDocumentOptions),
            cancellationToken);
        Assert.NotEmpty(bitmaps);

        try
        {
            var pageCount = bitmaps.Count;
            var index = 0;
            foreach (var bitmap in bitmaps)
            {
                var fileName = pageCount == 1
                    ? $"{sampleName}.png"
                    : $"{sampleName}-page-{index + 1}.png";
                var filePath = Path.Combine(outputDirectory, fileName);
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                data.SaveTo(fileStream);
                index++;
            }
        }
        finally
        {
            foreach (var bitmap in bitmaps)
            {
                bitmap.Dispose();
            }
        }
    }

    private static string GetSampleOutputDirectory()
        => Path.Combine(GetRepositoryRoot(), "docs", "assets", "samples");

    private static DocumentOptions WithDocumentationPreviewDensity(DocumentOptions documentOptions)
        => documentOptions with { DotsPerInch = DocumentationPreviewDotsPerInch };

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "docs", "manual", "work-plan.md")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the repository root from the test output directory.");
    }

    private static void DeleteStaleSampleFiles(string outputDirectory, string sampleName)
    {
        var exactFile = Path.Combine(outputDirectory, $"{sampleName}.png");
        if (File.Exists(exactFile))
        {
            File.Delete(exactFile);
        }

        foreach (var staleFile in Directory.EnumerateFiles(outputDirectory, $"{sampleName}-page-*.png"))
        {
            File.Delete(staleFile);
        }
    }
}
