using System.Text;
using System.Xml;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal static class BenchmarkTemplates
{
    private static readonly XmlReaderSettings XmlReaderSettings = new()
    {
        CloseInput = true,
    };

    public static byte[] SmallParseTemplate { get; } = ToUtf8(
        $$"""
          <?xml version="1.0" encoding="utf-8"?>
          <template xmlns="{{Constants.ControlsNamespace}}">
              <body>
                  <text>Hello benchmark</text>
                  <line thickness="1px" length="100%" color="black" />
              </body>
          </template>
          """);

    public static byte[] MediumParseTemplate { get; } = CreateRepeatedTextTemplate(80);

    public static byte[] LargeParseTemplate { get; } = CreateRepeatedTextTemplate(320);

    public static byte[] SimpleTemplateCreationTemplate { get; } = ToUtf8(
        $$"""
          <?xml version="1.0" encoding="utf-8"?>
          <template xmlns="{{BenchmarkControlNames.Namespace}}">
              <body>
                  <noop />
                  <parameter title="Benchmark" count="42" />
              </body>
          </template>
          """);

    public static byte[] NestedTemplateCreationTemplate { get; } = ToUtf8(
        $$"""
          <?xml version="1.0" encoding="utf-8"?>
          <template xmlns="{{BenchmarkControlNames.Namespace}}">
              <body>
                  <container>
                      <parameter title="Nested" enabled="true" count="7" ratio="2.5" orientation="Vertical" width="10px" padding="1px" color="#224466" />
                      <content>Nested content value</content>
                      <container>
                          <noop />
                          <service />
                      </container>
                  </container>
              </body>
          </template>
          """);

    public static byte[] MediumTemplateCreationTemplate { get; } = CreateRepeatedBenchmarkTemplate(120);

    public static byte[] RepresentativeGenerationTemplate { get; } = CreateRepresentativeInvoiceTemplate(28);

    public static XmlReader CreateXmlReader(byte[] bytes)
    {
        var stream = new MemoryStream(bytes, writable: false);
        return XmlReader.Create(stream, XmlReaderSettings);
    }

    private static byte[] CreateRepeatedTextTemplate(int count)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        builder.AppendLine($"""<template xmlns="{Constants.ControlsNamespace}">""");
        builder.AppendLine("  <template.style>");
        builder.AppendLine("""    <text padding="0.5px" />""");
        builder.AppendLine("  </template.style>");
        builder.AppendLine("  <body>");
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine($"""    <text fontsize="{10 + i % 5}" horizontalAlignment="left">Line {i}</text>""");
            if (i % 8 == 0)
                builder.AppendLine("""    <line thickness="1px" length="100%" color="#336699" />""");
        }

        builder.AppendLine("  </body>");
        builder.AppendLine("</template>");
        return ToUtf8(builder.ToString());
    }

    private static byte[] CreateRepeatedBenchmarkTemplate(int count)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        builder.AppendLine($"""<template xmlns="{BenchmarkControlNames.Namespace}">""");
        builder.AppendLine("  <body>");
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine(
                $"""    <parameter title="Item {i}" enabled="{(i % 2 == 0).ToString().ToLowerInvariant()}" count="{i}" ratio="1.25" orientation="Horizontal" width="{10 + i % 4}px" padding="1px 2px 1px 2px" color="#336699" />""");
        }

        builder.AppendLine("  </body>");
        builder.AppendLine("</template>");
        return ToUtf8(builder.ToString());
    }

    private static byte[] CreateRepresentativeInvoiceTemplate(int rowCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        builder.AppendLine($"""<template xmlns="{Constants.ControlsNamespace}">""");
        builder.AppendLine("  <template.style>");
        builder.AppendLine("""    <text padding="0.08cm" />""");
        builder.AppendLine("  </template.style>");
        builder.AppendLine("  <header>");
        builder.AppendLine("""    <text fontsize="18">Benchmark invoice #2026-05</text>""");
        builder.AppendLine("""    <text>Generated for deterministic benchmark data</text>""");
        builder.AppendLine("  </header>");
        builder.AppendLine("  <body>");
        builder.AppendLine("""    <table margin="0 0.25cm">""");
        builder.AppendLine("      <th>");
        builder.AppendLine("""        <td width="12%"><border color="black" thickness="0 0 0 1px"><text>#</text></border></td>""");
        builder.AppendLine("""        <td width="48%"><border color="black" thickness="0 0 0 1px"><text>Product</text></border></td>""");
        builder.AppendLine("""        <td width="20%"><border color="black" thickness="0 0 0 1px"><text horizontalAlignment="right">Quantity</text></border></td>""");
        builder.AppendLine("""        <td width="20%"><border color="black" thickness="0 0 0 1px"><text horizontalAlignment="right">Total</text></border></td>""");
        builder.AppendLine("      </th>");
        for (var i = 1; i <= rowCount; i++)
        {
            var background = i % 2 == 0 ? "#f0f0f0" : "#ffffff";
            builder.AppendLine("      <tr>");
            builder.AppendLine($"""        <td><border background="{background}"><text>{i}</text></border></td>""");
            builder.AppendLine($"""        <td><border background="{background}"><text>Benchmark item {i}</text></border></td>""");
            builder.AppendLine($"""        <td><border background="{background}"><text horizontalAlignment="right">{1 + i % 4}</text></border></td>""");
            builder.AppendLine($"""        <td><border background="{background}"><text horizontalAlignment="right">{25 + i * 3}.00 EUR</text></border></td>""");
            builder.AppendLine("      </tr>");
        }

        builder.AppendLine("    </table>");
        builder.AppendLine("  </body>");
        builder.AppendLine("  <footer>");
        builder.AppendLine("""    <text horizontalAlignment="right">Page footer</text>""");
        builder.AppendLine("  </footer>");
        builder.AppendLine("</template>");
        return ToUtf8(builder.ToString());
    }

    private static byte[] ToUtf8(string value) => Encoding.UTF8.GetBytes(value);
}
