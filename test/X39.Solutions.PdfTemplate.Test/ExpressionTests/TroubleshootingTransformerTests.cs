using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class TroubleshootingTransformerTests
{
    [Fact]
    public async Task ForLoopStepMustMoveTowardEnd()
    {
        const string template = """
                                <template>
                                    @for Step from 0 to 3 step -1 {
                                        <text>@Step</text>
                                    }
                                </template>
                                """;

        await Assert.ThrowsAsync<ArgumentException>(
            () => ReadAsync(template, new TemplateData(), [new ForTransformer()])
        );
    }

    [Fact]
    public async Task ForEachSourceMustBeCollection()
    {
        const string template = """
                                <template>
                                    @foreach Line in LineCount {
                                        <text>@Line</text>
                                    }
                                </template>
                                """;
        var templateData = new TemplateData();
        templateData.SetVariable("LineCount", 3);

        await Assert.ThrowsAsync<ArgumentException>(
            () => ReadAsync(template, templateData, [new ForEachTransformer()])
        );
    }

    private static async Task<XmlNodeInformation> ReadAsync(
        string template,
        TemplateData templateData,
        IReadOnlyCollection<ITransformer> transformers)
    {
        var templateReader = new XmlTemplateReader(
            default,
            CultureInfo.InvariantCulture,
            templateData,
            transformers
        );
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        return await templateReader.ReadAsync(xmlReader);
    }
}
