using System.Globalization;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.PdfTemplate.Test.Parsing;

public class ThicknessParsingTests
{
    [Fact]
    public void ParseSinglePixelValue()
    {
        var thickness = Thickness.Parse("1px", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Left);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Top);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Right);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Bottom);
    }
    
    [Fact]
    public void ParseSinglePercentValue()
    {
        var thickness = Thickness.Parse("1%", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Left);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Top);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Right);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Bottom);
    }
    
    [Fact]
    public void ParseTwoPixelValues()
    {
        var thickness = Thickness.Parse("1px 2px", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Left);
        Assert.Equal(new Length(2, ELengthUnit.Pixel), thickness.Top);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Right);
        Assert.Equal(new Length(2, ELengthUnit.Pixel), thickness.Bottom);
    }
    
    [Fact]
    public void ParseTwoPercentValues()
    {
        var thickness = Thickness.Parse("1% 2%", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Left);
        Assert.Equal(new Length(0.02F, ELengthUnit.Percent), thickness.Top);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Right);
        Assert.Equal(new Length(0.02F, ELengthUnit.Percent), thickness.Bottom);
    }
    
    [Fact]
    public void ParseFourPixelValues()
    {
        var thickness = Thickness.Parse("1px 2px 3px 4px", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Left);
        Assert.Equal(new Length(2, ELengthUnit.Pixel), thickness.Top);
        Assert.Equal(new Length(3, ELengthUnit.Pixel), thickness.Right);
        Assert.Equal(new Length(4, ELengthUnit.Pixel), thickness.Bottom);
    }
    
    [Fact]
    public void ParseFourPercentValues()
    {
        var thickness = Thickness.Parse("1% 2% 3% 4%", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Left);
        Assert.Equal(new Length(0.02F, ELengthUnit.Percent), thickness.Top);
        Assert.Equal(new Length(0.03F, ELengthUnit.Percent), thickness.Right);
        Assert.Equal(new Length(0.04F, ELengthUnit.Percent), thickness.Bottom);
    }

    [Fact]
    public void TryParseSinglePixelValue()
    {
        AssertTryParse(
            "1px",
            new Length(1, ELengthUnit.Pixel),
            new Length(1, ELengthUnit.Pixel),
            new Length(1, ELengthUnit.Pixel),
            new Length(1, ELengthUnit.Pixel));
    }

    [Fact]
    public void TryParseSinglePercentValue()
    {
        AssertTryParse(
            "1%",
            new Length(0.01F, ELengthUnit.Percent),
            new Length(0.01F, ELengthUnit.Percent),
            new Length(0.01F, ELengthUnit.Percent),
            new Length(0.01F, ELengthUnit.Percent));
    }

    [Fact]
    public void TryParseTwoPixelValues()
    {
        AssertTryParse(
            "1px 2px",
            new Length(1, ELengthUnit.Pixel),
            new Length(2, ELengthUnit.Pixel),
            new Length(1, ELengthUnit.Pixel),
            new Length(2, ELengthUnit.Pixel));
    }

    [Fact]
    public void TryParseTwoPercentValues()
    {
        AssertTryParse(
            "1% 2%",
            new Length(0.01F, ELengthUnit.Percent),
            new Length(0.02F, ELengthUnit.Percent),
            new Length(0.01F, ELengthUnit.Percent),
            new Length(0.02F, ELengthUnit.Percent));
    }

    [Fact]
    public void TryParseFourPixelValues()
    {
        AssertTryParse(
            "1px 2px 3px 4px",
            new Length(1, ELengthUnit.Pixel),
            new Length(2, ELengthUnit.Pixel),
            new Length(3, ELengthUnit.Pixel),
            new Length(4, ELengthUnit.Pixel));
    }

    [Fact]
    public void TryParseFourPercentValues()
    {
        AssertTryParse(
            "1% 2% 3% 4%",
            new Length(0.01F, ELengthUnit.Percent),
            new Length(0.02F, ELengthUnit.Percent),
            new Length(0.03F, ELengthUnit.Percent),
            new Length(0.04F, ELengthUnit.Percent));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("1px invalid")]
    [InlineData("1px 2px 3px")]
    [InlineData("1px 2px invalid 4px")]
    [InlineData("1px 2px 3px invalid")]
    [InlineData("1px 2px 3px 4px 5px")]
    public void TryParseInvalidValuesReturnsFalse(string input)
    {
        var parsed = true;
        Thickness thickness = new(new Length(1, ELengthUnit.Pixel));

        var exception = Record.Exception(
            () => parsed = Thickness.TryParse(input, CultureInfo.InvariantCulture, out thickness));

        Assert.Null(exception);
        Assert.False(parsed);
        Assert.Equal(default, thickness);
    }

    private static void AssertTryParse(
        string input,
        Length left,
        Length top,
        Length right,
        Length bottom)
    {
        Assert.True(Thickness.TryParse(input, CultureInfo.InvariantCulture, out var thickness));
        Assert.Equal(left, thickness.Left);
        Assert.Equal(top, thickness.Top);
        Assert.Equal(right, thickness.Right);
        Assert.Equal(bottom, thickness.Bottom);
    }
}
