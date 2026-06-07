using System.Globalization;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Abstraction.Controls;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Data.Compound;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.Papercraft.Exceptions;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Abstraction;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;
using X39.Solutions.Papercraft.Services;
using X39.Solutions.Papercraft.Services.PropertyAccessCache;
using X39.Solutions.Papercraft.Services.ResourceResolver;
using X39.Solutions.Papercraft.Services.TextService;
using X39.Solutions.Papercraft.Validators;
using X39.Solutions.Papercraft.Xml;
using X39.Solutions.PdfTemplate;
using X39.Solutions.PdfTemplate.Exceptions;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftCoreContractTests
{
    public static IEnumerable<object[]> RendererNeutralTypes
        => new[]
        {
            new object[] { typeof(RendererCapabilities) },
            new object[] { typeof(RendererFeatures) },
            new object[] { typeof(RendererOutputKind) },
            new object[] { typeof(RendererSupportLevel) },
            new object[] { typeof(RenderDiagnostic) },
            new object[] { typeof(RenderDiagnosticCodes) },
            new object[] { typeof(RenderFeatureUse) },
            new object[] { typeof(RenderValidationResult) },
            new object[] { typeof(RenderValidationException) },
            new object[] { typeof(RenderTarget) },
            new object[] { typeof(RenderOutput) },
            new object[] { typeof(RasterPageInfo) },
            new object[] { typeof(RasterPageRenderOutput) },
            new object[] { typeof(PapercraftMediaTypes) },
            new object[] { typeof(TemplateLocation) },
            new object[] { typeof(IPapercraftRenderBackend) },
            new object[] { typeof(IPapercraftTemplateDataAccessor) },
            new object[] { typeof(PapercraftRenderOptions) },
            new object[] { typeof(DocumentOptions) },
            new object[] { typeof(Constants) },
            new object[] { typeof(ControlRegistration) },
            new object[] { typeof(ControlRegistry) },
            new object[] { typeof(ControlFactory) },
            new object[] { typeof(global::X39.Solutions.Papercraft.Abstraction.Canvas) },
            new object[] { typeof(global::X39.Solutions.Papercraft.Abstraction.CanvasExtensions) },
            new object[] { typeof(IChainedTransformer) },
            new object[] { typeof(IContentControl) },
            new object[] { typeof(IControl) },
            new object[] { typeof(IControlFactory) },
            new object[] { typeof(IDeferredCanvas) },
            new object[] { typeof(IDrawableCanvas) },
            new object[] { typeof(IFunction) },
            new object[] { typeof(IImmediateCanvas) },
            new object[] { typeof(IInitializeControlAsync) },
            new object[] { typeof(INestedClauseTransformer) },
            new object[] { typeof(IParameterConverter<>) },
            new object[] { typeof(ITemplateData) },
            new object[] { typeof(ITransformer) },
            new object[] { typeof(TransformerChainClause) },
            new object[] { typeof(IChart) },
            new object[] { typeof(ControlAttribute) },
            new object[] { typeof(ControlConstructorAttribute) },
            new object[] { typeof(ParameterAttribute) },
            new object[] { typeof(ParameterConverterAttribute<>) },
            new object[] { typeof(ParameterConverterAttributeBase) },
            new object[] { typeof(ParameterConverterConstructorAttribute) },
            new object[] { typeof(Color) },
            new object[] { typeof(ColorConverter) },
            new object[] { typeof(Colors) },
            new object[] { typeof(ColumnLength) },
            new object[] { typeof(ColumnLengthConverter) },
            new object[] { typeof(EColumnUnit) },
            new object[] { typeof(Point) },
            new object[] { typeof(Rectangle) },
            new object[] { typeof(Size) },
            new object[] { typeof(Thickness) },
            new object[] { typeof(ThicknessConverter) },
            new object[] { typeof(Length) },
            new object[] { typeof(LengthConverter) },
            new object[] { typeof(ELengthUnit) },
            new object[] { typeof(TextStyle) },
            new object[] { typeof(Font) },
            new object[] { typeof(EFontStyle) },
            new object[] { typeof(EHorizontalAlignment) },
            new object[] { typeof(EOrientation) },
            new object[] { typeof(EVerticalAlignment) },
            new object[] { typeof(FontWeight) },
            new object[] { typeof(FontWeightConverter) },
            new object[] { typeof(FontWeights) },
            new object[] { typeof(FontWidth) },
            new object[] { typeof(FontWidths) },
            new object[] { typeof(FontStyleCompound) },
            new object[] { typeof(AreaIncompleteException) },
            new object[] { typeof(ContentControlDoesNotSupportChildrenException) },
            new object[] { typeof(ContentControlDoesNotSupportTheProvidedChildException) },
            new object[] { typeof(ControlParameterIsNotExistingException) },
            new object[] { typeof(EvaluationException) },
            new object[] { typeof(FailedToCreateControlException) },
            new object[] { typeof(FunctionExpressionNotFullyHandledException) },
            new object[] { typeof(FunctionNotFoundDuringEvaluationException) },
            new object[] { typeof(TransformationEvaluationFailedException) },
            new object[] { typeof(TransformationFunctionMissingClosingBracketException) },
            new object[] { typeof(TransformationFunctionNotFoundException) },
            new object[] { typeof(TransformationMissingClosingBracketException) },
            new object[] { typeof(TransformationMissingEndNodeBracketException) },
            new object[] { typeof(TransformationMissingOpeningBracketException) },
            new object[] { typeof(UnhandledXmlTemplateTransformationException) },
            new object[] { typeof(XmlNodeNameException) },
            new object[] { typeof(XmlStyleInformationCannotNestException) },
            new object[] { typeof(XmlTemplateReaderException) },
            new object[] { typeof(XmlTemplateTransformationException) },
            new object[] { typeof(ControlActivationCache) },
            new object[] { typeof(EncodedImageSizeReader) },
            new object[] { typeof(IPropertyAccessCache) },
            new object[] { typeof(DefaultResourceResolver) },
            new object[] { typeof(IResourceResolver) },
            new object[] { typeof(ITextService) },
            new object[] { typeof(Papercraft.Transformers.AlternateTransformer) },
            new object[] { typeof(Papercraft.Transformers.ForEachTransformer) },
            new object[] { typeof(Papercraft.Transformers.ForTransformer) },
            new object[] { typeof(Papercraft.Transformers.IfTransformer) },
            new object[] { typeof(Papercraft.Transformers.SwitchTransformer) },
            new object[] { typeof(Papercraft.Transformers.VariableTransformer) },
            new object[] { typeof(ControlName) },
            new object[] { typeof(ParameterName) },
            new object[] { typeof(XmlNode) },
            new object[] { typeof(XmlNodeInformation) },
            new object[] { typeof(XmlStyleInformation) },
            new object[] { typeof(XmlTemplateReader) },
            new object[] { typeof(DisplayCommand) },
            new object[] { typeof(DisplayList) },
            new object[] { typeof(DisplayPoint) },
            new object[] { typeof(DisplayRectangle) },
            new object[] { typeof(DisplayColor) },
            new object[] { typeof(DisplayTextStyle) },
            new object[] { typeof(DisplayFont) },
            new object[] { typeof(DisplayFontStyle) },
            new object[] { typeof(PapercraftDocument) },
            new object[] { typeof(PapercraftPage) },
            new object[] { typeof(PapercraftGenerator) },
            new object[] { typeof(PapercraftRenderer) },
            new object[] { typeof(PapercraftServiceBuilder) },
            new object[] { typeof(PapercraftServiceCollectionExtensions) },
            new object[] { typeof(PdfTemplateServiceBuilder) },
        };

    public static IEnumerable<object[]> FacadeOwnedTypes
        => new[]
        {
            new object[] { typeof(PapercraftFacadeServiceCollectionExtensions) },
        };

    public static IEnumerable<object[]> SkiaSpecificRuntimeTypes
        => new[]
        {
            new object[] { typeof(SkiaSharpRenderBackend) },
            new object[] { typeof(SkiaSharpDisplayListRenderer) },
            new object[] { typeof(SkPaintCache) },
            new object[] { typeof(SkiaSharpCanvasCompatibilityExtensions) },
        };

    [Theory]
    [MemberData(nameof(RendererNeutralTypes))]
    public void RendererNeutralTypesLiveInPapercraftCore(Type type)
    {
        Assert.Equal("X39.Solutions.Papercraft.Core", type.Assembly.GetName().Name);
    }

    [Theory]
    [MemberData(nameof(FacadeOwnedTypes))]
    public void FacadeOwnedTypesLiveInPapercraftFacade(Type type)
    {
        Assert.Equal("X39.Solutions.Papercraft", type.Assembly.GetName().Name);
    }

    [Theory]
    [MemberData(nameof(SkiaSpecificRuntimeTypes))]
    public void SkiaSpecificRuntimeTypesLiveInRendererPackage(Type type)
    {
        Assert.Equal("X39.Solutions.Papercraft.Rendering.SkiaSharp", type.Assembly.GetName().Name);
    }

    [Fact]
    public void CompatibilityBridgeForwardsSkiaSpecificRuntimeEntryPoints()
    {
        var forwardedTypes = System.Reflection.Assembly
            .Load("X39.Solutions.PdfTemplate")
            .GetForwardedTypes()
            .ToHashSet();

        Assert.Contains(typeof(SkPaintCache), forwardedTypes);
        Assert.Contains(typeof(SkiaSharpCanvasCompatibilityExtensions), forwardedTypes);
        Assert.Contains(typeof(PapercraftGenerator), forwardedTypes);
        Assert.Contains(typeof(PapercraftRenderer), forwardedTypes);
        Assert.Contains(typeof(PapercraftServiceBuilder), forwardedTypes);
        Assert.Contains(typeof(PapercraftServiceCollectionExtensions), forwardedTypes);
        Assert.Contains(typeof(PdfTemplateServiceBuilder), forwardedTypes);
    }

    [Fact]
    public void PapercraftFacadeForwardsCoreEntryPointsAndOwnsDefaultFacadeExtensions()
    {
        var forwardedTypes = System.Reflection.Assembly
            .Load("X39.Solutions.Papercraft")
            .GetForwardedTypes()
            .ToHashSet();

        Assert.Contains(typeof(IPapercraftRenderBackend), forwardedTypes);
        Assert.Contains(typeof(IPapercraftTemplateDataAccessor), forwardedTypes);
        Assert.Contains(typeof(RasterPageInfo), forwardedTypes);
        Assert.Contains(typeof(RasterPageRenderOutput), forwardedTypes);
        Assert.Contains(typeof(PapercraftRenderOptions), forwardedTypes);
        Assert.Contains(typeof(PapercraftDocument), forwardedTypes);
        Assert.Contains(typeof(PapercraftPage), forwardedTypes);
        Assert.Contains(typeof(PapercraftGenerator), forwardedTypes);
        Assert.Contains(typeof(PapercraftRenderer), forwardedTypes);
        Assert.Contains(typeof(PapercraftServiceBuilder), forwardedTypes);
        Assert.Contains(typeof(PapercraftServiceCollectionExtensions), forwardedTypes);
        Assert.DoesNotContain(typeof(PapercraftFacadeServiceCollectionExtensions), forwardedTypes);
    }

    [Fact]
    public void PapercraftCoreDoesNotReferenceCompatibilityOrSkiaSharp()
    {
        var references = typeof(RendererCapabilities)
            .Assembly
            .GetReferencedAssemblies()
            .Select((q) => q.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("X39.Solutions.PdfTemplate", references);
        Assert.DoesNotContain("SkiaSharp", references);
        Assert.DoesNotContain("X39.Util", references);
        Assert.DoesNotContain("JetBrains.Annotations", references);
    }

    [Fact]
    public void CoreContractsValidateRendererTargetsWithoutRuntimeDependencies()
    {
        var capabilities = new RendererCapabilities(
            "test",
            "Test Renderer",
            RendererOutputKind.Pdf,
            new[] { PapercraftMediaTypes.ApplicationPdf });

        var target = RenderTarget.FromMediaType(PapercraftMediaTypes.ApplicationPdf);

        Assert.True(capabilities.Supports(target));
        Assert.Same(RenderValidationResult.Supported, capabilities.ValidateTarget(target));
    }

    [Fact]
    public void RasterPageRenderOutputRequiresRasterTarget()
    {
        static ValueTask<Stream> OpenPageStream(RasterPageInfo _, CancellationToken __)
            => ValueTask.FromResult<Stream>(new MemoryStream());

        var output = new RasterPageRenderOutput(PapercraftMediaTypes.ImagePng, OpenPageStream);

        Assert.Equal(RendererOutputKind.RasterImage, output.Target.OutputKind);
        Assert.Equal(PapercraftMediaTypes.ImagePng, output.MediaType);
        Assert.False(output.LeaveStreamsOpen);
        Assert.Throws<ArgumentException>(
            () => new RasterPageRenderOutput(RenderTarget.Pdf, OpenPageStream));
    }

    [Fact]
    public void PapercraftDocumentFindsFeatureUsesFromDisplayCommands()
    {
        var document = CreateDocumentWithTextAndImage();

        var featureUses = document.FeatureUses;

        Assert.Contains(featureUses, (q) => q.Feature == RendererFeatures.TextDrawing);
        Assert.Contains(featureUses, (q) => q.Feature == RendererFeatures.TextMeasurement);
        Assert.Contains(featureUses, (q) => q.Feature == RendererFeatures.Color);
        Assert.Contains(featureUses, (q) => q.Feature == RendererFeatures.Transparency);
        Assert.Contains(featureUses, (q) => q.Feature == RendererFeatures.Fonts);
        Assert.Contains(featureUses, (q) => q.Feature == RendererFeatures.Images);
        Assert.All(featureUses, (q) => Assert.Null(q.Location));
    }

    [Fact]
    public void RendererCapabilitiesValidateDocumentFeaturesWithDiagnostics()
    {
        var document = CreateDocumentWithTextAndImage();
        var capabilities = new RendererCapabilities(
            "limited",
            "Limited",
            RendererOutputKind.Pdf,
            new[] { PapercraftMediaTypes.ApplicationPdf },
            new Dictionary<string, RendererSupportLevel>
            {
                [RendererFeatures.Images] = RendererSupportLevel.Unsupported,
                [RendererFeatures.Color] = RendererSupportLevel.Degraded,
            });

        var validation = capabilities.ValidateDocument(document);

        Assert.Equal(RendererSupportLevel.Unsupported, validation.SupportLevel);
        var imageDiagnostic = Assert.Single(validation.Diagnostics, (q) => q.Feature == RendererFeatures.Images);
        Assert.Equal(RenderDiagnosticCodes.UnsupportedFeature, imageDiagnostic.Code);
        Assert.Null(imageDiagnostic.Location);
        var colorDiagnostic = Assert.Single(validation.Diagnostics, (q) => q.Feature == RendererFeatures.Color);
        Assert.Equal(RenderDiagnosticCodes.DegradedFeature, colorDiagnostic.Code);
        Assert.Null(colorDiagnostic.Location);
    }

    [Fact]
    public void DisplayCommandsUseCoreOwnedPayloadTypes()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var textStyle = new DisplayTextStyle
        {
            Foreground = new DisplayColor(1, 2, 3, 4),
            FontFamily = new DisplayFont("Test")
            {
                LetterSpacing = 10,
                Weight = 700,
                Style = DisplayFontStyle.Italic,
            },
        };

        var list = new DisplayList();
        list.Add(new TranslateCommand(new DisplayPoint(1, 2)));
        list.Add(new ClipCommand(new DisplayRectangle(3, 4, 5, 6)));
        list.Add(new DrawLineCommand(new DisplayColor(7, 8, 9), 1, 2, 3, 4, 5));
        list.Add(new DrawTextCommand(textStyle, 96, "text", 10, 20));
        list.Add(new DrawRectangleCommand(new DisplayRectangle(1, 2, 3, 4), new DisplayColor(10, 11, 12)));
        list.Add(new DrawImageCommand(bytes, new DisplayRectangle(5, 6, 7, 8)));

        Assert.Equal(6, list.Commands.Count);
        Assert.All(list.Commands, (command) => Assert.Equal("X39.Solutions.Papercraft.Core", command.GetType().Assembly.GetName().Name));
    }

    private static PapercraftDocument CreateDocumentWithTextAndImage()
    {
        var displayList = new DisplayList();
        displayList.Add(
            new DrawTextCommand(
                new DisplayTextStyle
                {
                    Foreground = new DisplayColor(0x11, 0x22, 0x33, 0x80),
                    FontFamily = new DisplayFont("Nunito"),
                },
                96,
                "Hello",
                0,
                0));
        displayList.Add(new DrawImageCommand(new byte[] { 1, 2, 3 }, new DisplayRectangle(0, 0, 1, 1)));

        return new PapercraftDocument(
            new[]
            {
                new PapercraftPage(0, 1, 1, new Size(10, 10), 96 / 25.4F, displayList),
            },
            CultureInfo.InvariantCulture,
            DocumentOptions.Default);
    }
}
