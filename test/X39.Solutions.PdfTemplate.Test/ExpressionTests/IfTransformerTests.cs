using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Xml;

namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class IfTransformerTests
{
    [Fact]
    public async Task IfTrue()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if true {
                                         <text>True</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] {new Papercraft.Transformers.IfTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("True", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task IfFalse()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if false {
                                         <text>True</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] {new Papercraft.Transformers.IfTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Empty(nodeInformation.Children);
    }

    [Fact]
    public async Task IfConditionUsesBooleanVariable()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if HasPurchaseOrder {
                                         <text>Purchase order is available</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var data = new TemplateData();
        data.SetVariable("HasPurchaseOrder", true);
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            data,
            new[] {new Papercraft.Transformers.IfTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("Purchase order is available", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task IfConditionUsesBooleanFunction()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if hasPurchaseOrder() {
                                         <text>Purchase order is available</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var data = new TemplateData();
        data.RegisterFunction(new DummyValueFunction("hasPurchaseOrder", true, Type.EmptyTypes));
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            data,
            new[] {new Papercraft.Transformers.IfTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("Purchase order is available", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task IfConditionWithoutOperatorRejectsNonBooleanVariable()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if PurchaseOrder {
                                         <text>Purchase order is available</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var data = new TemplateData();
        data.SetVariable("PurchaseOrder", "PO-1007");
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            data,
            new[] {new Papercraft.Transformers.IfTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        await TransformerAssert.ThrowsDirectOrWrappedAsync<ArgumentException>(() => templateReader.ReadAsync(xmlReader));
    }

    [Fact]
    public async Task IfElseUsesIfBranchWhenTrue()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if true {
                                         <text>If</text>
                                     }
                                     @else if throwValue() {
                                         <text>Else If</text>
                                     }
                                     @else {
                                         <text>Else</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] { new Papercraft.Transformers.IfTransformer() });
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("If", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task IfElseIfUsesFirstTrueElseIfBranch()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if false {
                                         <text>If</text>
                                     }
                                     @else if false {
                                         <text>Else If 1</text>
                                     }
                                     @else if true {
                                         <text>Else If 2</text>
                                     }
                                     @else {
                                         <text>Else</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] { new Papercraft.Transformers.IfTransformer() });
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("Else If 2", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task IfElseUsesElseBranchWhenAllConditionsAreFalse()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if false {
                                         <text>If</text>
                                     }
                                     @else if false {
                                         <text>Else If</text>
                                     }
                                     @else {
                                         <text>Else</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] { new Papercraft.Transformers.IfTransformer() });
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("Else", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task IfElseIfWithoutMatchingBranchEmitsNothing()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if false {
                                         <text>If</text>
                                     }
                                     @else if false {
                                         <text>Else If</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] { new Papercraft.Transformers.IfTransformer() });
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Empty(nodeInformation.Children);
    }

    [Fact]
    public async Task IfElseIfStopsAtFirstTrueBranch()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if false {
                                         <text>If</text>
                                     }
                                     @else if true {
                                         <text>Else If 1</text>
                                     }
                                     @else if throwValue() {
                                         <text>Else If 2</text>
                                     }
                                     @else {
                                         <text>Else</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] { new Papercraft.Transformers.IfTransformer() });
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("Else If 1", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task NestedIfElseChainsAreParsed()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if true {
                                         @if false {
                                             <text>Nested If</text>
                                         }
                                         @else {
                                             <text>Nested Else</text>
                                         }
                                     }
                                     @else {
                                         <text>Outer Else</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] { new Papercraft.Transformers.IfTransformer() });
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Single(nodeInformation.Children);
        Assert.Equal("Nested Else", nodeInformation.Children.ElementAt(0).TextContent);
    }

    [Fact]
    public async Task ElseIfAfterElseThrows()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if false {
                                         <text>If</text>
                                     }
                                     @else {
                                         <text>Else</text>
                                     }
                                     @else if true {
                                         <text>Else If</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] { new Papercraft.Transformers.IfTransformer() });
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        await TransformerAssert.ThrowsDirectOrWrappedAsync<ArgumentException>(() => templateReader.ReadAsync(xmlReader));
    }

    [Fact]
    public async Task DuplicateElseThrows()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @if false {
                                         <text>If</text>
                                     }
                                     @else {
                                         <text>Else 1</text>
                                     }
                                     @else {
                                         <text>Else 2</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var templateReader = new XmlTemplateReader(
            default, CultureInfo.InvariantCulture,
            new TemplateData(),
            new[] { new Papercraft.Transformers.IfTransformer() });
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        await TransformerAssert.ThrowsDirectOrWrappedAsync<ArgumentException>(() => templateReader.ReadAsync(xmlReader));
    }

    [Theory]
    [InlineData(2, "&gt;", 1, true)]
    [InlineData(1, "&gt;", 2, false)]
    [InlineData(1, "&gt;=", 1, true)]
    [InlineData(1, "&gt;=", 2, false)]
    [InlineData(2, "&gt;=", 1, true)]
    [InlineData(1, "&lt;", 2, true)]
    [InlineData(2, "&lt;", 1, false)]
    [InlineData(1, "&lt;=", 1, true)]
    [InlineData(2, "&lt;=", 1, false)]
    [InlineData(1, "&lt;=", 2, true)]
    [InlineData(1, "==", 1, true)]
    [InlineData(1, "==", 2, false)]
    [InlineData(1, "!=", 1, false)]
    [InlineData(1, "!=", 2, true)]
    [InlineData("abc", "==", "abc", true)]
    [InlineData("ABC", "==", "abc", true)]
    [InlineData("abc", "==", "ABC", true)]
    [InlineData("abc", "===", "abc", true)]
    [InlineData("ABC", "===", "abc", false)]
    [InlineData("abc", "===", "ABC", false)]
    [InlineData("abc", "!=", "abc", false)]
    [InlineData("ABC", "!=", "abc", false)]
    [InlineData("abc", "!=", "ABC", false)]
    [InlineData("abc", "!==", "abc", false)]
    [InlineData("ABC", "!==", "abc", true)]
    [InlineData("abc", "!==", "ABC", true)]
    [InlineData("a", "in", "abc", true)]
    [InlineData("b", "in", "abc", true)]
    [InlineData("c", "in", "abc", true)]
    [InlineData("d", "in", "abc", false)]
    public async Task IfTheory(object left, string op, object right, bool exists)
    {
        const string ns = Constants.ControlsNamespace;
        var template = $$"""
                         <?xml version="1.0" encoding="utf-8"?>
                         <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                            @if variable {{op}} function(1, "") {
                                <text>True</text>
                            }
                         </styleMustBeEmptyTagTest>
                         """;
        var data = new TemplateData();
        data.SetVariable("variable", left);
        data.RegisterFunction(new DummyValueFunction("function", right, new[] {typeof(int), typeof(string)}));
        var templateReader = new XmlTemplateReader(default, CultureInfo.InvariantCulture, data, new[] {new Papercraft.Transformers.IfTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);

        if (exists)
        {
            Assert.Single(nodeInformation.Children);
            Assert.Equal("True", nodeInformation.Children.ElementAt(0).TextContent);
        }
        else
        {
            Assert.Empty(nodeInformation.Children);
        }
    }
}
