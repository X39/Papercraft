using System.Xml.Linq;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftPackageScaffoldTests
{
    private static readonly string[] RendererNeutralCoreContractFiles =
    {
        "PapercraftMediaTypes.cs",
        "RasterPageInfo.cs",
        "RasterPageRenderOutput.cs",
        "RenderDiagnostic.cs",
        "RenderDiagnosticCodes.cs",
        "RenderFeatureUse.cs",
        "RendererCapabilities.cs",
        "RendererFeatures.cs",
        "RendererOutputKind.cs",
        "RendererSupportLevel.cs",
        "RenderOutput.cs",
        "RenderTarget.cs",
        "RenderValidationException.cs",
        "RenderValidationResult.cs",
        "TemplateLocation.cs",
        "IPapercraftRenderBackend.cs",
        "PapercraftDocument.cs",
        "PapercraftPage.cs",
        "PapercraftInstrumentation.cs",
        "PapercraftRenderOptions.cs",
    };

    private static readonly string[] SkiaTypeNames =
    {
        "SkiaSharp",
        "SKBitmap",
        "SKCanvas",
        "SKPaint",
        "SKColor",
        "SKRect",
        "SKPoint",
        "SKTypeface",
    };

    private static readonly string[] LegacyCoreDependencyNames =
    {
        "X39.Util",
        "JetBrains",
        "PublicAPI",
        "MeansImplicitUse",
        "OpenTelemetry",
        "Microsoft.Extensions.Hosting",
    };

    private static readonly string[] BarcodeDependencyNames =
    {
        "Net.Codecrete.QrCodeGenerator",
        "ZXing.Net",
    };

    [Fact]
    public void ScaffoldPropsKeepPapercraftPackagesPackable()
    {
        var props = LoadDocument("source", "PapercraftPackageScaffold.props");

        Assert.Equal("net10.0", GetProperty(props, "TargetFramework"));
        Assert.Equal("true", GetProperty(props, "IsPackable"));
        Assert.Equal("true", GetProperty(props, "IncludeSymbols"));
        Assert.Equal("true", GetProperty(props, "IncludeSources"));
        Assert.Equal("snupkg", GetProperty(props, "SymbolPackageFormat"));
        Assert.Equal("true", GetProperty(props, "GenerateDocumentationFile"));
        Assert.Equal("LGPL-3.0-only", GetProperty(props, "PackageLicenseExpression"));
    }

    [Theory]
    [InlineData("X39.Solutions.Papercraft.Core")]
    [InlineData("X39.Solutions.Papercraft.Rendering.SkiaSharp")]
    [InlineData("X39.Solutions.Papercraft.Rendering.Svg")]
    [InlineData("X39.Solutions.Papercraft.Rendering.PdfSharp")]
    [InlineData("X39.Solutions.Papercraft.Rendering.EscPos")]
    [InlineData("X39.Solutions.Papercraft")]
    [InlineData("X39.Solutions.Papercraft.OpenTelemetry")]
    [InlineData("X39.Solutions.Papercraft.Controls.QrCode")]
    [InlineData("X39.Solutions.Papercraft.Controls.ZXing")]
    public void ScaffoldProjectsUsePlannedPackageIds(string packageId)
    {
        var project = LoadProject(packageId);

        AssertImportsScaffoldProps(project);
        Assert.Equal(packageId, GetProperty(project, "PackageId"));
        Assert.Equal(packageId, GetProperty(project, "AssemblyName"));
    }

    [Fact]
    public void ScaffoldProjectsModelPlannedPackageDependencyGraph()
    {
        var core = LoadProject("X39.Solutions.Papercraft.Core");
        var skiaSharp = LoadProject("X39.Solutions.Papercraft.Rendering.SkiaSharp");
        var svg = LoadProject("X39.Solutions.Papercraft.Rendering.Svg");
        var pdfSharp = LoadProject("X39.Solutions.Papercraft.Rendering.PdfSharp");
        var escPos = LoadProject("X39.Solutions.Papercraft.Rendering.EscPos");
        var facade = LoadProject("X39.Solutions.Papercraft");
        var openTelemetry = LoadProject("X39.Solutions.Papercraft.OpenTelemetry");
        var qrCodeControls = LoadProject("X39.Solutions.Papercraft.Controls.QrCode");
        var zxingControls = LoadProject("X39.Solutions.Papercraft.Controls.ZXing");
        var compatibility = LoadProject("X39.Solutions.PdfTemplate");

        Assert.Empty(GetProjectReferences(core));
        Assert.Equal(
            new[] { "Microsoft.Extensions.DependencyInjection.Abstractions" },
            GetPackageReferences(core),
            StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("SkiaSharp", GetPackageReferences(core), StringComparer.OrdinalIgnoreCase);

        Assert.Equal(
            new[]
            {
                @"..\X39.Solutions.Papercraft.Core\X39.Solutions.Papercraft.Core.csproj",
            },
            GetProjectReferences(skiaSharp),
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            new[] { "Microsoft.Extensions.DependencyInjection.Abstractions", "SkiaSharp" },
            GetPackageReferences(skiaSharp),
            StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            @"..\X39.Solutions.PdfTemplate\X39.Solutions.PdfTemplate.csproj",
            GetProjectReferences(skiaSharp),
            StringComparer.OrdinalIgnoreCase);

        Assert.Equal(
            new[]
            {
                @"..\X39.Solutions.Papercraft.Core\X39.Solutions.Papercraft.Core.csproj",
            },
            GetProjectReferences(svg),
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            new[] { "Microsoft.Extensions.DependencyInjection.Abstractions" },
            GetPackageReferences(svg),
            StringComparer.OrdinalIgnoreCase);

        Assert.Equal(
            new[]
            {
                @"..\X39.Solutions.Papercraft.Core\X39.Solutions.Papercraft.Core.csproj",
            },
            GetProjectReferences(pdfSharp),
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            new[] { "Microsoft.Extensions.DependencyInjection.Abstractions", "PDFsharp" },
            GetPackageReferences(pdfSharp),
            StringComparer.OrdinalIgnoreCase);

        Assert.Equal(
            new[]
            {
                @"..\X39.Solutions.Papercraft.Core\X39.Solutions.Papercraft.Core.csproj",
            },
            GetProjectReferences(escPos),
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            new[] { "Microsoft.Extensions.DependencyInjection.Abstractions" },
            GetPackageReferences(escPos),
            StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain(GetProjectReferences(escPos), ReferencesSkiaSharp);
        Assert.DoesNotContain(GetPackageReferences(escPos), ReferencesSkiaSharp);

        Assert.Equal(
            new[]
            {
                @"..\X39.Solutions.Papercraft.Core\X39.Solutions.Papercraft.Core.csproj",
                @"..\X39.Solutions.Papercraft.Rendering.SkiaSharp\X39.Solutions.Papercraft.Rendering.SkiaSharp.csproj",
                @"..\X39.Solutions.Papercraft\X39.Solutions.Papercraft.csproj",
            },
            GetProjectReferences(compatibility),
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            new[] { "Microsoft.Extensions.DependencyInjection.Abstractions", "X39.Util" },
            GetPackageReferences(compatibility),
            StringComparer.OrdinalIgnoreCase);

        Assert.Equal(
            new[]
            {
                @"..\X39.Solutions.Papercraft.Core\X39.Solutions.Papercraft.Core.csproj",
                @"..\X39.Solutions.Papercraft.Rendering.SkiaSharp\X39.Solutions.Papercraft.Rendering.SkiaSharp.csproj",
            },
            GetProjectReferences(facade),
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            new[] { "Microsoft.Extensions.DependencyInjection.Abstractions" },
            GetPackageReferences(facade),
            StringComparer.OrdinalIgnoreCase);

        Assert.Equal(
            new[]
            {
                @"..\X39.Solutions.Papercraft.Core\X39.Solutions.Papercraft.Core.csproj",
            },
            GetProjectReferences(openTelemetry),
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            new[]
            {
                "Microsoft.Extensions.Hosting.Abstractions",
                "OpenTelemetry",
                "OpenTelemetry.Extensions.Hosting",
            },
            GetPackageReferences(openTelemetry),
            StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain(GetProjectReferences(openTelemetry), ReferencesSkiaSharp);
        Assert.DoesNotContain(GetPackageReferences(openTelemetry), ReferencesSkiaSharp);

        Assert.Equal(
            new[]
            {
                @"..\X39.Solutions.Papercraft.Core\X39.Solutions.Papercraft.Core.csproj",
            },
            GetProjectReferences(qrCodeControls),
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            new[] { "Microsoft.Extensions.DependencyInjection.Abstractions", "Net.Codecrete.QrCodeGenerator" },
            GetPackageReferences(qrCodeControls),
            StringComparer.OrdinalIgnoreCase);

        Assert.Equal(
            new[]
            {
                @"..\X39.Solutions.Papercraft.Core\X39.Solutions.Papercraft.Core.csproj",
            },
            GetProjectReferences(zxingControls),
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            new[] { "Microsoft.Extensions.DependencyInjection.Abstractions", "ZXing.Net" },
            GetPackageReferences(zxingControls),
            StringComparer.OrdinalIgnoreCase);

        foreach (var project in new[] { core, skiaSharp, svg, pdfSharp, escPos, facade, openTelemetry, compatibility })
        {
            Assert.DoesNotContain(GetPackageReferences(project), ReferencesBarcodeDependency);
            Assert.DoesNotContain(GetProjectReferences(project), ReferencesBarcodePackage);
        }
    }

    [Fact]
    public void SkiaSharpRendererImplementationLivesInRendererPackage()
    {
        var repositoryRoot = GetRepositoryRoot();
        var compatibilityFile = Path.Combine(
            repositoryRoot.FullName,
            "source",
            "X39.Solutions.PdfTemplate",
            "Papercraft",
            "Rendering",
            "SkiaSharp",
            "SkiaSharpRenderBackend.cs");
        var rendererFile = Path.Combine(
            repositoryRoot.FullName,
            "source",
            "X39.Solutions.Papercraft.Rendering.SkiaSharp",
            "SkiaSharpRenderBackend.cs");

        Assert.True(
            File.Exists(rendererFile),
            "SkiaSharpRenderBackend is Skia-specific and must be owned by X39.Solutions.Papercraft.Rendering.SkiaSharp.");
        Assert.False(
            File.Exists(compatibilityFile),
            "X39.Solutions.PdfTemplate must not keep a second SkiaSharpRenderBackend copy because the Skia renderer package owns Skia-specific implementation.");
    }

    [Fact]
    public void CoreProjectRemainsSkiaFree()
    {
        var core = LoadProject("X39.Solutions.Papercraft.Core");

        Assert.DoesNotContain(GetProjectReferences(core), ReferencesSkiaSharp);
        Assert.DoesNotContain(GetPackageReferences(core), ReferencesSkiaSharp);
        Assert.DoesNotContain(GetPackageReferences(core), ReferencesLegacyCoreDependency);

        var coreDirectory = GetRepositoryRoot()
            .GetDirectories("source")
            .Single()
            .GetDirectories("X39.Solutions.Papercraft.Core")
            .Single();
        var offendingFiles = GetProjectSourceFiles(coreDirectory)
            .Where((file) => SkiaTypeNames.Any(
                (skiaTypeName) => File.ReadAllText(file.FullName).Contains(
                    skiaTypeName,
                    StringComparison.Ordinal)))
            .Select((file) => Path.GetRelativePath(GetRepositoryRoot().FullName, file.FullName))
            .ToArray();

        Assert.Empty(offendingFiles);

        var legacyDependencyFiles = GetProjectSourceFiles(coreDirectory)
            .Where((file) => LegacyCoreDependencyNames.Any(
                (dependencyName) => File.ReadAllText(file.FullName).Contains(
                    dependencyName,
                    StringComparison.Ordinal)))
            .Select((file) => Path.GetRelativePath(GetRepositoryRoot().FullName, file.FullName))
            .ToArray();

        Assert.Empty(legacyDependencyFiles);
    }

    [Theory]
    [MemberData(nameof(GetRendererNeutralCoreContractFiles))]
    public void RendererNeutralCapabilityOutputAndDiagnosticContractsLiveInCore(string fileName)
    {
        var repositoryRoot = GetRepositoryRoot();
        var compatibilityFile = Path.Combine(
            repositoryRoot.FullName,
            "source",
            "X39.Solutions.PdfTemplate",
            "Papercraft",
            fileName);
        var coreFile = Path.Combine(repositoryRoot.FullName, "source", "X39.Solutions.Papercraft.Core", fileName);
        var skiaRendererFile = Path.Combine(
            repositoryRoot.FullName,
            "source",
            "X39.Solutions.Papercraft.Rendering.SkiaSharp",
            fileName);
        var facadeFile = Path.Combine(repositoryRoot.FullName, "source", "X39.Solutions.Papercraft", fileName);

        Assert.True(
            File.Exists(coreFile),
            $"{fileName} is renderer-neutral and must be owned by X39.Solutions.Papercraft.Core.");
        Assert.False(
            File.Exists(compatibilityFile),
            $"{fileName} is owned by X39.Solutions.Papercraft.Core; the compatibility package should reference or forward it instead of carrying its own copy.");
        Assert.False(
            File.Exists(skiaRendererFile),
            $"{fileName} is renderer-neutral and belongs in X39.Solutions.Papercraft.Core, not the SkiaSharp renderer package.");
        Assert.False(
            File.Exists(facadeFile),
            $"{fileName} is a core contract and should not be owned by the facade package.");
    }

    [Theory]
    [MemberData(nameof(GetRendererNeutralCoreContractFiles))]
    public void ArchitecturePlanNamesRendererNeutralCoreContracts(string fileName)
    {
        var plan = File.ReadAllText(Path.Combine(
            GetRepositoryRoot().FullName,
            "docs",
            "manual",
            "papercraft-architecture-plan.md"));
        var typeName = Path.GetFileNameWithoutExtension(fileName);

        Assert.Contains(typeName, plan, StringComparison.Ordinal);
    }

    private static void AssertImportsScaffoldProps(XDocument project)
    {
        var import = project.Root
            ?.Elements("Import")
            .SingleOrDefault((q) => string.Equals(
                q.Attribute("Project")?.Value,
                @"..\PapercraftPackageScaffold.props",
                StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(import);
    }

    private static IReadOnlyCollection<string> GetPackageReferences(XDocument project)
        => project.Root
               ?.Elements("ItemGroup")
               .Elements("PackageReference")
               .Select((q) => q.Attribute("Include")?.Value)
               .Where((q) => !string.IsNullOrWhiteSpace(q))
               .Cast<string>()
               .ToArray()
           ?? Array.Empty<string>();

    public static TheoryData<string> GetRendererNeutralCoreContractFiles()
    {
        var data = new TheoryData<string>();
        foreach (var fileName in RendererNeutralCoreContractFiles)
        {
            data.Add(fileName);
        }

        return data;
    }

    private static IEnumerable<FileInfo> GetProjectSourceFiles(DirectoryInfo projectDirectory)
        => projectDirectory
            .EnumerateFiles("*", SearchOption.AllDirectories)
            .Where((file) => !IsGeneratedOutput(file)
                             && string.Equals(file.Extension, ".cs", StringComparison.OrdinalIgnoreCase)
                             && !string.Equals(file.Extension, ".dll", StringComparison.OrdinalIgnoreCase)
                             && !string.Equals(file.Extension, ".pdb", StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyCollection<string> GetProjectReferences(XDocument project)
        => project.Root
               ?.Elements("ItemGroup")
               .Elements("ProjectReference")
               .Select((q) => q.Attribute("Include")?.Value)
               .Where((q) => !string.IsNullOrWhiteSpace(q))
               .Cast<string>()
               .ToArray()
           ?? Array.Empty<string>();

    private static string? GetProperty(XDocument document, string name)
        => document.Root
            ?.Elements("PropertyGroup")
            .Elements(name)
            .SingleOrDefault()
            ?.Value;

    private static bool IsGeneratedOutput(FileInfo file)
    {
        var relativePath = Path.GetRelativePath(GetRepositoryRoot().FullName, file.FullName);
        return relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                           .Any((q) => string.Equals(q, "bin", StringComparison.OrdinalIgnoreCase)
                                       || string.Equals(q, "obj", StringComparison.OrdinalIgnoreCase));
    }

    private static XDocument LoadProject(string packageId)
        => LoadDocument("source", packageId, $"{packageId}.csproj");

    private static XDocument LoadDocument(params string[] pathSegments)
        => XDocument.Load(Path.Combine(new[] { GetRepositoryRoot().FullName }.Concat(pathSegments).ToArray()));

    private static bool ReferencesSkiaSharp(string reference)
        => reference.Contains("SkiaSharp", StringComparison.OrdinalIgnoreCase);

    private static bool ReferencesLegacyCoreDependency(string reference)
        => reference.Contains("X39.Util", StringComparison.OrdinalIgnoreCase)
           || reference.Contains("JetBrains", StringComparison.OrdinalIgnoreCase)
           || reference.Contains("OpenTelemetry", StringComparison.OrdinalIgnoreCase)
           || reference.Contains("Microsoft.Extensions.Hosting", StringComparison.OrdinalIgnoreCase);

    private static bool ReferencesBarcodeDependency(string reference)
        => BarcodeDependencyNames.Any((dependencyName) => reference.Contains(dependencyName, StringComparison.OrdinalIgnoreCase));

    private static bool ReferencesBarcodePackage(string reference)
        => reference.Contains("X39.Solutions.Papercraft.Controls.QrCode", StringComparison.OrdinalIgnoreCase)
           || reference.Contains("X39.Solutions.Papercraft.Controls.ZXing", StringComparison.OrdinalIgnoreCase);

    private static DirectoryInfo GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "X39.Solutions.PdfTemplate.sln")))
        {
            directory = directory.Parent;
        }

        return directory
               ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
