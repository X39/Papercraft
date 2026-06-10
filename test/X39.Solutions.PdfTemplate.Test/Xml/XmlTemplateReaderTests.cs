using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Exceptions;
using X39.Solutions.Papercraft.Xml;
using X39.Solutions.PdfTemplate.Exceptions;

namespace X39.Solutions.PdfTemplate.Test.Xml;

public class XmlTemplateReaderTests
{
    [Fact]
    public async Task ElementsWithoutNamespaceUseBuiltInControlNamespace()
    {
        const string template = """
                                <?xml version="1.0" encoding="utf-8"?>
                                <template>
                                    <body>
                                        <text>Hello</text>
                                    </body>
                                </template>
                                """;
        var templateReader = new XmlTemplateReader(
            default,
            CultureInfo.InvariantCulture,
            new TemplateData(),
            ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);

        var node = await templateReader.ReadAsync(xmlReader);

        Assert.Equal(Constants.ControlsNamespace, node.NodeNamespace);
        Assert.Equal(Constants.ControlsNamespace, node["body", Constants.ControlsNamespace]!.NodeNamespace);
        Assert.Equal(
            Constants.ControlsNamespace,
            node["body", Constants.ControlsNamespace]!["text", Constants.ControlsNamespace]!.NodeNamespace);
    }

    [Fact]
    public async Task CustomDefaultNamespaceIsPreserved()
    {
        const string customNamespace = "MyApp.PdfControls";
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <template xmlns="{customNamespace}">
                                     <body>
                                         <approvalStamp/>
                                     </body>
                                 </template>
                                 """;
        var templateReader = new XmlTemplateReader(
            default,
            CultureInfo.InvariantCulture,
            new TemplateData(),
            ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);

        var node = await templateReader.ReadAsync(xmlReader);

        Assert.Equal(customNamespace, node.NodeNamespace);
        Assert.Equal(customNamespace, node["body", customNamespace]!.NodeNamespace);
        Assert.Equal(customNamespace, node["body", customNamespace]!["approvalStamp", customNamespace]!.NodeNamespace);
    }

    [Fact]
    public async Task PrefixedControlNameIsRejected()
    {
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <template>
                                     <body xmlns:pt="{Constants.ControlsNamespace}">
                                         <pt:text>Hello</pt:text>
                                     </body>
                                 </template>
                                 """;
        var templateReader = new XmlTemplateReader(
            default,
            CultureInfo.InvariantCulture,
            new TemplateData(),
            ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);

        await Assert.ThrowsAsync<XmlNodeNameException>(() => templateReader.ReadAsync(xmlReader));
    }

