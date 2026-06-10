using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal static class ActivityProfileRunner
{
    private const string DefaultCase = "RepresentativeInvoice";
    private const string DefaultTarget = "png";
    private const string DefaultBackendId = "skiasharp";
    private const int DefaultIterations = 5;

    public static async Task RunAsync(string[] args)
    {
        var options = ActivityProfileOptions.Parse(args);
        var template = GetTemplate(options.CaseName);

        await using var serviceProvider = BenchmarkServices.CreateDefaultServiceProvider();
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        var renderOptions = new PapercraftRenderOptions
        {
            BackendId = options.BackendId,
            DocumentOptions = CreateDocumentOptions(),
        };

        Console.WriteLine(
            $"Warming Papercraft activity profile case '{options.CaseName}' for target '{options.Target}' on backend '{options.BackendId}'.");
        await RunRenderAsync(renderer, template, options.Target, renderOptions)
            .ConfigureAwait(false);

        var measurements = new List<ActivityProfileMeasurement>();
        var currentIteration = 0;
        using var listener = new ActivityListener
        {
            ShouldListenTo = (source) => string.Equals(
                source.Name,
                PapercraftInstrumentation.ActivitySourceName,
                StringComparison.Ordinal),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = (activity) =>
            {
                lock (measurements)
                {
                    measurements.Add(ActivityProfileMeasurement.From(activity, currentIteration));
                }
            },
        };
        ActivitySource.AddActivityListener(listener);

        for (var iteration = 1; iteration <= options.Iterations; iteration++)
        {
            currentIteration = iteration;
            await RunRenderAsync(renderer, template, options.Target, renderOptions)
                .ConfigureAwait(false);
        }

        var summary = ActivityProfileSummary.Create(measurements);
        var outputFile = WriteCsv(summary);
        WriteConsoleSummary(options, summary, outputFile);
    }

    private static async Task RunRenderAsync(
        PapercraftRenderer renderer,
        byte[] template,
        string target,
        PapercraftRenderOptions options)
    {
        using var reader = BenchmarkTemplates.CreateXmlReader(template);
        if (string.Equals(target, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            await using var stream = new MemoryStream();
            await renderer.RenderAsync(
                    reader,
                    new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
                    BenchmarkServices.Culture,
                    options)
                .ConfigureAwait(false);
            return;
        }

        if (!string.Equals(target, "png", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Activity profile target must be 'pdf' or 'png'.", nameof(target));

        var streams = new List<MemoryStream>();
        try
        {
            await renderer.RenderRasterPagesAsync(
                    reader,
                    new RasterPageRenderOutput(
                        PapercraftMediaTypes.ImagePng,
                        (_, _) =>
                        {
                            var stream = new MemoryStream();
                            streams.Add(stream);
                            return ValueTask.FromResult<Stream>(stream);
                        },
                        leaveStreamsOpen: true),
                    BenchmarkServices.Culture,
                    options)
                .ConfigureAwait(false);
        }
        finally
        {
            foreach (var stream in streams)
            {
                await stream.DisposeAsync()
                    .ConfigureAwait(false);
            }
        }
    }

    private static byte[] GetTemplate(string caseName)
    {
        if (string.Equals(caseName, DefaultCase, StringComparison.OrdinalIgnoreCase)
            || string.Equals(caseName, "representative", StringComparison.OrdinalIgnoreCase)
            || string.Equals(caseName, "invoice", StringComparison.OrdinalIgnoreCase))
            return BenchmarkTemplates.RepresentativeGenerationTemplate;

        if (string.Equals(caseName, "TransformerHeavy", StringComparison.OrdinalIgnoreCase))
            return BenchmarkTemplates.TransformerHeavyGenerationTemplate;

        if (string.Equals(caseName, "TransformerHeavyExpanded", StringComparison.OrdinalIgnoreCase))
            return BenchmarkTemplates.TransformerHeavyExpandedGenerationTemplate;

        var controlCase = BenchmarkTemplates.ControlGenerationCases.FirstOrDefault(
            (q) => string.Equals(q, caseName, StringComparison.OrdinalIgnoreCase));
        if (controlCase is not null)
            return BenchmarkTemplates.GetControlGenerationTemplate(controlCase);

        throw new ArgumentException(
            $"Unknown activity profile case '{caseName}'. Use RepresentativeInvoice, TransformerHeavy, TransformerHeavyExpanded, or one of: {string.Join(", ", BenchmarkTemplates.ControlGenerationCases)}.");
    }

    private static DocumentOptions CreateDocumentOptions()
        => new()
        {
            DotsPerInch = 72,
            Modified = new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc),
            Producer = "X39.Solutions.PdfTemplate.Benchmark.ActivityProfile",
        };

    private static string WriteCsv(IReadOnlyCollection<ActivityProfileSummary> summary)
    {
        var directory = Path.Combine("BenchmarkDotNet.Artifacts", "activity-profile");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(
            directory,
            $"papercraft-activity-profile-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
        var builder = new StringBuilder();
        builder.AppendLine("Name,Layer,BackendId,TargetOutputKind,Count,TotalMilliseconds,MeanMilliseconds,MinMilliseconds,MaxMilliseconds");
        foreach (var row in summary)
        {
            builder.Append(Escape(row.Name));
            builder.Append(',');
            builder.Append(Escape(row.Layer));
            builder.Append(',');
            builder.Append(Escape(row.BackendId));
            builder.Append(',');
            builder.Append(Escape(row.TargetOutputKind));
            builder.Append(',');
            builder.Append(row.Count.ToString(CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(row.TotalMilliseconds.ToString("F4", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(row.MeanMilliseconds.ToString("F4", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(row.MinMilliseconds.ToString("F4", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.AppendLine(row.MaxMilliseconds.ToString("F4", CultureInfo.InvariantCulture));
        }

        File.WriteAllText(file, builder.ToString());
        return Path.GetFullPath(file);
    }

    private static void WriteConsoleSummary(
        ActivityProfileOptions options,
        IReadOnlyCollection<ActivityProfileSummary> summary,
        string outputFile)
    {
        Console.WriteLine();
        Console.WriteLine(
            $"Papercraft activity profile: case={options.CaseName}, target={options.Target}, backend={options.BackendId}, iterations={options.Iterations}");
        Console.WriteLine($"CSV: {outputFile}");
        Console.WriteLine();
        Console.WriteLine("Activity                                             Count     Mean ms    Total ms");
        Console.WriteLine("--------------------------------------------------------------------------------");
        foreach (var row in summary.OrderByDescending((q) => q.TotalMilliseconds).Take(20))
        {
            var name = string.IsNullOrEmpty(row.Layer)
                ? row.Name
                : $"{row.Name}[{row.Layer}]";
            Console.WriteLine(
                $"{Trim(name, 52),-52} {row.Count,5} {row.MeanMilliseconds,10:F4} {row.TotalMilliseconds,10:F4}");
        }
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        if (!value.Contains('"') && !value.Contains(',') && !value.Contains('\n') && !value.Contains('\r'))
            return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string Trim(string value, int length)
        => value.Length <= length ? value : value[..(length - 3)] + "...";

    private sealed record ActivityProfileOptions(
        string CaseName,
        string Target,
        string BackendId,
        int Iterations)
    {
        public static ActivityProfileOptions Parse(IReadOnlyList<string> args)
        {
            var iterationsText = ReadOption(args, "--iterations", DefaultIterations.ToString(CultureInfo.InvariantCulture));
            if (!int.TryParse(iterationsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var iterations)
                || iterations < 1)
                throw new ArgumentException("--iterations must be a positive integer.");

            return new ActivityProfileOptions(
                ReadOption(args, "--case", DefaultCase),
                ReadOption(args, "--target", DefaultTarget),
                ReadOption(args, "--backendId", DefaultBackendId),
                iterations);
        }

        private static string ReadOption(IReadOnlyList<string> args, string name, string defaultValue)
        {
            for (var i = 0; i < args.Count; i++)
            {
                var argument = args[i];
                if (argument.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
                    return argument[(name.Length + 1)..];
                if (string.Equals(argument, name, StringComparison.OrdinalIgnoreCase)
                    && i + 1 < args.Count)
                    return args[i + 1];
            }

            return defaultValue;
        }
    }

    private sealed record ActivityProfileMeasurement(
        int Iteration,
        string Name,
        string? Layer,
        string? BackendId,
        string? TargetOutputKind,
        double DurationMilliseconds)
    {
        public static ActivityProfileMeasurement From(Activity activity, int iteration)
            => new(
                iteration,
                activity.OperationName,
                GetTag(activity, PapercraftActivity.LayerTag),
                GetTag(activity, PapercraftActivity.BackendIdTag),
                GetTag(activity, PapercraftActivity.TargetOutputKindTag),
                activity.Duration.TotalMilliseconds);

        private static string? GetTag(Activity activity, string key)
            => activity.TagObjects.FirstOrDefault((q) => string.Equals(q.Key, key, StringComparison.Ordinal)).Value?.ToString();
    }

    private sealed record ActivityProfileSummary(
        string Name,
        string? Layer,
        string? BackendId,
        string? TargetOutputKind,
        int Count,
        double TotalMilliseconds,
        double MeanMilliseconds,
        double MinMilliseconds,
        double MaxMilliseconds)
    {
        public static IReadOnlyCollection<ActivityProfileSummary> Create(IReadOnlyCollection<ActivityProfileMeasurement> measurements)
            => measurements
                .GroupBy((q) => new
                {
                    q.Name,
                    q.Layer,
                    q.BackendId,
                    q.TargetOutputKind,
                })
                .Select(
                    (q) =>
                    {
                        var durations = q.Select((measurement) => measurement.DurationMilliseconds).ToArray();
                        return new ActivityProfileSummary(
                            q.Key.Name,
                            q.Key.Layer,
                            q.Key.BackendId,
                            q.Key.TargetOutputKind,
                            durations.Length,
                            durations.Sum(),
                            durations.Average(),
                            durations.Min(),
                            durations.Max());
                    })
                .OrderBy((q) => q.Name, StringComparer.Ordinal)
                .ThenBy((q) => q.Layer, StringComparer.Ordinal)
                .ToArray();
    }
}
