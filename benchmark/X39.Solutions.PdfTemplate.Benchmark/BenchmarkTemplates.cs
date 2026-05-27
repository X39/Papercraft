using System.Text;
using System.Xml;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal static class BenchmarkTemplates
{
    public static readonly string[] ControlGenerationCases =
    [
        "Text",
        "Line",
        "Border",
        "Image",
        "PageNumber",
        "Table",
        "LineChart",
        "BarChart",
        "PieChart",
    ];

    public static readonly string[] TransformerCases =
    [
        "For",
        "ForEach",
        "If",
        "Alternate",
        "Var",
    ];

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

    public static byte[] MediumParseTemplate { get; } = CreateRepeatedTextTemplate((int) BenchmarkTemplateSize.Medium);

    public static byte[] LargeParseTemplate { get; } = CreateRepeatedTextTemplate((int) BenchmarkTemplateSize.Large);

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

    public static byte[] TransformerHeavyGenerationTemplate { get; } =
        CreateTransformerHeavyGenerationTemplate(expanded: false, rowCount: 24);

    public static byte[] TransformerHeavyExpandedGenerationTemplate { get; } =
        CreateTransformerHeavyGenerationTemplate(expanded: true, rowCount: 24);

    public static XmlReader CreateXmlReader(byte[] bytes)
    {
        var stream = new MemoryStream(bytes, writable: false);
        return XmlReader.Create(stream, XmlReaderSettings);
    }

    public static byte[] GetBuiltInControlTemplate(string controlName)
        => controlName switch
        {
            "barChart"   => WrapBody(CreateBarChartXml()),
            "border"     => WrapBody("""<border thickness="1px" color="#202020" background="#F5F7FA"><text>Border benchmark</text></border>"""),
            "chart"      => WrapBody($"""<chart>{CreateLineChartXml()}</chart>"""),
            "data"       => WrapBody($"""<chart>{CreateLineChartXml(dataPointCount: 1)}</chart>"""),
            "image"      => WrapBody($"""<image source="{BenchmarkServices.TinyPngDataUri}" width="16px" height="16px" />"""),
            "lineChart"  => WrapBody($"""<chart>{CreateLineChartXml()}</chart>"""),
            "line"       => WrapBody("""<line thickness="1px" length="100%" orientation="Horizontal" color="#336699" />"""),
            "pageNumber" => WrapBody("""<pageNumber mode="CurrentTotal" prefix="Page " delimiter=" of " />"""),
            "pieChart"   => WrapBody($"""<chart>{CreatePieChartXml()}</chart>"""),
            "td"         => WrapBody("""<table><tr><td width="100%"><text>Cell benchmark</text></td></tr></table>"""),
            "table"      => WrapBody(CreateTableXml(3)),
            "th"         => WrapBody(CreateTableXml(2)),
            "tr"         => WrapBody(CreateTableXml(2)),
            "text"       => WrapBody("""<text fontSize="11" foreground="#202020">Text benchmark</text>"""),
            _            => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null),
        };

    public static byte[] GetControlGenerationTemplate(string caseName)
        => caseName switch
        {
            "Text"       => WrapBody(CreateRepeatedTextControls(12)),
            "Line"       => WrapBody(CreateRepeatedLineControls(12)),
            "Border"     => WrapBody(CreateRepeatedBorderControls(8)),
            "Image"      => WrapBody(CreateRepeatedImageControls(6)),
            "PageNumber" => WrapTemplate("""<body><text>Body</text></body><footer><pageNumber mode="CurrentTotal" prefix="Page " delimiter=" of " /></footer>"""),
            "Table"      => WrapBody(CreateTableXml(12)),
            "LineChart"  => WrapBody($"""<chart>{CreateLineChartXml(dataPointCount: 12)}</chart>"""),
            "BarChart"   => WrapBody($"""<chart>{CreateBarChartXml(dataPointCount: 12)}</chart>"""),
            "PieChart"   => WrapBody($"""<chart>{CreatePieChartXml(dataPointCount: 8)}</chart>"""),
            _            => throw new ArgumentOutOfRangeException(nameof(caseName), caseName, null),
        };

    public static byte[] GetTransformerTemplate(
        string transformerName,
        BenchmarkTemplateSize size,
        bool expanded)
    {
        var count = (int) size;
        var body = transformerName switch
        {
            "For"       => expanded ? CreateExpandedTextRows(count, "For") : CreateForTransformerBody(count),
            "ForEach"   => expanded ? CreateExpandedTextRows(count, "ForEach") : CreateForEachTransformerBody(),
            "If"        => expanded ? CreateExpandedTextRows(count, "If") : CreateIfTransformerBody(count),
            "Alternate" => expanded ? CreateExpandedAlternateRows(count) : CreateAlternateTransformerBody(count),
            "Var"       => expanded ? CreateExpandedTextRows(count, "Var") : CreateVarTransformerBody(count),
            _           => throw new ArgumentOutOfRangeException(nameof(transformerName), transformerName, null),
        };
        return WrapBody(body);
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

    private static string CreateRepeatedTextControls(int count)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine($"""<text fontSize="{10 + i % 3}" foreground="#202020">Text control benchmark {i}</text>""");
        }

        return builder.ToString();
    }

    private static string CreateRepeatedLineControls(int count)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine($"""<line thickness="{1 + i % 2}px" length="100%" orientation="Horizontal" color="#336699" />""");
        }

        return builder.ToString();
    }

    private static string CreateRepeatedBorderControls(int count)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var background = i % 2 == 0 ? "#FFFFFF" : "#F5F7FA";
            builder.AppendLine($"""<border thickness="1px" color="#202020" background="{background}"><text>Border control benchmark {i}</text></border>""");
        }

        return builder.ToString();
    }

    private static string CreateRepeatedImageControls(int count)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine($"""<image source="{BenchmarkServices.TinyPngDataUri}" width="12px" height="12px" />""");
        }

        return builder.ToString();
    }

    private static string CreateTableXml(int rowCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<table margin="0 0.1cm">""");
        builder.AppendLine("""  <th><td width="20%"><text>#</text></td><td width="50%"><text>Name</text></td><td width="30%"><text>Value</text></td></th>""");
        for (var i = 1; i <= rowCount; i++)
        {
            builder.AppendLine("  <tr>");
            builder.AppendLine($"""    <td><text>{i}</text></td>""");
            builder.AppendLine($"""    <td><text>Table row {i}</text></td>""");
            builder.AppendLine($"""    <td><text horizontalAlignment="Right">{i * 7}</text></td>""");
            builder.AppendLine("  </tr>");
        }

        builder.AppendLine("</table>");
        return builder.ToString();
    }

    private static string CreateLineChartXml(int dataPointCount = 5)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<lineChart height="140px" title="Line benchmark" show-grid="true">""");
        for (var i = 0; i < dataPointCount; i++)
        {
            builder.AppendLine($"""  <data x="{i}" y="{10 + i * 3}" label="P{i}" />""");
        }

        builder.AppendLine("</lineChart>");
        return builder.ToString();
    }

    private static string CreateBarChartXml(int dataPointCount = 5)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<barChart height="140px" title="Bar benchmark" orientation="Vertical" bar-color="#4472C4">""");
        for (var i = 0; i < dataPointCount; i++)
        {
            builder.AppendLine($"""  <data x="{i}" y="{8 + i * 4}" label="B{i}" />""");
        }

        builder.AppendLine("</barChart>");
        return builder.ToString();
    }

    private static string CreatePieChartXml(int dataPointCount = 5)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<pieChart height="160px" title="Pie benchmark" inner-radius="35%" show-labels="true" show-percentages="true">""");
        for (var i = 0; i < dataPointCount; i++)
        {
            builder.AppendLine($"""  <data y="{10 + i * 5}" label="S{i}" />""");
        }

        builder.AppendLine("</pieChart>");
        return builder.ToString();
    }

    private static string CreateForTransformerBody(int count)
    {
        return $$"""
                 @for i from 0 to {{count}} {
                   <text>For row @i</text>
                 }
                 """;
    }

    private static string CreateForEachTransformerBody()
    {
        return """
               @foreach item in items with index {
                 <text>ForEach row @index: @item</text>
               }
               """;
    }

    private static string CreateIfTransformerBody(int count)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine("""
                               @if true {
                               """);
            builder.AppendLine($"""  <text>If row {i}</text>""");
            builder.AppendLine("}");
        }

        return builder.ToString();
    }

    private static string CreateAlternateTransformerBody(int count)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine("""
                               @alternate on background with ["#FFFFFF", "#F5F7FA"] {
                               """);
            builder.AppendLine($"""  <border background="@background"><text>Alternate row {i}</text></border>""");
            builder.AppendLine("}");
        }

        return builder.ToString();
    }

    private static string CreateVarTransformerBody(int count)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine($"@var label = \"Var row {i}\" {{");
            builder.AppendLine("  <text>@label</text>");
            builder.AppendLine("}");
        }

        return builder.ToString();
    }

    private static string CreateExpandedTextRows(int count, string label)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine($"""<text>{label} row {i}</text>""");
        }

        return builder.ToString();
    }

    private static string CreateExpandedAlternateRows(int count)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var background = i % 2 == 0 ? "#FFFFFF" : "#F5F7FA";
            builder.AppendLine($"""<border background="{background}"><text>Alternate row {i}</text></border>""");
        }

        return builder.ToString();
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

    private static byte[] CreateTransformerHeavyGenerationTemplate(bool expanded, int rowCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        builder.AppendLine($"""<template xmlns="{Constants.ControlsNamespace}">""");
        builder.AppendLine("  <body>");
        builder.AppendLine("""    <table margin="0 0.1cm">""");
        builder.AppendLine("""      <th><td width="15%"><text>#</text></td><td width="55%"><text>Description</text></td><td width="30%"><text>Amount</text></td></th>""");
        if (expanded)
        {
            for (var i = 1; i <= rowCount; i++)
            {
                AppendExpandedTransformerHeavyRow(builder, i);
            }
        }
        else
        {
            builder.AppendLine($"      @for i from 1 to {rowCount + 1} {{");
            builder.AppendLine("""        @alternate on background with ["#FFFFFF", "#F5F7FA"] {""");
            builder.AppendLine("          <tr>");
            builder.AppendLine("""            <td><border background="@background"><text>@i</text></border></td>""");
            builder.AppendLine("""            <td><border background="@background"><text>Generated row @i</text></border></td>""");
            builder.AppendLine("""            <td><border background="@background"><text horizontalAlignment="Right">@i</text></border></td>""");
            builder.AppendLine("          </tr>");
            builder.AppendLine("        }");
            builder.AppendLine("      }");
        }

        builder.AppendLine("    </table>");
        builder.AppendLine("  </body>");
        builder.AppendLine("</template>");
        return ToUtf8(builder.ToString());
    }

    private static void AppendExpandedTransformerHeavyRow(StringBuilder builder, int index)
    {
        var background = index % 2 == 1 ? "#FFFFFF" : "#F5F7FA";
        builder.AppendLine("      <tr>");
        builder.AppendLine($"""        <td><border background="{background}"><text>{index}</text></border></td>""");
        builder.AppendLine($"""        <td><border background="{background}"><text>Generated row {index}</text></border></td>""");
        builder.AppendLine($"""        <td><border background="{background}"><text horizontalAlignment="Right">{index}</text></border></td>""");
        builder.AppendLine("      </tr>");
    }

    private static byte[] WrapBody(string body)
        => WrapTemplate($"""
                         <body>
                         {body}
                         </body>
                         """);

    private static byte[] WrapTemplate(string templateContent)
        => ToUtf8($$"""
                   <?xml version="1.0" encoding="utf-8"?>
                   <template xmlns="{{Constants.ControlsNamespace}}">
                   {{templateContent}}
                   </template>
                   """);

    private static byte[] ToUtf8(string value) => Encoding.UTF8.GetBytes(value);
}
