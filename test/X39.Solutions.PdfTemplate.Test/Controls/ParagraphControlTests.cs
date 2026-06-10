using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Exceptions;
using X39.Solutions.Papercraft.Services.TextService;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class ParagraphControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(100F, 100F);

    [Fact]
    public async Task XmlActivatesParagraphSpanAndBreakControlsAndAppliesParameters()
    {
        var control = await """
                            <paragraph
                                foreground="red"
                                fontsize="14"
                                lineheight="1.5"
                                scale="1.2"
                                rotation="5"
                                strokethickness="2"
                                decoration="underline"
                                letterspacing="5"
                                weight="bold"
                                style="italic"
                                fontfamily="Arial">
                                <span>Total: </span>
                                <span foreground="blue" weight="bold">Value</span>
                                <br/>
                            </paragraph>
                            """.ToControl<ParagraphControl>();

        Assert.Equal(Colors.Red, control.Foreground);
        Assert.Equal(14F, control.FontSize);
        Assert.Equal(1.5F, control.LineHeight);
        Assert.Equal(1.2F, control.Scale);
        Assert.Equal(5F, control.Rotation);
        Assert.Equal(2F, control.StrokeThickness);
        Assert.Equal(TextDecoration.Underline, control.Decoration);
        Assert.Equal(FontWidths.Normal, control.LetterSpacing);
        Assert.Equal(FontWeights.Bold, control.Weight);
        Assert.Equal(EFontStyle.Italic, control.Style);
        Assert.Equal("Arial", control.FontFamily);
        Assert.Collection(
            control.Children,
            (child) =>
            {
                var span = Assert.IsType<SpanControl>(child);
                Assert.Equal("Total:", span.Text);
            },
            (child) =>
            {
                var span = Assert.IsType<SpanControl>(child);
                Assert.Equal("Value", span.Text);
                Assert.Equal(Colors.Blue, span.Foreground);
                Assert.Equal(FontWeights.Bold, span.Weight);
            },
            (child) => Assert.IsType<BrControl>(child));
    }

    [Fact]
    public void ParagraphOnlyAcceptsSpansAndBreaks()
    {
        var control = CreateParagraph();

        Assert.True(control.CanAdd(typeof(SpanControl)));
        Assert.True(control.CanAdd(typeof(BrControl)));
        Assert.False(control.CanAdd(typeof(TextControl)));
        Assert.Throws<ArgumentException>(() => control.Add(new TextControl(new FixedTextService())));
    }

    [Fact]
    public async Task TemplateCreationRejectsDirectNonInlineChildren()
    {
        var exception = await Assert.ThrowsAsync<FailedToCreateControlException>(
            async () => await "<paragraph><text>bad</text></paragraph>".ToControl<ParagraphControl>());

        Assert.True(
            HasInnerException<ContentControlDoesNotSupportTheProvidedChildException>(exception),
            $"Expected {nameof(ContentControlDoesNotSupportTheProvidedChildException)} in the exception chain.");
    }

    [Fact]
    public void MultipleSpansRenderOnSameLine()
    {
        var control = CreateParagraph();
        control.Add(new SpanControl { Text = "Hello " });
        control.Add(new SpanControl { Text = "World" });
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        var textStyle = control.GetTextStyle();
        canvas.AssertState();
        canvas.AssertDrawText(
            (textStyle, "Hello ", 0F, 10F),
            (textStyle, "World", 30F, 10F));
    }

    [Fact]
    public void BrMovesFollowingSpanToNextLine()
    {
        var control = CreateParagraph();
        control.Add(new SpanControl { Text = "First" });
        control.Add(new BrControl());
        control.Add(new SpanControl { Text = "Second" });
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        var textStyle = control.GetTextStyle();
        canvas.AssertState();
        canvas.AssertDrawText(
            (textStyle, "First", 0F, 10F),
            (textStyle, "Second", 0F, 20F));
    }

    [Fact]
    public void SpanStyleOverrideChangesDrawTextStyle()
    {
        var control = CreateParagraph();
        control.Foreground = Colors.Red;
        control.Add(new SpanControl { Text = "Default" });
        control.Add(
            new SpanControl
            {
                Text = "Bold",
                Foreground = Colors.Blue,
                Weight = FontWeights.Bold,
            });
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        var baseStyle = control.GetTextStyle();
        var overrideStyle = baseStyle with
        {
            Foreground = Colors.Blue,
            FontFamily = baseStyle.FontFamily with { Weight = FontWeights.Bold },
        };
        canvas.AssertState();
        canvas.AssertDrawText(
            (baseStyle, "Default", 0F, 10F),
            (overrideStyle, "Bold", 35F, 10F));
    }

    [Fact]
    public void SpanDecorationOverrideChangesDrawTextStyle()
    {
        var control = CreateParagraph();
        control.Decoration = TextDecoration.Underline;
        control.Add(new SpanControl { Text = "Default" });
        control.Add(
            new SpanControl
            {
                Text = "Deleted",
                Decoration = TextDecoration.StrikeThrough,
            });
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        var baseStyle = control.GetTextStyle();
        var overrideStyle = baseStyle with { Decoration = TextDecoration.StrikeThrough };
        canvas.AssertState();
        canvas.AssertDrawText(
            (baseStyle, "Default", 0F, 10F),
            (overrideStyle, "Deleted", 35F, 10F));
    }

    [Fact]
    public void WrappingProducesAdditionalLineWhenMaxWidthIsSmall()
    {
        var control = CreateParagraph();
        control.Add(new SpanControl { Text = "one two" });
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas, new Size(20F, 100F));

        var textStyle = control.GetTextStyle();
        canvas.AssertState();
        canvas.AssertDrawText(
            (textStyle, "one ", 0F, 10F),
            (textStyle, "two", 0F, 20F));
    }

    private static ParagraphControl CreateParagraph()
        => new(new FixedTextService())
        {
            Clip = false,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };

    private static void ArrangeAndRender(
        ParagraphControl control,
        DeferredCanvasMock canvas,
        Size? availableSize = null)
    {
        var remainingSize = availableSize ?? PageSize;
        control.Measure(Dpi, PageSize, remainingSize, remainingSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, remainingSize, remainingSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, remainingSize, CultureInfo.InvariantCulture);
    }

    private static DeferredCanvasMock CreateCanvas()
        => new() { ActualPageSize = PageSize, PageSize = PageSize };

    private static bool HasInnerException<TException>(Exception exception)
        where TException : Exception
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is TException)
                return true;
        }

        return false;
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
}
