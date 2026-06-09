using System.Globalization;
using System.Text;
using System.Xml;
using SkiaSharp;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;
using X39.Util.Collections;

namespace X39.Solutions.PdfTemplate.Test.Samples;

[Collection("Samples")]
public class ReadMeSample : SampleBase
{
    [Fact]
    public async Task SimpleTable()
    {
        var       docOptions = new DocumentOptions
        {
            Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)),
        };
        using var generator  = CreateGenerator();
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $$"""
                  <?xml version="1.0" encoding="utf-8"?>
                  <template xmlns="{{Constants.ControlsNamespace}}">
                      <template.style>
                          <text padding="0.125cm" />
                      </template.style>
                      <header>
                        <text fontsize="20">Invoice #1234</text>
                        <text>Issue date 12.12.2024</text>
                        <text>Due date 12.12.2024</text>
                      </header>
                      <body>
                         <table margin="0 1cm">
                            <tr>
                                <td width="45%">
                                    <text>From:</text>
                                    <line thickness="1pt" length="100%"/>
                                    <text>John Doe</text>
                                    <text>1234 Main Street</text>
                                    <text>Springfield, IL 62701</text>
                                    <text>United States</text>
                                </td>
                                <td width="10%"/>
                                <td width="45%">
                                    <text>To:</text>
                                    <line thickness="1pt" length="100%"/>
                                    <text>Jane Doe</text>
                                    <text>1234 Main Street</text>
                                    <text>Springfield, IL 62701</text>
                                    <text>United States</text>
                                </td>
                            </tr>
                         </table>
                         
                         <table>
                            <th borderThickness="0 0 0 1pt" borderColor="black">
                                <td>
                                    <text>#</text>
                                </td>
                                <td>
                                    <text>Product</text>
                                </td>
                                <td>
                                    <text horizontalAlignment="right">Price</text>
                                </td>
                                <td>
                                    <text horizontalAlignment="right">Quantity</text>
                                </td>
                                <td>
                                    <text horizontalAlignment="right">Total</text>
                                </td>
                            </th>
                            @alternate on value with ["#f0f0f0", "#ffffff"] {
                            <tr background="@value">
                                <td><text>1</text></td>
                                <td><text>Fancy shirt</text></td>
                                <td><text horizontalAlignment="right">100.00 $</text></td>
                                <td><text horizontalAlignment="right">1</text></td>
                                <td><text horizontalAlignment="right">100.00 $</text></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr background="@value">
                                <td><text>2</text></td>
                                <td><text>Shoes</text></td>
                                <td><text horizontalAlignment="right">50.00 $</text></td>
                                <td><text horizontalAlignment="right">2</text></td>
                                <td><text horizontalAlignment="right">100.00 $</text></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr background="@value">
                                <td><text>3</text></td>
                                <td><text>Jeans</text></td>
                                <td><text horizontalAlignment="right">74.99 $</text></td>
                                <td><text horizontalAlignment="right">1</text></td>
                                <td><text horizontalAlignment="right">74.99 $</text></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr background="@value">
                                <td><text>4</text></td>
                                <td><text>Hat</text></td>
                                <td><text horizontalAlignment="right">24.99 $</text></td>
                                <td><text horizontalAlignment="right">1</text></td>
                                <td><text horizontalAlignment="right">24.99 $</text></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr background="@value">
                                <td><text>5</text></td>
                                <td><text>Watch</text></td>
                                <td><text horizontalAlignment="right">200.00 $</text></td>
                                <td><text horizontalAlignment="right">1</text></td>
                                <td><text horizontalAlignment="right">200.00 $</text></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr background="@value">
                                <td><text>6</text></td>
                                <td><text>Exquisite Fountain Pen</text></td>
                                <td><text horizontalAlignment="right">300.00 $</text></td>
                                <td><text horizontalAlignment="right">1</text></td>
                                <td><text horizontalAlignment="right">300.00 $</text></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr background="@value">
                                <td><text>7</text></td>
                                <td><text>Leather-bound Notebook</text></td>
                                <td><text horizontalAlignment="right">70.00 $</text></td>
                                <td><text horizontalAlignment="right">3</text></td>
                                <td><text horizontalAlignment="right">210.00 $</text></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr background="@value">
                                <td><text>8</text></td>
                                <td><text>Vintage Vinyl Records</text></td>
                                <td><text horizontalAlignment="right">45.00 $</text></td>
                                <td><text horizontalAlignment="right">5</text></td>
                                <td><text horizontalAlignment="right">225.00 $</text></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr background="@value">
                                <td><text>9</text></td>
                                <td><text>Retro Game Console</text></td>
                                <td><text horizontalAlignment="right">120.00 $</text></td>
                                <td><text horizontalAlignment="right">1</text></td>
                                <td><text horizontalAlignment="right">120.00 $</text></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr background="@value">
                                <td><text>10</text></td>
                                <td><text>Handcrafted Chess Set</text></td>
                                <td><text horizontalAlignment="right">150.00 $</text></td>
                                <td><text horizontalAlignment="right">1</text></td>
                                <td><text horizontalAlignment="right">150.00 $</text></td>
                            </tr>
                            }
                        </table>
                      </body>
                  </template>
                  """
            )
        );
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader  = XmlReader.Create(xmlStream);
        // var       bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture, docOptions);
        // using var fstream = new FileStream("output.png", FileMode.Create);
        // {
        //     bitmaps.First().Encode(fstream, SKEncodedImageFormat.Png, 100);
        // }
        // await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
        var results = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture, docOptions);
        foreach (var (skBitmap, index) in results.Indexed())
            using (skBitmap)
            {
                using var image = SKImage.FromBitmap(skBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                await using var fstream = new FileStream(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{index}.png"),
                    FileMode.Create,
                    FileAccess.Write
                );
                data.SaveTo(fstream);
            }
    }
}
