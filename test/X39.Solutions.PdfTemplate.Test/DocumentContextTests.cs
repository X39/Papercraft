using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services.ResourceResolver;

namespace X39.Solutions.PdfTemplate.Test;

public class DocumentContextTests
{
    [Fact]
    public async Task ResourceResolverReceivesDocumentContext()
    {
        var resolver = new RecordingResourceResolver();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateControl<ImageControl>();
        serviceCollection.AddSingleton<IResourceResolver>(resolver);
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        var context = new PrintRequestContext("print-request-1");
        using var xmlReader = CreateReader("""<image source="logo:customer" />""");

        var bitmaps = await generator.GenerateBitmapsAsync(
            xmlReader,
            CultureInfo.InvariantCulture,
            new DocumentOptions { Context = context });
        Dispose(bitmaps);

        Assert.Equal("logo:customer", resolver.Source);
        Assert.Same(context, resolver.Context);
        Assert.Equal(1, resolver.ResolveCount);
    }

    [Fact]
    public async Task InitializeAsyncReceivesDocumentContext()
    {
        var recorder = new InitializationContextRecorder();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateControl<ContextRecordingControl>();
        serviceCollection.AddSingleton(recorder);
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        var context = new PrintRequestContext("print-request-2");
        using var xmlReader = CreateReader("""<context-recording-control />""");

        var bitmaps = await generator.GenerateBitmapsAsync(
            xmlReader,
            CultureInfo.InvariantCulture,
            new DocumentOptions { Context = context });
        Dispose(bitmaps);

        Assert.Same(context, recorder.Context);
        Assert.Equal(1, recorder.InitializeCount);
    }

    [Fact]
    public async Task DefaultImageResolverAcceptsNullContext()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateControl<ImageControl>();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var xmlReader = CreateReader($"""<image source="{Convert.ToBase64String(PngBytes)}" />""");

        var bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture);

        try
        {
            Assert.NotEmpty(bitmaps);
        }
        finally
        {
            Dispose(bitmaps);
        }
    }

    private static XmlReader CreateReader(string body)
    {
        var xml = $$"""
                    <?xml version="1.0" encoding="utf-8"?>
                    <template xmlns="{{Constants.ControlsNamespace}}">
                        <body>
                            {{body}}
                        </body>
                    </template>
                    """;
        return XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
    }

    private static void Dispose(IEnumerable<SKBitmap> bitmaps)
    {
        foreach (var bitmap in bitmaps)
        {
            bitmap.Dispose();
        }
    }

    private static readonly byte[] PngBytes = CreatePngBytes();

    private static byte[] CreatePngBytes()
    {
        using var bitmap = new SKBitmap(1, 1);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private sealed record PrintRequestContext(string Id);

    private sealed class RecordingResourceResolver : IResourceResolver
    {
        public object? Context { get; private set; }
        public string? Source { get; private set; }
        public int ResolveCount { get; private set; }

        public ValueTask<byte[]> ResolveImageAsync(
            string source,
            object? context,
            CancellationToken cancellationToken = default)
        {
            Source = source;
            Context = context;
            ResolveCount++;
            return ValueTask.FromResult(PngBytes);
        }
    }

    private sealed class InitializationContextRecorder
    {
        public object? Context { get; set; }
        public int InitializeCount { get; set; }
    }

    [Control(Constants.ControlsNamespace, "context-recording-control")]
    private sealed class ContextRecordingControl : IControl, IInitializeControlAsync
    {
        private readonly InitializationContextRecorder _recorder;

        public ContextRecordingControl(InitializationContextRecorder recorder)
        {
            _recorder = recorder;
        }

        public Task InitializeControlAsync(object? context, CancellationToken cancellationToken = default)
        {
            _recorder.Context = context;
            _recorder.InitializeCount++;
            return Task.CompletedTask;
        }

        public Size Measure(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
            => Size.Zero;

        public Size Arrange(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
            => Size.Zero;

        public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
            => Size.Zero;
    }
}
