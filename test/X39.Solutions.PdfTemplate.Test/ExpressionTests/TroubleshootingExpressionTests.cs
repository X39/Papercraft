using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Exceptions;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class TroubleshootingExpressionTests
{
    [Fact]
    public async Task MissingVariableInTextRemainsLiteral()
    {
        const string template = """
                                <template>
                                    <text>Order @MissingOrderNumber is still pending</text>
                                </template>
                                """;

        var nodeInformation = await ReadAsync(template);

        var text = Assert.Single(nodeInformation.Children);
        Assert.Equal("Order @MissingOrderNumber is still pending", text.TextContent);
    }

    [Fact]
    public async Task MissingVariableInAttributeBecomesEmptyString()
    {
        const string template = """
                                <template>
                                    <text color="@MissingColor">Color uses template data</text>
                                </template>
                                """;

        var nodeInformation = await ReadAsync(template);

        var text = Assert.Single(nodeInformation.Children);
        Assert.Equal(string.Empty, text["color"]);
    }

    [Fact]
    public async Task UnknownFunctionInTextThrowsFunctionNotFound()
    {
        const string template = """
                                <template>
                                    <text>@missingFunction()</text>
                                </template>
                                """;

        var exception = await Assert.ThrowsAsync<TransformationFunctionNotFoundException>(
            () => ReadAsync(template)
        );
        Assert.Equal("missingFunction", exception.FunctionName);
    }

    [Fact]
    public async Task UnknownFunctionInAttributeThrowsEvaluationFailed()
    {
        const string template = """
                                <template>
                                    <text color="@missingFunction()">Color uses a function</text>
                                </template>
                                """;

        var exception = await Assert.ThrowsAsync<TransformationEvaluationFailedException>(
            () => ReadAsync(template)
        );
        Assert.Equal("@missingFunction()", exception.Expression);
        Assert.IsType<FunctionNotFoundDuringEvaluationException>(exception.InnerException);
    }

    [Fact]
    public async Task FunctionTextMissingClosingBracketThrowsMissingClosingBracket()
    {
        const string template = """
                                <template>
                                    <text>@formatName(CustomerName</text>
                                </template>
                                """;

        var exception = await Assert.ThrowsAsync<TransformationFunctionMissingClosingBracketException>(
            () => ReadAsync(template)
        );
        Assert.Equal("@formatName(CustomerName", exception.FunctionText);
        Assert.Equal(1, exception.BracketsMissing);
    }

    private static async Task<XmlNodeInformation> ReadAsync(string template)
    {
        var templateReader = new XmlTemplateReader(
            default,
            CultureInfo.InvariantCulture,
            new TemplateData(),
            []
        );
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        return await templateReader.ReadAsync(xmlReader);
    }
}
