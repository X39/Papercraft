using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class SignatureControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(100F, 100F);
    private static readonly TextStyle DefaultTextStyle = new();

    [Fact]
    public async Task XmlParametersAreApplied()
    {
        var control = await """
                            <signature
                                height="30px"
                                lineWidth="60px"
                                lineThickness="2px"
                                lineColor="red"
                                label="Signed"
                                subtext="Manager"
                                textPlacement="Above"
                                foreground="blue"
                                fontsize="10"
                                lineheight="1.5"
                                scale="2"
                                rotation="3"
                                strokethickness="4"
                                letterspacing="6"
                                weight="700"
                                style="italic"
                                fontfamily="Courier"
                                horizontalAlignment="right"
                                verticalAlignment="bottom"/>
                            """.ToControl<SignatureControl>();

        Assert.Equal(new Length(30F, ELengthUnit.Pixel), control.Height);
        Assert.Equal(new Length(60F, ELengthUnit.Pixel), control.LineWidth);
        Assert.Equal(new Length(2F, ELengthUnit.Pixel), control.LineThickness);
        Assert.Equal(Colors.Red, control.LineColor);
        Assert.Equal("Signed", control.Label);
        Assert.Equal("Manager", control.Subtext);
        Assert.Equal(ESignatureTextPlacement.Above, control.TextPlacement);
        Assert.Equal(Colors.Blue, control.Foreground);
        Assert.Equal(10F, control.FontSize);
        Assert.Equal(1.5F, control.LineHeight);
        Assert.Equal(2F, control.Scale);
        Assert.Equal(3F, control.Rotation);
        Assert.Equal(4F, control.StrokeThickness);
        Assert.Equal(new FontWidth(6), control.LetterSpacing);
        Assert.Equal(new FontWeight(700), control.Weight);
        Assert.Equal(EFontStyle.Italic, control.Style);
        Assert.Equal("Courier", control.FontFamily);
        Assert.Equal(EHorizontalAlignment.Right, control.HorizontalAlignment);
        Assert.Equal(EVerticalAlignment.Bottom, control.VerticalAlignment);
    }

    [Fact]
    public void MeasureAndArrangeUseConfiguredHeightAndWiderTextWidth()
    {
        var control = CreateSignature();
        control.Height = 20F;
        control.LineWidth = 30F;
        control.Label = "LongLabel";
        control.Subtext = "Name";

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(45F, 20F), measured);
        Assert.Equal(new Size(45F, 20F), arranged);
        Assert.Equal(new Rectangle(0F, 0F, 45F, 20F), control.Arrangement);
        Assert.Equal(new Rectangle(0F, 0F, 45F, 20F), control.ArrangementInner);
    }

    [Fact]
    public void RenderDrawsLineAndTextBelowByDefault()
    {
        var control = CreateSignature();
        control.Height = 40F;
        control.LineWidth = 50F;
        control.LineThickness = 2F;
        control.LineColor = Colors.Green;
        control.Label = "Approved";
        control.Subtext = "Person";
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        canvas.AssertState();
        canvas.AssertDrawLine((Colors.Green, 2F, 0F, 17F, 50F, 17F));
        canvas.AssertDrawText(
            (DefaultTextStyle, "Approved", 0F, 30F),
            (DefaultTextStyle, "Person", 0F, 40F));
    }

    [Fact]
    public void RenderDrawsTextAboveLineWhenConfigured()
    {
        var control = CreateSignature();
        control.Height = 40F;
        control.LineWidth = 50F;
        control.LineThickness = 2F;
        control.LineColor = Colors.Green;
        control.Label = "Approved";
        control.Subtext = "Person";
        control.TextPlacement = ESignatureTextPlacement.Above;
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        canvas.AssertState();
        canvas.AssertDrawLine((Colors.Green, 2F, 0F, 39F, 50F, 39F));
        canvas.AssertDrawText(
            (DefaultTextStyle, "Approved", 0F, 26F),
            (DefaultTextStyle, "Person", 0F, 36F));
    }

    [Fact]
    public void RightBottomAlignmentIsAppliedByBaseControl()
    {
        var control = CreateSignature();
        control.Height = 20F;
        control.LineWidth = 50F;
        control.LineThickness = 1F;
        control.Label = string.Empty;
        control.HorizontalAlignment = EHorizontalAlignment.Right;
        control.VerticalAlignment = EVerticalAlignment.Bottom;
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        canvas.AssertState();
        canvas.AssertDrawLine((Colors.Black, 1F, 50F, 99.5F, 100F, 99.5F));
    }

    private static SignatureControl CreateSignature()
        => new(new FixedTextService())
        {
            Clip = false,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };

    private static void ArrangeAndRender(SignatureControl control, DeferredCanvasMock canvas)
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
