using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
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
    }

    private static HyperlinkControl CreateControl(string text)
        => new(new FixedTextService())
        {
            Text = text,
            Clip = false,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };

    private static void ArrangeAndRender(HyperlinkControl control, DeferredCanvasMock canvas)
    {
        control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);
    }

    private static DeferredCanvasMock CreateCanvas()
        => new() {ActualPageSize = PageSize, PageSize = PageSize};

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
}
