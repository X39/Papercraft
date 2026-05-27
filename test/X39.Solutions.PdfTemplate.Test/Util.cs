using System.Globalization;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test;

public static class Util
{
    public static void ApplyForEach<TKey, TValue>(this IDictionary<TKey, TValue> self, Func<TKey, TValue, TValue> action)
    {
        foreach (var key in self.Keys)
            self[key] = action(key, self[key]);
    }
    
    public static async Task<T> ToControl<T>([LanguageInjection(InjectedLanguage.XML)] this string template)
        where T : IControl
    {
        template = string.Concat(
            $"<?xml version=\"1.0\" encoding=\"utf-8\"?><template xmlns=\"{Constants.ControlsNamespace}\"><body>",
            template,
            "</body></template>"
        );
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        serviceCollection.AddPdfTemplateDefaults();
        serviceCollection.AddPdfTemplateControl<Mock.MockControl>();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        ITemplateData templateData = new TemplateData();
        var transformers = scope.ServiceProvider.GetServices<ITransformer>().ToArray();
        using var xmlTemplateReader = new XmlTemplateReader(default, CultureInfo.InvariantCulture, templateData, transformers);
        var root = await xmlTemplateReader.ReadAsync(xmlReader);
        var t = await Template.CreateAsync(
            root,
            scope.ServiceProvider.GetRequiredService<IControlFactory>(),
            CultureInfo.InvariantCulture,
            null,
            default);
        return t.BodyControls.Cast<T>().First();
    }
}
