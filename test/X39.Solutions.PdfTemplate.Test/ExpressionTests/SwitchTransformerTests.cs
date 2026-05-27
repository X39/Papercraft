using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class SwitchTransformerTests
{
    [Fact]
    public async Task SwitchMatchesStringCase()
    {
        var data = new TemplateData();
        data.SetVariable("status", "PAID");
        var nodeInformation = await ReadAsync(
            """
            @switch status {
                @case "paid" {
                    <text>Paid</text>
                }
                @default {
                    <text>Unknown</text>
                }
            }
            """,
            data);

        Assert.Single(nodeInformation.Children);
        Assert.Equal("Paid", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task SwitchMatchesNumericCase()
    {
        var data = new TemplateData();
        data.SetVariable("value", 2);
        var nodeInformation = await ReadAsync(
            """
            @switch value {
                @case 1 {
                    <text>One</text>
                }
                @case 2 {
                    <text>Two</text>
                }
            }
            """,
            data);

        Assert.Single(nodeInformation.Children);
        Assert.Equal("Two", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Theory]
    [InlineData(5, "> 3", "Greater")]
    [InlineData(3, ">= 3", "GreaterOrEqual")]
    [InlineData(2, "&lt; 3", "Less")]
    [InlineData(3, "&lt;= 3", "LessOrEqual")]
    [InlineData("ABC", "=== \"abc\"", "Default")]
    [InlineData("ABC", "!== \"abc\"", "StrictNotEqual")]
    [InlineData("b", "in \"abc\"", "Contained")]
    public async Task SwitchMatchesOperatorCases(object value, string caseExpression, string expected)
    {
        var data = new TemplateData();
        data.SetVariable("value", value);
        var nodeInformation = await ReadAsync(
            $$"""
              @switch value {
                  @case {{caseExpression}} {
                      <text>{{expected}}</text>
                  }
                  @default {
                      <text>Default</text>
                  }
              }
              """,
            data);

        Assert.Single(nodeInformation.Children);
        Assert.Equal(expected, nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task SwitchMatchesInEnumerableCase()
    {
        var data = new TemplateData();
        data.SetVariable("status", "paid");
        data.SetVariable("allowedStatuses", new[] { "draft", "paid" });
        var nodeInformation = await ReadAsync(
            """
            @switch status {
                @case in allowedStatuses {
                    <text>Allowed</text>
                }
            }
            """,
            data);

        Assert.Single(nodeInformation.Children);
        Assert.Equal("Allowed", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task SwitchUsesFirstMatchingCase()
    {
        var data = new TemplateData();
        data.SetVariable("value", 1);
        var nodeInformation = await ReadAsync(
            """
            @switch value {
                @case 1 {
                    <text>First</text>
                }
                @case 1 {
                    <text>Second</text>
                }
            }
            """,
            data);

        Assert.Single(nodeInformation.Children);
        Assert.Equal("First", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task SwitchUsesDefaultWhenNoCaseMatches()
    {
        var data = new TemplateData();
        data.SetVariable("value", 3);
        var nodeInformation = await ReadAsync(
            """
            @switch value {
                @case 1 {
                    <text>One</text>
                }
                @default {
                    <text>Default</text>
                }
            }
            """,
            data);

        Assert.Single(nodeInformation.Children);
        Assert.Equal("Default", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task SwitchWithoutMatchOrDefaultEmitsNothing()
    {
        var data = new TemplateData();
        data.SetVariable("value", 3);
        var nodeInformation = await ReadAsync(
            """
            @switch value {
                @case 1 {
                    <text>One</text>
                }
            }
            """,
            data);

        Assert.Empty(nodeInformation.Children);
    }

    [Fact]
    public async Task SwitchSelectedCaseRunsNestedTransformers()
    {
        var data = new TemplateData();
        data.SetVariable("value", 1);
        var nodeInformation = await ReadAsync(
            """
            @switch value {
                @case 1 {
                    @if false {
                        <text>If</text>
                    }
                    @else {
                        <text>Else</text>
                    }
                }
            }
            """,
            data);

        Assert.Single(nodeInformation.Children);
        Assert.Equal("Else", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task SwitchDoesNotEvaluateUnselectedCases()
    {
        var data = new TemplateData();
        data.SetVariable("value", 1);
        data.RegisterFunction(new DummyValueFunction("throwValue", (_) => throw new InvalidOperationException(), Type.EmptyTypes));
        var nodeInformation = await ReadAsync(
            """
            @switch value {
                @case 1 {
                    <text>One</text>
                }
                @case throwValue() {
                    <text>Throw</text>
                }
            }
            """,
            data);

        Assert.Single(nodeInformation.Children);
        Assert.Equal("One", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task SwitchThrowsForDirectContent()
    {
        var data = new TemplateData();
        data.SetVariable("value", 1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => ReadAsync(
                """
                @switch value {
                    <text>Invalid</text>
                    @case 1 {
                        <text>One</text>
                    }
                }
                """,
                data));
    }

    [Fact]
    public async Task SwitchThrowsForDuplicateDefault()
    {
        var data = new TemplateData();
        data.SetVariable("value", 1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => ReadAsync(
                """
                @switch value {
                    @default {
                        <text>Default 1</text>
                    }
                    @default {
                        <text>Default 2</text>
                    }
                }
                """,
                data));
    }

    [Fact]
    public async Task SwitchThrowsForCaseAfterDefault()
    {
        var data = new TemplateData();
        data.SetVariable("value", 1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => ReadAsync(
                """
                @switch value {
                    @default {
                        <text>Default</text>
                    }
                    @case 1 {
                        <text>One</text>
                    }
                }
                """,
                data));
    }

    [Fact]
    public async Task SwitchThrowsForEmptyCase()
    {
        var data = new TemplateData();
        data.SetVariable("value", 1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => ReadAsync(
                """
                @switch value {
                    @case {
                        <text>One</text>
                    }
                }
                """,
                data));
    }

    [Fact]
    public async Task SwitchThrowsForMissingCaseClosingBrace()
    {
        var data = new TemplateData();
        data.SetVariable("value", 1);

        await Assert.ThrowsAnyAsync<Exception>(
            () => ReadAsync(
                """
                @switch value {
                    @case 1 {
                        <text>One</text>
                }
                """,
                data));
    }

    private static async Task<XmlNodeInformation> ReadAsync(string body, TemplateData templateData)
    {
        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            {{body}}
                         </styleMustBeEmptyTagTest>
                         """;
        var templateReader = new XmlTemplateReader(
            default,
            CultureInfo.InvariantCulture,
            templateData,
            new ITransformer[] { new SwitchTransformer(), new IfTransformer() });
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        return await templateReader.ReadAsync(xmlReader);
    }
}
