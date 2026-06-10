using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.Papercraft.Services.TextService;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class HyperlinkControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(100F, 100F);

    [Fact]
    public async Task XmlActivatesHyperlinkAndAppliesContentAndParameters()
    {
        var control = await """
                            <hyperlink
                                href="https://example.test/invoice/123"
                                underline="false"
                                foreground="red"
                                fontsize="14">View invoice</hyperlink>
                            """.ToControl<HyperlinkControl>();

        Assert.Equal("https://example.test/invoice/123", control.Href);
        Assert.Equal("View invoice", control.Text);
        Assert.False(control.Underline);
        Assert.Equal(TextDecoration.None, control.Decoration);
        Assert.Equal(Colors.Red, control.Foreground);
        Assert.Equal(14F, control.FontSize);
    }

    [Fact]
    public async Task TextAttributeProvidesVisibleTextAndDefaultsToBlueUnderline()
    {
        var control = await """<hyperlink href="mailto:support@example.test" text="Contact support"/>"""
            .ToControl<HyperlinkControl>();

        Assert.Equal("mailto:support@example.test", control.Href);
        Assert.Equal("Contact support", control.Text);
        Assert.True(control.Underline);
        Assert.Equal(TextDecoration.Underline, control.Decoration);
        Assert.Equal(Colors.Blue, control.Foreground);
    }

    [Fact]
    public void MeasuresLikeTextControl()
    {
        const string text = "customer portal";
        var textService = new FixedTextService();
        var hyperlink = new HyperlinkControl(textService) {Text = text};
        var textControl = new TextControl(textService) {Text = text};

        var hyperlinkSize = hyperlink.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var textSize = textControl.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(textSize, hyperlinkSize);
        Assert.Equal(new Size(75F, 10F), hyperlinkSize);
    }

    [Fact]
    public void RendersVisibleTextWithUnderlineDecorationByDefault()
    {
        var control = CreateControl("Portal");
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        Assert.Equal(TextDecoration.Underline, control.GetTextStyle().Decoration);
        canvas.AssertState();
        canvas.AssertDrawText(control.GetTextStyle(), "Portal", 0F, 10F);
        Assert.Equal(0, canvas.DrawLineCount);
    }

    [Fact]
    public void EmitsLinkAnnotationForRenderedTextBounds()
    {
        const string href = "https://example.test/portal";
        var control = CreateControl("Portal", new FixedTextLayoutService(width: 30F));
        control.Href = href;
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        canvas.AssertDrawText(control.GetTextStyle(), "Portal", 0F, 8F);
        canvas.AssertLinkAnnotation(href, new Rectangle(0F, 0F, 30F, 10F));
    }

    [Fact]
    public void MultilineHyperlinkCreatesAnnotationPerRenderedLine()
    {
        const string href = "https://example.test/multiline";
        var control = CreateControl("First\nSecond", new FixedTextLayoutService(width: 40F));
        control.Href = href;
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        canvas.AssertLinkAnnotation(
            (href, new Rectangle(0F, 0F, 40F, 10F)),
            (href, new Rectangle(0F, 10F, 40F, 10F)));
    }

    [Fact]
    public void WrappedHyperlinkCreatesAnnotationPerWrappedLine()
    {
        const string href = "https://example.test/wrapped";
        var control = CreateControl("Alpha Beta", new WrappingTextLayoutService());
        control.Href = href;
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas, new Size(12F, 100F));

        canvas.AssertLinkAnnotation(
            (href, new Rectangle(0F, 0F, 12F, 10F)),
            (href, new Rectangle(0F, 10F, 12F, 10F)));
    }

    [Fact]
    public void LinkAnnotationRectanglesRespectPaginationAndTranslation()
    {
        const string href = "https://example.test/translated";
        var control = CreateControl("Portal", new FixedTextLayoutService(width: 30F));
        control.Href = href;
        var canvas = CreateCanvas();
        canvas.Translate(5F, 95F);

        ArrangeAndRender(control, canvas);

        canvas.AssertLinkAnnotation(href, new Rectangle(5F, 100F, 30F, 10F));
    }

    [Fact]
    public void DoesNotDrawUnderlineWhenDisabled()
    {
        var control = CreateControl("Portal");
        control.Underline = false;
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        Assert.Equal(TextDecoration.None, control.GetTextStyle().Decoration);
        canvas.AssertState();
        canvas.AssertDrawText(control.GetTextStyle(), "Portal", 0F, 10F);
        Assert.Equal(0, canvas.DrawLineCount);
    }

    [Fact]
    public void EmptyHrefStillRendersVisualText()
    {
        var control = CreateControl("No target");
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        Assert.Equal(string.Empty, control.Href);
        Assert.Equal(TextDecoration.Underline, control.GetTextStyle().Decoration);
        canvas.AssertState();
        canvas.AssertDrawText(control.GetTextStyle(), "No target", 0F, 10F);
        Assert.Equal(0, canvas.DrawLineCount);
        Assert.Equal(0, canvas.LinkAnnotationCount);
    }

    [Fact]
    public async Task HyperlinkEmitsLinkAnnotationDisplayCommand()
    {
        const string href = "https://example.test/display-command";
        var document = await GenerateDocumentAsync($"""<hyperlink href="{href}">Portal</hyperlink>""");

        var link = Assert.Single(document.Pages.SelectMany((page) => page.DisplayList.Commands).OfType<LinkAnnotationCommand>());

        Assert.Equal(href, link.Uri);
        Assert.True(link.Rectangle.Width > 0F);
        Assert.True(link.Rectangle.Height > 0F);
    }

    [Theory]
    [InlineData("https://example.test/invoice/123")]
    [InlineData("mailto:support@example.test")]
    public async Task PdfOutputContainsClickableUriAnnotation(string href)
    {
        var bytes = await RenderAsync(PapercraftMediaTypes.ApplicationPdf, $"""<hyperlink href="{href}">Open link</hyperlink>""");
        var pdfText = Encoding.Latin1.GetString(bytes);

        Assert.Contains("/Subtype /Link", pdfText, StringComparison.Ordinal);
        Assert.Contains("/S /URI", pdfText, StringComparison.Ordinal);
        Assert.Contains($"/URI ({href})", pdfText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PngOutputStillRendersVisualHyperlinkText()
    {
        const string body = """<hyperlink href="https://example.test/png">Open link</hyperlink>""";
        var validation = await ValidateAsync(RenderTarget.ImagePng, body);
        var bytes = await RenderAsync(PapercraftMediaTypes.ImagePng, body);

        Assert.Equal(RendererSupportLevel.Degraded, validation.SupportLevel);
        Assert.Contains(validation.Diagnostics, (q) => q.Feature == RendererFeatures.LinkAnnotations);
        Assert.True(bytes.AsSpan().StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47 }));
    }

    private static HyperlinkControl CreateControl(string text)
        => CreateControl(text, new FixedTextService());

    private static HyperlinkControl CreateControl(string text, ITextService textService)
        => new(textService)
        {
            Text = text,
            Clip = false,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };

    private static void ArrangeAndRender(HyperlinkControl control, DeferredCanvasMock canvas)
        => ArrangeAndRender(control, canvas, PageSize);

    private static void ArrangeAndRender(HyperlinkControl control, DeferredCanvasMock canvas, Size availableSize)
    {
        control.Measure(Dpi, PageSize, availableSize, availableSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, availableSize, availableSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, availableSize, CultureInfo.InvariantCulture);
    }

    private static DeferredCanvasMock CreateCanvas()
        => new() {ActualPageSize = PageSize, PageSize = PageSize};

    private static async Task<PapercraftDocument> GenerateDocumentAsync(string body)
    {
        var services = new ServiceCollection();
        services.AddPapercraft();
        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftGenerator>();
        using var reader = CreateReader(body);
        return await generator.GenerateAsync(reader, CultureInfo.InvariantCulture);
    }

    private static async Task<RenderValidationResult> ValidateAsync(RenderTarget target, string body)
    {
        var services = new ServiceCollection();
        services.AddPapercraft();
        await using var serviceProvider = services.BuildServiceProvider();
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        using var reader = CreateReader(body);
        return await renderer.ValidateAsync(reader, target, CultureInfo.InvariantCulture);
    }

    private static async Task<byte[]> RenderAsync(string mediaType, string body)
    {
        var services = new ServiceCollection();
        services.AddPapercraft();
        await using var serviceProvider = services.BuildServiceProvider();
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        using var reader = CreateReader(body);
        using var output = new MemoryStream();

        await renderer.RenderAsync(
            reader,
            new RenderOutput(mediaType, output),
            CultureInfo.InvariantCulture);

        return output.ToArray();
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

    private sealed class FixedTextService : ITextService
    {
        public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
            => text.IsEmpty
                ? Size.Zero
                : new Size(text.Length * 5F, 10F);

        public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            if (text.IsEmpty)
                return;

            canvas.DrawText(textStyle, dpi, text.ToString(), 0F, 10F);
        }
    }

    private sealed class WrappingTextLayoutService : ITextLayoutService
    {
        private const float LineHeight = 10F;
        private const float BaselineOffset = 8F;
        private const float WordWidth = 12F;

        public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            var lines = Layout(textStyle, dpi, text, maxWidth);
            if (lines.Count is 0)
                return Size.Zero;

            return new Size(
                lines.Max((q) => q.Width),
                LineHeight + (lines.Count - 1) * LineHeight);
        }

        public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            foreach (var line in Layout(textStyle, dpi, text, maxWidth))
            {
                canvas.DrawText(textStyle, dpi, line.Text, line.X, line.BaselineY);
            }
        }

        public IReadOnlyList<TextLineLayout> Layout(
            TextStyle textStyle,
            float dpi,
            ReadOnlySpan<char> text,
            float maxWidth)
        {
            if (text.IsEmpty)
                return Array.Empty<TextLineLayout>();

            var textValue = text.ToString();
            var lines = maxWidth < WordWidth * 2F && textValue.Contains(' ', StringComparison.Ordinal)
                ? textValue.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                : new[] { textValue };

            return lines
                .Select(
                    (line, index) =>
                    {
                        var top = index * LineHeight;
                        return new TextLineLayout(
                            line,
                            0F,
                            top + BaselineOffset,
                            top,
                            LineHeight,
                            WordWidth);
                    })
                .ToArray();
        }
    }
}
