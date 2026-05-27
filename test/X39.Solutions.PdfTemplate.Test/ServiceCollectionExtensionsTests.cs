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
using X39.Solutions.PdfTemplate.Transformers;
using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

namespace X39.Solutions.PdfTemplate.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddPdfTemplateServiceRegistersBuiltInControls()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var xmlReader = CreateReader("""<text>hello</text>""");

        var bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture);

        Dispose(bitmaps);
    }

    [Fact]
    public void AddPdfTemplateServiceRegistersSwitchTransformer()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService();
        using var serviceProvider = serviceCollection.BuildServiceProvider();

        Assert.Contains(serviceProvider.GetServices<ITransformer>(), (q) => q is SwitchTransformer);
    }

    [Fact]
    public async Task AddPdfTemplateServiceCanBeCalledTwiceWithoutDuplicatingDefaults()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService();
        serviceCollection.AddPdfTemplateService();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var xmlReader = CreateReader("""<text>hello</text>""");

        var bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture);

        Dispose(bitmaps);
    }

    [Fact]
    public async Task AddPdfTemplateServiceBuilderRegistersCustomServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService(
            (builder) => builder
                .ClearControls()
                .ClearTransformers()
                .AddControl<CustomControl>()
                .AddTransformer<CustomTransformer>()
                .AddFunction<CustomFunction>());
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var xmlReader = CreateReader("""
                                           @custom {
                                               <custom />
                                           }
                                           """);

        var bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture);

        Dispose(bitmaps);
        Assert.Single(serviceProvider.GetServices<IFunction>().OfType<CustomFunction>());
    }

    [Fact]
    public async Task ClearControlsRemovesBuiltInControls()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService((builder) => builder.ClearControls());
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        using var xmlReader = CreateReader("""<text>hello</text>""");

        await Assert.ThrowsAsync<FailedToCreateControlException>(
            () => generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ReplaceControlReplacesBuiltInControlWithSameXmlName()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService((builder) => builder.ReplaceControl<ReplacementTextControl>());
        using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var control = scope.ServiceProvider.GetRequiredService<IControlFactory>()
                           .Create(
                               Constants.ControlsNamespace,
                               "text",
                               new Dictionary<string, string>(),
                               null,
                               CultureInfo.InvariantCulture);

        Assert.IsType<ReplacementTextControl>(control);
    }

    [Fact]
    public void DuplicateControlNamesFailDeterministically()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService(
            (builder) => builder
                .ClearControls()
                .AddControl<CustomControl>()
                .AddControl<DuplicateCustomControl>());
        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredService<ControlRegistry>());
        Assert.Contains("custom", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DuplicateTransformerNamesFailDeterministically()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService(
            (builder) => builder
                .ClearTransformers()
                .AddTransformer<CustomTransformer>()
                .AddTransformer<DuplicateCustomTransformer>());
        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredService<Generator>());
        Assert.Contains("custom", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConverterDependenciesResolveFromCurrentServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService(
            (builder) => builder
                .ClearControls()
                .AddControl<ConverterControl>());
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

    [Control(Constants.ControlsNamespace, "text")]
    private sealed class ReplacementTextControl : IControl
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

    private sealed class CustomFunction : IFunction
    {
        public string Name => "customFunction";

        public int Arguments => 0;

        public bool IsVariadic => false;

        public ValueTask<object?> ExecuteAsync(
            CultureInfo cultureInfo,
            object?[] arguments,
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult<object?>("custom");
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
