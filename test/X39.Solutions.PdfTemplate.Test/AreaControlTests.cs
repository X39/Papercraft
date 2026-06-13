using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class AreaControlTests
{
    [Fact]
    public async Task TableInsideAreaUsesAreaLocalPageOrigin()
    {
        var document = await GenerateDocumentAsync(
            $$"""
              <?xml version="1.0" encoding="utf-8"?>
              <template xmlns="{{Constants.ControlsNamespace}}">
                  <areas>
                      <area left="10px" top="50px" width="100px" height="100px">
                          <table>
                              <tr>
                                  <td><mock width="100px" height="80px" /></td>
                              </tr>
                          </table>
                      </area>
                  </areas>
              </template>
              """,
            CreatePixelDocumentOptions(200, 200));

        var clips = CollectClips(document.Pages.Single());

        Assert.Contains(new Rectangle(10, 50, 100, 100), clips);
        Assert.Contains(new Rectangle(10, 50, 100, 80), clips);
        Assert.DoesNotContain(new Rectangle(10, 100, 100, 80), clips);
    }

    [Fact]
    public async Task AreasRepeatAtSamePositionOnGeneratedPages()
    {
        var document = await GenerateDocumentAsync(
            $$"""
              <?xml version="1.0" encoding="utf-8"?>
              <template xmlns="{{Constants.ControlsNamespace}}">
                  <body>
                      <mock width="1px" height="150px" />
                  </body>
                  <areas>
                      <area left="10px" top="15px" width="20px" height="20px">
                          <table>
                              <tr>
                                  <td><mock width="20px" height="10px" /></td>
                              </tr>
                          </table>
                      </area>
                  </areas>
              </template>
              """,
            CreatePixelDocumentOptions(100, 100));

        Assert.Equal(2, document.Pages.Count);
        foreach (var page in document.Pages)
        {
            var clips = CollectClips(page);
            Assert.Contains(new Rectangle(10, 15, 20, 20), clips);
            Assert.Contains(new Rectangle(10, 15, 20, 10), clips);
            Assert.DoesNotContain(new Rectangle(10, 115, 20, 20), clips);
            Assert.DoesNotContain(new Rectangle(10, 115, 20, 10), clips);
        }
    }

    private static async Task<PapercraftDocument> GenerateDocumentAsync(string template, DocumentOptions options)
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService((builder) => builder.AddControl<MockControl>());
        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftGenerator>();

        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        return await generator.GenerateAsync(xmlReader, CultureInfo.InvariantCulture, options);
    }

    private static DocumentOptions CreatePixelDocumentOptions(float width, float height)
        => new()
        {
            DotsPerMillimeter = 1F,
            PageWidthInMillimeters = width,
            PageHeightInMillimeters = height,
        };

    private static IReadOnlyCollection<Rectangle> CollectClips(PapercraftPage page)
    {
        var clips = new List<Rectangle>();
        var translations = new Stack<Point>();
        translations.Push(default);

        foreach (var command in page.DisplayList.Commands)
        {
            switch (command)
            {
                case PushStateCommand:
                    translations.Push(translations.Peek());
                    break;
                case PopStateCommand:
                    translations.Pop();
                    break;
                case TranslateCommand translate:
                {
                    var current = translations.Pop();
                    translations.Push(current + new Point(translate.Offset.X, translate.Offset.Y));
                    break;
                }
                case ClipCommand clip:
                    clips.Add(ToRectangle(clip.Rectangle) + translations.Peek());
                    break;
            }
        }

        return clips;
    }

    private static Rectangle ToRectangle(DisplayRectangle rectangle)
        => new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
}
