using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;
using X39.Solutions.Papercraft.Services;
using X39.Solutions.PdfTemplate;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal static class BenchmarkServices
{
    public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    public static readonly IReadOnlyDictionary<string, string> EmptyParameters = new Dictionary<string, string>();

    public static readonly IReadOnlyDictionary<string, string> LightParameters = new Dictionary<string, string>
    {
        ["TITLE"] = "Benchmark",
        ["COUNT"] = "42",
    };

    public static readonly IReadOnlyDictionary<string, string> HeavyParameters = new Dictionary<string, string>
    {
        ["TITLE"]       = "Benchmark",
        ["ENABLED"]     = "true",
        ["COUNT"]       = "42",
        ["RATIO"]       = "1.5",
        ["ORIENTATION"] = "Horizontal",
        ["WIDTH"]       = "12px",
        ["PADDING"]     = "1px 2px 3px 4px",
        ["COLOR"]       = "#336699",
    };

    public static readonly IReadOnlyDictionary<string, string> AsyncParameters = new Dictionary<string, string>
    {
        ["TITLE"] = "Benchmark",
        ["COUNT"] = "42",
        ["RATIO"] = "1.5",
        ["WIDTH"] = "12px",
    };

    public const string ContentText = "Deterministic content parameter text.";
    public const string TinyPngDataUri = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=";

    public static readonly string[] BuiltInControlNames =
    [
        "barChart",
        "border",
        "chart",
        "data",
        "image",
        "lineChart",
        "line",
        "pageNumber",
        "pieChart",
        "td",
        "table",
        "th",
        "tr",
        "text",
    ];

    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService(
            (builder) => builder
                .AddControl<NoDependencyControl>()
                .AddControl<ServiceDependencyControl>()
                .AddControl<ParameterHeavyControl>()
                .AddControl<ContentParameterControl>()
                .AddControl<BenchmarkContainerControl>()
                .AddControl<CompletedInitializedControl>()
                .AddControl<YieldInitializedControl>());
        services.AddSingleton<IBenchmarkDependency, BenchmarkDependency>();
        return services.BuildServiceProvider();
    }

    public static ServiceProvider CreateDefaultServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService();
        return services.BuildServiceProvider();
    }

    public static void WarmBenchmarkControlCache(IServiceProvider serviceProvider, ControlActivationCache cache)
    {
        cache.CreateControl(serviceProvider, typeof(NoDependencyControl), EmptyParameters, null, Culture);
        cache.CreateControl(serviceProvider, typeof(ServiceDependencyControl), EmptyParameters, null, Culture);
        cache.CreateControl(serviceProvider, typeof(ParameterHeavyControl), HeavyParameters, null, Culture);
        cache.CreateControl(serviceProvider, typeof(ContentParameterControl), EmptyParameters, ContentText, Culture);
        cache.CreateControl(serviceProvider, typeof(BenchmarkContainerControl), EmptyParameters, null, Culture);
        cache.CreateControl(serviceProvider, typeof(CompletedInitializedControl), AsyncParameters, ContentText, Culture);
        cache.CreateControl(serviceProvider, typeof(YieldInitializedControl), AsyncParameters, ContentText, Culture);
    }

    public static void WarmDefaultControlCache(IServiceProvider serviceProvider, ControlActivationCache cache)
    {
        foreach (var controlName in BuiltInControlNames)
        {
            cache.CreateControl(
                serviceProvider,
                GetBuiltInControlType(controlName),
                GetBuiltInControlParameters(controlName),
                GetBuiltInControlContent(controlName),
                Culture);
        }
    }

    public static Generator CreateDefaultGenerator(ServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<Generator>();
    }

    public static Type GetBuiltInControlType(string controlName)
        => controlName switch
        {
            "barChart"   => typeof(BarChart),
            "border"     => typeof(BorderControl),
            "chart"      => typeof(ChartControl),
            "data"       => typeof(ChartDataControl),
            "image"      => typeof(ImageControl),
            "lineChart"  => typeof(LineChart),
            "line"       => typeof(LineControl),
            "pageNumber" => typeof(PageNumberControl),
            "pieChart"   => typeof(PieChart),
            "td"         => typeof(TableCellControl),
            "table"      => typeof(TableControl),
            "th"         => typeof(TableHeaderControl),
            "tr"         => typeof(TableRowControl),
            "text"       => typeof(TextControl),
            _            => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null),
        };

    public static IReadOnlyDictionary<string, string> GetBuiltInControlParameters(string controlName)
        => controlName switch
        {
            "barChart" => Merge(
                CommonAlignmentParameters,
                ChartParameters,
                new Dictionary<string, string>
                {
                    ["ORIENTATION"] = "Vertical",
                    ["BAR-WIDTH"]   = "8px",
                    ["BAR-SPACING"] = "20%",
                    ["BAR-COLOR"]   = "#4472C4",
                }),
            "border" => Merge(
                CommonAlignmentParameters,
                new Dictionary<string, string>
                {
                    ["THICKNESS"]  = "1px",
                    ["COLOR"]      = "#202020",
                    ["BACKGROUND"] = "#F5F7FA",
                }),
            "chart" => CommonAlignmentParameters,
            "data" => new Dictionary<string, string>
            {
                ["X"]       = "1",
                ["Y"]       = "42",
                ["X-LABEL"] = "X",
                ["Y-LABEL"] = "Y",
                ["COLOR"]   = "#70AD47",
                ["LABEL"]   = "Point",
            },
            "image" => Merge(
                CommonAlignmentParameters,
                new Dictionary<string, string>
                {
                    ["SOURCE"] = TinyPngDataUri,
                    ["WIDTH"]  = "16px",
                    ["HEIGHT"] = "16px",
                }),
            "lineChart" => Merge(
                CommonAlignmentParameters,
                ChartParameters,
                new Dictionary<string, string>
                {
                    ["LINE-THICKNESS"] = "2px",
                    ["LINE-COLOR"]     = "#4472C4",
                    ["SHOW-POINTS"]    = "true",
                    ["POINT-SIZE"]     = "4",
                }),
            "line" => Merge(
                CommonAlignmentParameters,
                new Dictionary<string, string>
                {
                    ["THICKNESS"]   = "1px",
                    ["LENGTH"]      = "100%",
                    ["ORIENTATION"] = "Horizontal",
                    ["COLOR"]       = "#336699",
                }),
            "pageNumber" => Merge(
                CommonTextParameters,
                new Dictionary<string, string>
                {
                    ["PREFIX"]    = "Page ",
                    ["DELIMITER"] = " of ",
                    ["MODE"]      = "CurrentTotal",
                    ["SUFFIX"]    = "",
                }),
            "pieChart" => Merge(
                CommonAlignmentParameters,
                ChartParameters,
                new Dictionary<string, string>
                {
                    ["START-ANGLE"]      = "0",
                    ["INNER-RADIUS"]     = "35%",
                    ["SHOW-PERCENTAGES"] = "true",
                    ["SHOW-LABELS"]      = "true",
                }),
            "td" => Merge(
                CommonAlignmentParameters,
                new Dictionary<string, string>
                {
                    ["WIDTH"]      = "25%",
                    ["COLUMNSPAN"] = "1",
                }),
            "table" => CommonAlignmentParameters,
            "th"    => CommonAlignmentParameters,
            "tr"    => CommonAlignmentParameters,
            "text"  => CommonTextParameters,
            _       => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null),
        };

    public static string? GetBuiltInControlContent(string controlName)
        => controlName == "text" ? ContentText : null;

    private static readonly IReadOnlyDictionary<string, string> CommonAlignmentParameters =
        new Dictionary<string, string>
        {
            ["MARGIN"]              = "0px",
            ["PADDING"]             = "1px",
            ["CLIP"]                = "true",
            ["HORIZONTALALIGNMENT"] = "Stretch",
            ["VERTICALALIGNMENT"]   = "Top",
        };

    private static readonly IReadOnlyDictionary<string, string> CommonTextParameters = Merge(
        CommonAlignmentParameters,
        new Dictionary<string, string>
        {
            ["FOREGROUND"]      = "#202020",
            ["FONTSIZE"]        = "11",
            ["LINEHEIGHT"]      = "1.2",
            ["SCALE"]           = "1",
            ["ROTATION"]        = "0",
            ["STROKETHICKNESS"] = "0",
            ["WEIGHT"]          = "Normal",
            ["STYLE"]           = "Normal",
            ["FONTFAMILY"]      = "Arial",
        });

    private static readonly IReadOnlyDictionary<string, string> ChartParameters =
        new Dictionary<string, string>
        {
            ["WIDTH"]        = "100%",
            ["HEIGHT"]       = "140px",
            ["TITLE"]        = "Benchmark chart",
            ["SHOW-GRID"]    = "true",
            ["GRID-COLOR"]   = "#CCCCCC",
            ["AXIS-COLOR"]   = "#202020",
            ["SHOW-X-AXIS"]  = "true",
            ["SHOW-Y-AXIS"]  = "true",
            ["X-AXIS-LABEL"] = "X",
            ["Y-AXIS-LABEL"] = "Y",
        };

    private static IReadOnlyDictionary<string, string> Merge(
        params IReadOnlyDictionary<string, string>[] dictionaries)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dictionary in dictionaries)
        {
            foreach (var (key, value) in dictionary)
            {
                result[key] = value;
            }
        }

        return result;
    }
}
