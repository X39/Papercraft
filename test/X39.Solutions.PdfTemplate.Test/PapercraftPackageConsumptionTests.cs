using System.Diagnostics;
using System.Security;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftPackageConsumptionTests
{
    private static readonly TimeSpan BuildTimeout = TimeSpan.FromMinutes(2);

    [Fact]
    public async Task CompatibilityConsumerCompilesLegacyPdfTemplateEntryPoints()
    {
        using var project = TemporaryConsumerProject.Create(
            "CompatibilityLegacyConsumer",
            new[] { ProjectPath("source", "X39.Solutions.PdfTemplate", "X39.Solutions.PdfTemplate.csproj") },
            """
            using System.Globalization;
            using System.IO;
            using System.Threading.Tasks;
            using System.Xml;
            using Microsoft.Extensions.DependencyInjection;
            using SkiaSharp;
            using X39.Solutions.PdfTemplate;

            namespace CompatibilityLegacyConsumer;

            public static class LegacySmoke
            {
                public static Task ConfigureAndRenderAsync(
                    IServiceCollection services,
                    Generator generator,
                    Stream output,
                    XmlReader reader)
                {
                    services.AddPdfTemplateService();
                    return generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);
                }

                public static Task<IReadOnlyCollection<SKBitmap>> RenderBitmapsAsync(
                    Generator generator,
                    XmlReader reader)
                    => generator.GenerateBitmapsAsync(reader, CultureInfo.InvariantCulture);
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Rendering.SkiaSharp");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("SkiaSharp");
    }

    [Fact]
    public async Task CompatibilityConsumerCompilesPapercraftFacadeEntryPoints()
    {
        using var project = TemporaryConsumerProject.Create(
            "CompatibilityPapercraftConsumer",
            new[] { ProjectPath("source", "X39.Solutions.PdfTemplate", "X39.Solutions.PdfTemplate.csproj") },
            """
            using System;
            using System.Globalization;
            using System.IO;
            using System.Threading.Tasks;
            using System.Xml;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft;

            namespace CompatibilityPapercraftConsumer;

            public static class PapercraftSmoke
            {
                public static Task ConfigureDefaultAndRenderAsync(
                    IServiceCollection services,
                    PapercraftRenderer renderer,
                    Stream output,
                    XmlReader reader)
                {
                    services.AddPapercraft();
                    return renderer.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);
                }

                public static void ConfigureCoreOnly(IServiceCollection services)
                    => services.AddPapercraftCore();
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Rendering.SkiaSharp");
    }

    [Fact]
    public async Task FacadePackageConsumerResolvesGeneratorAndGeneratesPdf()
    {
        using var project = TemporaryConsumerProject.Create(
            "PapercraftFacadeConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft", "X39.Solutions.Papercraft.csproj") },
            """
            using System;
            using System.Globalization;
            using System.IO;
            using System.Threading.Tasks;
            using System.Xml;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft;

            namespace PapercraftFacadeConsumer;

            public static class Program
            {
                public static async Task Main()
                {
                    var services = new ServiceCollection();
                    services.AddPapercraft();

                    await using var serviceProvider = services.BuildServiceProvider();
                    var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
                    await using var output = new MemoryStream();
                    using var reader = XmlReader.Create(new StringReader(
                        @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <template xmlns=""X39.Solutions.PdfTemplate.Controls"">
                            <body>
                                <text>Facade package consumer</text>
                            </body>
                        </template>"));

                    await generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);

                    var bytes = output.ToArray();
                    if (bytes.Length < 4
                        || bytes[0] != (byte) '%'
                        || bytes[1] != (byte) 'P'
                        || bytes[2] != (byte) 'D'
                        || bytes[3] != (byte) 'F')
                    {
                        throw new InvalidOperationException("Papercraft facade consumer did not generate a PDF.");
                    }

                    var pageStreams = new List<MemoryStream>();
                    using var rasterReader = XmlReader.Create(new StringReader(
                        @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <template xmlns=""X39.Solutions.PdfTemplate.Controls"">
                            <body>
                                <text>Facade package raster consumer</text>
                            </body>
                        </template>"));
                    await generator.RenderRasterPagesAsync(
                        rasterReader,
                        new RasterPageRenderOutput(
                            PapercraftMediaTypes.ImagePng,
                            (info, _) =>
                            {
                                if (info.PageNumber < 1)
                                {
                                    throw new InvalidOperationException("Papercraft raster page metadata is invalid.");
                                }

                                var stream = new MemoryStream();
                                pageStreams.Add(stream);
                                return ValueTask.FromResult<Stream>(stream);
                            },
                            leaveStreamsOpen: true),
                        CultureInfo.InvariantCulture);

                    if (pageStreams.Count is 0)
                    {
                        throw new InvalidOperationException("Papercraft facade consumer did not generate raster pages.");
                    }

                    var png = pageStreams[0].ToArray();
                    if (png.Length < 8
                        || png[0] != 0x89
                        || png[1] != 0x50
                        || png[2] != 0x4E
                        || png[3] != 0x47)
                    {
                        throw new InvalidOperationException("Papercraft facade consumer did not generate PNG raster output.");
                    }
                }
            }
            """,
            isExecutable: true,
            packageReferences: new[]
            {
                new PackageReferenceSpec("Microsoft.Extensions.DependencyInjection", "10.0.8"),
                new PackageReferenceSpec("SkiaSharp.NativeAssets.Linux", "3.119.4"),
            });

        await project.BuildAsync();
        await project.RunAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Rendering.SkiaSharp");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("SkiaSharp");
    }

    [Fact]
    public async Task FacadePackageConsumerResolvesPapercraftSessionAndRendersTargets()
    {
        using var project = TemporaryConsumerProject.Create(
            "PapercraftFacadeSessionConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft", "X39.Solutions.Papercraft.csproj") },
            """
            using System;
            using System.Globalization;
            using System.IO;
            using System.Threading.Tasks;
            using System.Xml;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft;

            namespace PapercraftFacadeSessionConsumer;

            public static class Program
            {
                public static async Task Main()
                {
                    var services = new ServiceCollection();
                    services.AddPapercraft();

                    await using var serviceProvider = services.BuildServiceProvider();
                    var papercraft = serviceProvider.GetRequiredService<Papercraft>();
                    await using var session = papercraft.CreateSession();
                    session.TemplateData.SetVariable("Name", "session diagnostics");

                    await using var pdf = new MemoryStream();
                    using (var reader = CreateReader("<text>Session PDF</text>"))
                    {
                        await session.RenderAsync(
                            reader,
                            new RenderOutput(RenderTarget.Pdf, pdf),
                            CultureInfo.InvariantCulture);
                    }

                    var pdfBytes = pdf.ToArray();
                    if (pdfBytes.Length < 4
                        || pdfBytes[0] != (byte) '%'
                        || pdfBytes[1] != (byte) 'P'
                        || pdfBytes[2] != (byte) 'D'
                        || pdfBytes[3] != (byte) 'F')
                    {
                        throw new InvalidOperationException("Papercraft session consumer did not generate a PDF.");
                    }

                    PapercraftRenderResult lowered;
                    using (var reader = CreateReader("<text>@Name</text>"))
                    {
                        lowered = await session.RenderAsync(
                            reader,
                            RenderTarget.LoweredXml,
                            CultureInfo.InvariantCulture);
                    }

                    if (!lowered.ReadText().Contains("session diagnostics", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("Papercraft session consumer did not expose lowered XML text.");
                    }

                    var pageStreams = new List<MemoryStream>();
                    using (var reader = CreateReader("<text>Session raster</text>"))
                    {
                        await session.RenderRasterPagesAsync(
                            reader,
                            new RasterPageRenderOutput(
                                PapercraftMediaTypes.ImagePng,
                                (info, _) =>
                                {
                                    if (info.PageNumber < 1)
                                    {
                                        throw new InvalidOperationException("Papercraft raster page metadata is invalid.");
                                    }

                                    var stream = new MemoryStream();
                                    pageStreams.Add(stream);
                                    return ValueTask.FromResult<Stream>(stream);
                                },
                                leaveStreamsOpen: true),
                            CultureInfo.InvariantCulture);
                    }

                    if (pageStreams.Count is 0)
                    {
                        throw new InvalidOperationException("Papercraft session consumer did not generate raster pages.");
                    }

                    var png = pageStreams[0].ToArray();
                    if (png.Length < 8
                        || png[0] != 0x89
                        || png[1] != 0x50
                        || png[2] != 0x4E
                        || png[3] != 0x47)
                    {
                        throw new InvalidOperationException("Papercraft session consumer did not generate PNG raster output.");
                    }
                }

                private static XmlReader CreateReader(string body)
                    => XmlReader.Create(new StringReader(
                        $@"<?xml version=""1.0"" encoding=""utf-8""?>
                        <template xmlns=""X39.Solutions.PdfTemplate.Controls"">
                            <body>
                                {body}
                            </body>
                        </template>"));
            }
            """,
            isExecutable: true,
            packageReferences: new[]
            {
                new PackageReferenceSpec("Microsoft.Extensions.DependencyInjection", "10.0.8"),
                new PackageReferenceSpec("SkiaSharp.NativeAssets.Linux", "3.119.4"),
            });

        await project.BuildAsync();
        await project.RunAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Rendering.SkiaSharp");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("SkiaSharp");
    }

    [Fact]
    public async Task SkiaSharpRendererConsumerCompilesExplicitRendererEntryPoint()
    {
        using var project = TemporaryConsumerProject.Create(
            "SkiaSharpRendererConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft.Rendering.SkiaSharp", "X39.Solutions.Papercraft.Rendering.SkiaSharp.csproj") },
            """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft.Rendering.SkiaSharp;

            namespace SkiaSharpRendererConsumer;

            public static class SkiaSharpRendererSmoke
            {
                public static void ConfigureExplicitRenderer(IServiceCollection services)
                    => services.AddPapercraftSkiaSharpRenderer();

                public static Type ExplicitRendererType => typeof(SkiaSharpRenderBackend);
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Rendering.SkiaSharp");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("SkiaSharp");
    }

    [Fact]
    public async Task SvgRendererConsumerCompilesExplicitRendererEntryPoint()
    {
        using var project = TemporaryConsumerProject.Create(
            "SvgRendererConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft.Rendering.Svg", "X39.Solutions.Papercraft.Rendering.Svg.csproj") },
            """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft.Rendering.Svg;

            namespace SvgRendererConsumer;

            public static class SvgRendererSmoke
            {
                public static void ConfigureExplicitRenderer(IServiceCollection services)
                    => services.AddPapercraftSvgRenderer();

                public static Type ExplicitRendererType => typeof(SvgRenderBackend);
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Rendering.Svg");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsDoesNotContainLibrary("SkiaSharp");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
    }

    [Fact]
    public async Task PdfSharpRendererConsumerCompilesExplicitRendererEntryPoint()
    {
        using var project = TemporaryConsumerProject.Create(
            "PdfSharpRendererConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft.Rendering.PdfSharp", "X39.Solutions.Papercraft.Rendering.PdfSharp.csproj") },
            """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft.Rendering.PdfSharp;

            namespace PdfSharpRendererConsumer;

            public static class PdfSharpRendererSmoke
            {
                public static void ConfigureExplicitRenderer(IServiceCollection services)
                    => services.AddPapercraftPdfSharpRenderer();

                public static Type ExplicitRendererType => typeof(PdfSharpRenderBackend);
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Rendering.PdfSharp");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsDoesNotContainLibrary("SkiaSharp");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("PDFsharp");
    }

    [Fact]
    public async Task PdfSharpRendererConsumerResolvesRendererAndGeneratesPdfWithExplicitBackend()
    {
        using var project = TemporaryConsumerProject.Create(
            "PdfSharpRendererRuntimeConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft.Rendering.PdfSharp", "X39.Solutions.Papercraft.Rendering.PdfSharp.csproj") },
            """
            using System;
            using System.Globalization;
            using System.IO;
            using System.Threading.Tasks;
            using System.Xml;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft;
            using X39.Solutions.Papercraft.Rendering.PdfSharp;

            namespace PdfSharpRendererRuntimeConsumer;

            public static class Program
            {
                public static async Task Main()
                {
                    var services = new ServiceCollection();
                    services.AddPapercraftPdfSharpRenderer();

                    await using var serviceProvider = services.BuildServiceProvider();
                    var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
                    await using var output = new MemoryStream();
                    using var reader = XmlReader.Create(new StringReader(
                        @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <template xmlns=""X39.Solutions.PdfTemplate.Controls"">
                            <body>
                                <text>PDFsharp runtime consumer</text>
                            </body>
                        </template>"));

                    await renderer.GeneratePdfAsync(
                        output,
                        reader,
                        CultureInfo.InvariantCulture,
                        new PapercraftRenderOptions
                        {
                            BackendId = PdfSharpRenderBackend.RendererId,
                            DocumentOptions = DocumentOptions.Default,
                        });

                    var bytes = output.ToArray();
                    if (bytes.Length < 4
                        || bytes[0] != (byte) '%'
                        || bytes[1] != (byte) 'P'
                        || bytes[2] != (byte) 'D'
                        || bytes[3] != (byte) 'F')
                    {
                        throw new InvalidOperationException("PDFsharp consumer did not generate a PDF.");
                    }
                }
            }
            """,
            isExecutable: true,
            packageReferences: new[]
            {
                new PackageReferenceSpec("Microsoft.Extensions.DependencyInjection", "10.0.8"),
            });

        await project.BuildAsync();
        await project.RunAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Rendering.PdfSharp");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsDoesNotContainLibrary("SkiaSharp");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("PDFsharp");
    }

    [Fact]
    public async Task EscPosRendererConsumerCompilesExplicitRendererEntryPoint()
    {
        using var project = TemporaryConsumerProject.Create(
            "EscPosRendererConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft.Rendering.EscPos", "X39.Solutions.Papercraft.Rendering.EscPos.csproj") },
            """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft.Rendering.EscPos;

            namespace EscPosRendererConsumer;

            public static class EscPosRendererSmoke
            {
                public static void ConfigureExplicitRenderer(IServiceCollection services)
                    => services.AddPapercraftEscPosRenderer();

                public static Type ExplicitRendererType => typeof(EscPosRenderBackend);
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Rendering.EscPos");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsDoesNotContainLibrary("SkiaSharp");
        project.AssertAssetsDoesNotContainLibrary("PDFsharp");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
    }

    [Fact]
    public async Task CorePackageConsumerCompilesRendererNeutralContractsWithoutSkiaSharp()
    {
        using var project = TemporaryConsumerProject.Create(
            "CoreOnlyConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft.Core", "X39.Solutions.Papercraft.Core.csproj") },
            """
            using System.Globalization;
            using System.Threading;
            using System.Threading.Tasks;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft;
            using X39.Solutions.Papercraft.Abstraction;
            using X39.Solutions.Papercraft.Data;
            using X39.Solutions.Papercraft.Services.TextService;

            namespace CoreOnlyConsumer;

            public static class CoreContractSmoke
            {
                public static RenderValidationResult ValidatePdfTarget()
                {
                    var capabilities = new RendererCapabilities(
                        "core-only",
                        "Core only",
                        RendererOutputKind.Pdf,
                        new[] { PapercraftMediaTypes.ApplicationPdf });

                    return capabilities.ValidateTarget(RenderTarget.Pdf);
                }

                public static PapercraftDocument PrepareDocument()
                    => new(
                        new[]
                        {
                            new PapercraftPage(
                                0,
                                1,
                                1,
                                new X39.Solutions.Papercraft.Data.Size(1, 1),
                                96 / 25.4F,
                                new X39.Solutions.Papercraft.Display.DisplayList()),
                        },
                        CultureInfo.InvariantCulture,
                        DocumentOptions.Default);

                public static PapercraftServiceBuilder ConfigureCore(IServiceCollection services)
                    => services.AddPapercraftCore();

                public static RasterPageRenderOutput CreateRasterOutput()
                    => new(
                        PapercraftMediaTypes.ImagePng,
                        static (_, _) => ValueTask.FromResult<Stream>(new MemoryStream()));
            }

            public sealed class CoreOnlyRenderer : IPapercraftRenderBackend
            {
                public RendererCapabilities Capabilities { get; } = new(
                    "core-only",
                    "Core only",
                    RendererOutputKind.Pdf,
                    new[] { PapercraftMediaTypes.ApplicationPdf });

                public ITextService TextService { get; } = new CoreOnlyTextService();

                public ValueTask<RenderValidationResult> ValidateAsync(
                    PapercraftDocument document,
                    RenderTarget target,
                    CancellationToken cancellationToken = default)
                    => ValueTask.FromResult(Capabilities.ValidateTarget(target));

                public ValueTask RenderAsync(
                    PapercraftDocument document,
                    RenderOutput output,
                    CancellationToken cancellationToken = default)
                    => ValueTask.CompletedTask;

                public ValueTask RenderRasterPagesAsync(
                    PapercraftDocument document,
                    RasterPageRenderOutput output,
                    CancellationToken cancellationToken = default)
                    => ValueTask.CompletedTask;
            }

            public sealed class CoreOnlyTextService : ITextService
            {
                public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
                    => new(Math.Max(1F, text.Length), Math.Max(1F, textStyle.FontSize));

                public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
                    => canvas.DrawText(textStyle, dpi, text.ToString(), 0F, textStyle.FontSize);
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsDoesNotContainLibrary("SkiaSharp");
    }

    [Fact]
    public async Task OpenTelemetryPackageConsumerCompilesHostRegistrationExtensions()
    {
        using var project = TemporaryConsumerProject.Create(
            "OpenTelemetryConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft.OpenTelemetry", "X39.Solutions.Papercraft.OpenTelemetry.csproj") },
            """
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;
            using X39.Solutions.Papercraft.OpenTelemetry;

            namespace OpenTelemetryConsumer;

            public static class OpenTelemetrySmoke
            {
                public static void Configure()
                {
                    var applicationBuilder = Host.CreateApplicationBuilder();
                    applicationBuilder.AddPapercraftOpenTelemetry();
                    applicationBuilder.Services.AddPapercraftOpenTelemetry();

                    var hostBuilder = Host.CreateDefaultBuilder();
                    hostBuilder.AddPapercraftOpenTelemetry();
                }
            }
            """,
            packageReferences: new[]
            {
                new PackageReferenceSpec("Microsoft.Extensions.Hosting", "10.0.8"),
            });

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.OpenTelemetry");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("OpenTelemetry");
        project.AssertAssetsContainLibrary("OpenTelemetry.Extensions.Hosting");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
        project.AssertAssetsDoesNotContainLibrary("SkiaSharp");
    }

    [Fact]
    public async Task QrCodeControlPackageConsumerCompilesWithoutZxing()
    {
        using var project = TemporaryConsumerProject.Create(
            "QrCodeControlConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft.Controls.QrCode", "X39.Solutions.Papercraft.Controls.QrCode.csproj") },
            """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft;
            using X39.Solutions.Papercraft.Controls.QrCode;
            using X39.Solutions.Papercraft.Controls.QrCode.Controls;

            namespace QrCodeControlConsumer;

            public static class QrCodeSmoke
            {
                public static PapercraftServiceBuilder Configure(IServiceCollection services)
                    => services.AddPapercraftCore().AddQrCodeControls();

                public static Type ControlType => typeof(QrCodeControl);
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Controls.QrCode");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("Net.Codecrete.QrCodeGenerator");
        project.AssertAssetsDoesNotContainLibrary("ZXing.Net");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
    }

    [Fact]
    public async Task ZxingControlPackageConsumerCompilesWithoutQrCodeGenerator()
    {
        using var project = TemporaryConsumerProject.Create(
            "ZxingControlConsumer",
            new[] { ProjectPath("source", "X39.Solutions.Papercraft.Controls.ZXing", "X39.Solutions.Papercraft.Controls.ZXing.csproj") },
            """
            using System;
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft;
            using X39.Solutions.Papercraft.Controls.ZXing;
            using X39.Solutions.Papercraft.Controls.ZXing.Controls;

            namespace ZxingControlConsumer;

            public static class ZxingSmoke
            {
                public static PapercraftServiceBuilder Configure(IServiceCollection services)
                    => services.AddPapercraftCore().AddZxingBarcodeControls();

                public static Type ControlType => typeof(BarcodeControl);
                public static Type AliasType => typeof(Code128BarcodeControl);
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Controls.ZXing");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("ZXing.Net");
        project.AssertAssetsDoesNotContainLibrary("Net.Codecrete.QrCodeGenerator");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
    }

    [Fact]
    public async Task CombinedBarcodeControlPackageConsumerCompilesWithBothDependencies()
    {
        using var project = TemporaryConsumerProject.Create(
            "CombinedBarcodeControlConsumer",
            new[]
            {
                ProjectPath("source", "X39.Solutions.Papercraft.Controls.QrCode", "X39.Solutions.Papercraft.Controls.QrCode.csproj"),
                ProjectPath("source", "X39.Solutions.Papercraft.Controls.ZXing", "X39.Solutions.Papercraft.Controls.ZXing.csproj"),
            },
            """
            using Microsoft.Extensions.DependencyInjection;
            using X39.Solutions.Papercraft;
            using X39.Solutions.Papercraft.Controls.QrCode;
            using X39.Solutions.Papercraft.Controls.ZXing;

            namespace CombinedBarcodeControlConsumer;

            public static class CombinedBarcodeSmoke
            {
                public static PapercraftServiceBuilder Configure(IServiceCollection services)
                    => services.AddPapercraftCore()
                        .AddQrCodeControls()
                        .AddZxingBarcodeControls();
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Controls.QrCode");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Controls.ZXing");
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("Net.Codecrete.QrCodeGenerator");
        project.AssertAssetsContainLibrary("ZXing.Net");
        project.AssertAssetsDoesNotContainLibrary("X39.Solutions.PdfTemplate");
    }

    [Theory]
    [InlineData("X39.Solutions.Papercraft.Rendering.SkiaSharp")]
    [InlineData("X39.Solutions.Papercraft")]
    public async Task FuturePackageConsumerCompilesThroughTransitiveCoreContracts(string packageId)
    {
        using var project = TemporaryConsumerProject.Create(
            $"{packageId}.Consumer",
            new[] { ProjectPath("source", packageId, $"{packageId}.csproj") },
            """
            using X39.Solutions.Papercraft;

            namespace FuturePackageConsumer;

            public static class FuturePackageSmoke
            {
                public static RendererOutputKind PdfKind => RendererOutputKind.Pdf;
            }
            """);

        await project.BuildAsync();
        project.AssertAssetsContainLibrary(packageId);
        project.AssertAssetsContainLibrary("X39.Solutions.Papercraft.Core");
        project.AssertAssetsContainLibrary("SkiaSharp");
    }

    private static string ProjectPath(params string[] pathSegments)
        => Path.Combine(new[] { GetRepositoryRoot().FullName }.Concat(pathSegments).ToArray());

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

    private sealed record PackageReferenceSpec(string Include, string Version);

    private sealed class TemporaryConsumerProject : IDisposable
    {
        private TemporaryConsumerProject(DirectoryInfo projectDirectory, FileInfo projectFile)
        {
            ProjectDirectory = projectDirectory;
            ProjectFile = projectFile;
        }

        private DirectoryInfo ProjectDirectory { get; }

        private FileInfo ProjectFile { get; }

        public static TemporaryConsumerProject Create(
            string name,
            IReadOnlyCollection<string> projectReferences,
            string source,
            bool isExecutable = false,
            IReadOnlyCollection<PackageReferenceSpec>? packageReferences = null)
        {
            var directory = Directory.CreateDirectory(Path.Combine(
                Path.GetTempPath(),
                "X39.Solutions.PdfTemplate.PackageConsumptionTests",
                $"{name}-{Guid.NewGuid():N}"));
            var projectFile = new FileInfo(Path.Combine(directory.FullName, $"{name}.csproj"));
            File.WriteAllText(
                projectFile.FullName,
                CreateProjectXml(projectReferences, isExecutable, packageReferences ?? Array.Empty<PackageReferenceSpec>()));
            File.WriteAllText(Path.Combine(directory.FullName, "Consumer.cs"), source);
            return new TemporaryConsumerProject(directory, projectFile);
        }

        public async Task BuildAsync()
        {
            using var process = Process.Start(CreateBuildStartInfo())
                                ?? throw new InvalidOperationException("Failed to start dotnet build.");
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit((int) BuildTimeout.TotalMilliseconds))
            {
                process.Kill(entireProcessTree: true);
                throw new TimeoutException($"dotnet build timed out for {ProjectFile.FullName}.");
            }

            var output = await outputTask.ConfigureAwait(false);
            var error = await errorTask.ConfigureAwait(false);
            Assert.True(
                process.ExitCode is 0,
                $"""
                dotnet build failed for {ProjectFile.FullName} with exit code {process.ExitCode}.

                STDOUT:
                {output}

                STDERR:
                {error}
                """);
        }

        public async Task RunAsync()
        {
            using var process = Process.Start(CreateRunStartInfo())
                                ?? throw new InvalidOperationException("Failed to start dotnet run.");
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit((int) BuildTimeout.TotalMilliseconds))
            {
                process.Kill(entireProcessTree: true);
                throw new TimeoutException($"dotnet run timed out for {ProjectFile.FullName}.");
            }

            var output = await outputTask.ConfigureAwait(false);
            var error = await errorTask.ConfigureAwait(false);
            Assert.True(
                process.ExitCode is 0,
                $"""
                dotnet run failed for {ProjectFile.FullName} with exit code {process.ExitCode}.

                STDOUT:
                {output}

                STDERR:
                {error}
                """);
        }

        public void AssertAssetsContainLibrary(string libraryName)
        {
            var assets = ReadAssetsFile();
            Assert.Contains($"\"{libraryName}/", assets, StringComparison.OrdinalIgnoreCase);
        }

        public void AssertAssetsDoesNotContainLibrary(string libraryName)
        {
            var assets = ReadAssetsFile();
            Assert.DoesNotContain($"\"{libraryName}/", assets, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            try
            {
                ProjectDirectory.Delete(recursive: true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static string CreateProjectXml(
            IReadOnlyCollection<string> projectReferences,
            bool isExecutable,
            IReadOnlyCollection<PackageReferenceSpec> packageReferences)
        {
            var references = string.Join(
                Environment.NewLine,
                projectReferences.Select(
                    (q) => $"""        <ProjectReference Include="{SecurityElement.Escape(q)}" />"""));
            var outputType = isExecutable
                ? $"        <OutputType>Exe</OutputType>{Environment.NewLine}"
                : string.Empty;
            var packageReferenceItemGroup = CreatePackageReferenceItemGroup(packageReferences);
            return $"""
                   <Project Sdk="Microsoft.NET.Sdk">
                       <PropertyGroup>
                           <TargetFramework>net10.0</TargetFramework>
                   {outputType}        <ImplicitUsings>enable</ImplicitUsings>
                           <Nullable>enable</Nullable>
                       </PropertyGroup>
                   {packageReferenceItemGroup}    <ItemGroup>
                   {references}
                       </ItemGroup>
                   </Project>
                   """;
        }

        private ProcessStartInfo CreateBuildStartInfo()
        {
            var startInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = ProjectDirectory.FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            startInfo.ArgumentList.Add("build");
            startInfo.ArgumentList.Add(ProjectFile.FullName);
            startInfo.ArgumentList.Add("--nologo");
            startInfo.ArgumentList.Add("--disable-build-servers");
            startInfo.ArgumentList.Add("-v");
            startInfo.ArgumentList.Add("minimal");
            startInfo.ArgumentList.Add("-nr:false");
            startInfo.ArgumentList.Add("/p:UseSharedCompilation=false");
            return startInfo;
        }

        private ProcessStartInfo CreateRunStartInfo()
        {
            var startInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = ProjectDirectory.FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            startInfo.ArgumentList.Add("run");
            startInfo.ArgumentList.Add("--project");
            startInfo.ArgumentList.Add(ProjectFile.FullName);
            startInfo.ArgumentList.Add("--no-build");
            return startInfo;
        }

        private static string CreatePackageReferenceItemGroup(
            IReadOnlyCollection<PackageReferenceSpec> packageReferences)
        {
            if (packageReferences.Count is 0)
                return string.Empty;

            var references = string.Join(
                Environment.NewLine,
                packageReferences.Select(
                    (q) =>
                        $"""        <PackageReference Include="{SecurityElement.Escape(q.Include)}" Version="{SecurityElement.Escape(q.Version)}" />"""));
            return $"""
                       <ItemGroup>
                   {references}
                       </ItemGroup>
                   
                   """;
        }

        private string ReadAssetsFile()
        {
            var assetsFile = Path.Combine(ProjectDirectory.FullName, "obj", "project.assets.json");
            Assert.True(File.Exists(assetsFile), $"Expected NuGet assets file at {assetsFile}.");
            return File.ReadAllText(assetsFile);
        }
    }
}
