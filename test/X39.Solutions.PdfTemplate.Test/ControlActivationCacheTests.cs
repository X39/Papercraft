using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Exceptions;
using X39.Solutions.Papercraft.Services;
using X39.Solutions.PdfTemplate.Exceptions;

namespace X39.Solutions.PdfTemplate.Test;

public class ControlActivationCacheTests
{
    [Fact]
    public void CreateControl_SkipsParameterMetadata_WhenNoParametersAndNoContent()
    {
        var control = new ControlActivationCache().CreateControl(
            new ServiceCollection().BuildServiceProvider(),
            typeof(FastPathControl),
            new Dictionary<string, string>(),
            null,
            CultureInfo.InvariantCulture);

        Assert.IsType<FastPathControl>(control);
    }

    [Fact]
    public void CreateControl_UsesControlConstructorAttribute()
    {
        var services = new ServiceCollection()
            .AddSingleton(new ConstructorDependency("dependency"))
            .BuildServiceProvider();

        var control = (ConstructorControl) new ControlActivationCache().CreateControl(
            services,
            typeof(ConstructorControl),
            new Dictionary<string, string>(),
            null,
            CultureInfo.InvariantCulture);

        Assert.Equal("dependency", control.Value);
    }

    [Fact]
    public void CreateControl_ThrowsForUnknownParameter()
    {
        var exception = Assert.Throws<ControlParameterIsNotExistingException>(
            () => new ControlActivationCache().CreateControl(
                new ServiceCollection().BuildServiceProvider(),
                typeof(ContentControl),
                new Dictionary<string, string> { ["UNKNOWN"] = "value" },
                null,
                CultureInfo.InvariantCulture));

        Assert.Equal(typeof(ContentControl), exception.ControlType);
        Assert.Collection(exception.MissingParameters, (parameter) => Assert.Equal("UNKNOWN", parameter));
        Assert.Collection(exception.AvailableParameters, (parameter) => Assert.Equal("TEXT", parameter));
    }

    [Fact]
    public void CreateControl_AppliesContentAfterAttributes()
    {
        var control = (ContentControl) new ControlActivationCache().CreateControl(
            new ServiceCollection().BuildServiceProvider(),
            typeof(ContentControl),
            new Dictionary<string, string> { ["TEXT"] = "attribute" },
            "content",
            CultureInfo.InvariantCulture);

        Assert.Equal("content", control.Text);
    }

    [Fact]
    public void CreateControl_ResolvesParameterConverterDependencies()
    {
        var services = new ServiceCollection()
            .AddSingleton(new ConverterDependency("prefix-"))
            .BuildServiceProvider();

        var control = (ConverterControl) new ControlActivationCache().CreateControl(
            services,
            typeof(ConverterControl),
            new Dictionary<string, string> { ["VALUE"] = "value" },
            null,
            CultureInfo.InvariantCulture);

        Assert.Equal("prefix-value", control.Value);
    }

    private sealed class FastPathControl : TestControl
    {
        [Parameter]
        public string BrokenParameter => throw new InvalidOperationException("Parameter metadata should not be built.");
    }

    private sealed record ConstructorDependency(string Value);

    private sealed class ConstructorControl : TestControl
    {
        public ConstructorControl()
        {
            Value = "parameterless";
        }

        [ControlConstructor]
        public ConstructorControl(ConstructorDependency dependency)
        {
            Value = dependency.Value;
        }

        public string Value { get; }
    }

    private sealed class ContentControl : TestControl
    {
        [Parameter(IsContent = true)]
        public string Text { get; private set; } = string.Empty;
    }

    private sealed class ConverterControl : TestControl
    {
        [Parameter(Converter = typeof(DependencyConverter))]
        public string Value { get; private set; } = string.Empty;
    }

    private sealed record ConverterDependency(string Prefix);

    private sealed class DependencyConverter : IParameterConverter<string>
    {
        private readonly string _prefix;

        public DependencyConverter()
        {
            _prefix = "wrong-";
        }

        [ParameterConverterConstructor]
        public DependencyConverter(ConverterDependency dependency)
        {
            _prefix = dependency.Prefix;
        }

        public string Convert(string value, string? format, CultureInfo cultureInfo) => _prefix + value;
    }

    private abstract class TestControl : IControl
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
}
