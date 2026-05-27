using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Exceptions;
using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

namespace X39.Solutions.PdfTemplate.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddPdfTemplateDefaultsRegistersBuiltInControls()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateDefaults();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var xmlReader = CreateReader("""<text>hello</text>""");

        var bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture);

        Dispose(bitmaps);
    }

    [Fact]
    public async Task AddPdfTemplateControlRegistersCustomControl()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateControl<CustomControl>();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var xmlReader = CreateReader("""<custom />""");

        var bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture);

        Dispose(bitmaps);
    }

    [Fact]
    public async Task AddPdfTemplateTransformerRegistersCustomTransformer()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateControl<CustomControl>();
        serviceCollection.AddPdfTemplateTransformer<CustomTransformer>();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var xmlReader = CreateReader("""
                                           @custom {
                                               <custom />
                                           }
                                           """);

        var bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture);

        Dispose(bitmaps);
    }

    [Fact]
    public async Task MissingDefaultControlsFailDuringGeneration()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var xmlReader = CreateReader("""<text>hello</text>""");

        await Assert.ThrowsAsync<FailedToCreateControlException>(
            () => generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void DuplicateControlNamesFailDeterministically()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateControl<CustomControl>();
        serviceCollection.AddPdfTemplateControl<DuplicateCustomControl>();
        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredService<ControlRegistry>());
        Assert.Contains("custom", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DuplicateTransformerNamesFailDeterministically()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateTransformer<CustomTransformer>();
        serviceCollection.AddPdfTemplateTransformer<DuplicateCustomTransformer>();
        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredService<Generator>());
        Assert.Contains("custom", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConverterDependenciesResolveFromCurrentServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateControl<ConverterControl>();
        serviceCollection.AddSingleton(new ConverterDependency("prefix-"));
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var control = scope.ServiceProvider.GetRequiredService<IControlFactory>()
                           .Create(
                               Constants.ControlsNamespace,
                               "converter",
                               new Dictionary<string, string> { ["VALUE"] = "value" },
                               null,
                               CultureInfo.InvariantCulture);

        var converterControl = Assert.IsType<ConverterControl>(control);
        Assert.Equal("prefix-value", converterControl.Value);
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

    [Control(Constants.ControlsNamespace, "custom")]
    private class CustomControl : IControl
    {
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

    [Control(Constants.ControlsNamespace, "custom")]
    private sealed class DuplicateCustomControl : CustomControl;

    [Control(Constants.ControlsNamespace, "converter")]
    private sealed class ConverterControl : CustomControl
    {
        [Parameter(Converter = typeof(DependencyConverter))]
        public string? Value { get; private set; }
    }

    private sealed record ConverterDependency(string Prefix);

    private sealed class DependencyConverter : IParameterConverter<string>
    {
        private readonly ConverterDependency _dependency;

        public DependencyConverter(ConverterDependency dependency)
        {
            _dependency = dependency;
        }

        public string Convert(string value, string? format, CultureInfo cultureInfo)
            => _dependency.Prefix + value;
    }

    private class CustomTransformer : ITransformer
    {
        public string Name => "custom";

        public async IAsyncEnumerable<XmlNode> TransformAsync(
            CultureInfo cultureInfo,
            ITemplateData templateData,
            string remainingLine,
            IReadOnlyCollection<XmlNode> nodes,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            foreach (var node in nodes)
            {
                yield return node.DeepCopy();
            }
        }
    }

    private sealed class DuplicateCustomTransformer : CustomTransformer;
}