    [Fact]
    public async Task CustomDefaultNamespaceDoesNotActivateBuiltInControls()
    {
        const string customNamespace = "MyApp.PdfControls";
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <template xmlns="{customNamespace}">
                                     <body>
                                         <text>Hello</text>
                                     </body>
                                 </template>
                                 """;
        var root = await ReadTemplateAsync(template);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var exception = await Assert.ThrowsAsync<FailedToCreateControlException>(
            () => Template.CreateAsync(
                root,
                scope.ServiceProvider.GetRequiredService<IControlFactory>(),
                CultureInfo.InvariantCulture,
                null,
                default));

        Assert.Contains($"{customNamespace}:text", exception.InnerException?.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CustomControlInBuiltInNamespaceActivatesWithoutPrefix()
    {
        const string template = """
                                <?xml version="1.0" encoding="utf-8"?>
                                <template>
                                    <body>
                                        <namespace-test/>
                                    </body>
                                </template>
                                """;
        var root = await ReadTemplateAsync(template);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService((builder) => builder.AddControl<NamespaceTestControl>());
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        await using var parsedTemplate = await Template.CreateAsync(
            root,
            scope.ServiceProvider.GetRequiredService<IControlFactory>(),
            CultureInfo.InvariantCulture,
            null,
            default);

        Assert.IsType<NamespaceTestControl>(Assert.Single(parsedTemplate.BodyControls));
    }

    [Fact]
    public async Task EffectiveStyle()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <effectiveStyleTest xmlns="{ns}">
                                     <nested>
                                         <nested.style>
                                             <line margin="2px" padding="2px"/>
                                         </nested.style>
                                         <line margin="3px"/>
                                     </nested>
                                     <effectiveStyleTest.style>
                                         <line margin="1px" padding="1px"/>
                                     </effectiveStyleTest.style>
                                     <line margin="4px"/>
                                 </effectiveStyleTest>
                                 """;
        var templateReader = new XmlTemplateReader(default, CultureInfo.InvariantCulture, new TemplateData(), ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var node = await templateReader.ReadAsync(xmlReader);

        // Assert all nodes are present
        Assert.Equal("effectiveStyleTest", node.NodeName);
        Assert.Equal(ns, node.NodeNamespace);
        Assert.Equal(2, node.Children.Count);
        Assert.Equal("nested", node.Children.ElementAt(0).NodeName);
        Assert.Equal(ns, node.Children.ElementAt(0).NodeNamespace);
        Assert.Single(node.Children.ElementAt(0).Children);
        Assert.Equal("line", node.Children.ElementAt(0).Children.ElementAt(0).NodeName);
        Assert.Equal(ns, node.Children.ElementAt(0).Children.ElementAt(0).NodeNamespace);
        Assert.Equal("line", node.Children.ElementAt(1).NodeName);
        Assert.Equal(ns, node.Children.ElementAt(1).NodeNamespace);

        // Assert all effective styles are as expected
        Assert.Equal("3px", node["nested", ns]!["line", ns]!.Attributes["MARGIN"]);
        Assert.Equal("2px", node["nested", ns]!["line", ns]!.Attributes["PADDING"]);
        Assert.Equal("4px", node["line", ns]!.Attributes["MARGIN"]);
        Assert.Equal("1px", node["line", ns]!.Attributes["PADDING"]);
    }

    [Fact]
    public async Task LowestLevelStyleAppliedLast()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <styleTest xmlns="{ns}">
                                     <styleTest.style>
                                         <line margin="1px" padding="1px"/>
                                     </styleTest.style>
                                     <nested>
                                         <nested.style>
                                             <line margin="2px" padding="2px"/>
                                         </nested.style>
                                         <line margin="0px"/>
                                     </nested>
                                    <line margin="0px"/>
                                 </styleTest>
                                 """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            ArraySegment<ITransformer>.Empty
        );
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var       node      = await templateReader.ReadAsync(xmlReader);

        // Assert effective styles are as expected
        Assert.Equal("0px", node["nested", ns]!["line", ns]!.Attributes["MARGIN"]);
        Assert.Equal("2px", node["nested", ns]!["line", ns]!.Attributes["PADDING"]);
        Assert.Equal("0px", node["line", ns]!.Attributes["MARGIN"]);
        Assert.Equal("1px", node["line", ns]!.Attributes["PADDING"]);
    }

    [Fact]
    public async Task StylesApplyToFollowingSiblingsAndAttributesAreCaseInsensitive()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <styleCaseTest xmlns="{ns}">
                                     <styleCaseTest.style>
                                         <line Margin="1px"/>
                                     </styleCaseTest.style>
                                     <line/>
                                     <styleCaseTest.style>
                                         <line Padding="2px"/>
                                     </styleCaseTest.style>
                                     <line margin="3px"/>
                                 </styleCaseTest>
                                 """;

        var node = await ReadTemplateAsync(template);

        var firstLine = node.Children.ElementAt(0);
        var secondLine = node.Children.ElementAt(1);
        Assert.Equal("1px", firstLine.Attributes["margin"]);
        Assert.False(firstLine.Attributes.ContainsKey("padding"));
        Assert.Equal("3px", secondLine.Attributes["MARGIN"]);
        Assert.Equal("2px", secondLine.Attributes["padding"]);
    }

    [Fact]
    public async Task NoDotInName()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <noDotInNameTest xmlns="{ns}">
                                     <invalid.element margin="4px"/>
                                 </noDotInNameTest>
                                 """;
        var templateReader = new XmlTemplateReader(default, CultureInfo.InvariantCulture, new TemplateData(), ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        await Assert.ThrowsAsync<XmlNodeNameException>(() => templateReader.ReadAsync(xmlReader));
    }

    [Fact]
    public async Task StyleMustBeEmptyTag()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <styleMustBeEmptyTagTest xmlns="{ns}">
                                    <styleMustBeEmptyTagTest.style>
                                        <nonEmptyElement>
                                            <someElement/>
                                        </nonEmptyElement>
                                    </styleMustBeEmptyTagTest.style>
                                 </styleMustBeEmptyTagTest>
                                 """;
        var templateReader = new XmlTemplateReader(default, CultureInfo.InvariantCulture, new TemplateData(), ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        await Assert.ThrowsAsync<XmlStyleInformationCannotNestException>(() => templateReader.ReadAsync(xmlReader));
    }

    [Fact]
    public async Task ForLoop()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    @for i from 0 to 10 {
                                        <text>@i</text>
                                    }
                                 </styleMustBeEmptyTagTest>
                                 """;
        var templateReader = new XmlTemplateReader(default, CultureInfo.InvariantCulture, new TemplateData(), new []{new Papercraft.Transformers.ForTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(10, nodeInformation.Children.Count);
    }

    private static async Task<XmlNodeInformation> ReadTemplateAsync(string template)
    {
        var templateReader = new XmlTemplateReader(
            default,
            CultureInfo.InvariantCulture,
            new TemplateData(),
            ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        return await templateReader.ReadAsync(xmlReader);
    }

    [Control(Constants.ControlsNamespace, "namespace-test")]
    private sealed class NamespaceTestControl : IControl
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
