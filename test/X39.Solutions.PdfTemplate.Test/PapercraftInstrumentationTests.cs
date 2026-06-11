using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftInstrumentationTests
{
    [Fact]
    public async Task GeneratePdfEmitsRendererGeneratorTemplateAndBackendActivities()
    {
        using var capture = ActivityCapture.Start();
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>instrumented pdf</text>");

        await renderer.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);
        var activities = capture.Snapshot();

        Assert.Contains(activities, (q) => q.Name == "Papercraft.Renderer.Render");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.Renderer.SelectBackend" && q.Tag("papercraft.backend.id") == "skiasharp");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.Generator.Generate");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.Template.ReadXml");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.Template.Transform");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.Template.Materialize");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.Template.Create");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.Generator.MeasureArrange");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.Generator.ComposePages");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.SkiaSharp.RenderPdf");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.SkiaSharp.DisplayList.Render");

        var renderActivity = Assert.Single(activities, (q) => q.Name == "Papercraft.Renderer.Render");
        Assert.Equal(PapercraftMediaTypes.ApplicationPdf, renderActivity.Tag("papercraft.target.media_type"));
        Assert.Equal(RendererOutputKind.Pdf.ToString(), renderActivity.Tag("papercraft.target.output_kind"));
        Assert.Equal(ActivityStatusCode.Unset, renderActivity.Status);
    }

    [Fact]
    public async Task RenderRasterPagesEmitsRasterPageAndEncodeActivities()
    {
        using var capture = ActivityCapture.Start();
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        var pageStreams = new List<MemoryStream>();
        using var reader = CreateReader("<text>instrumented raster</text>");

        await renderer.RenderRasterPagesAsync(
            reader,
            new RasterPageRenderOutput(
                PapercraftMediaTypes.ImagePng,
                (_, _) =>
                {
                    var stream = new MemoryStream();
                    pageStreams.Add(stream);
                    return ValueTask.FromResult<Stream>(stream);
                },
                leaveStreamsOpen: true),
            CultureInfo.InvariantCulture);
        var activities = capture.Snapshot();

        Assert.NotEmpty(pageStreams);
        Assert.Contains(activities, (q) => q.Name == "Papercraft.Renderer.RenderRasterPages");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.SkiaSharp.RenderRasterPages");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.SkiaSharp.RenderRasterPage");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.SkiaSharp.RenderPageToBitmap");
        Assert.Contains(activities, (q) => q.Name == "Papercraft.SkiaSharp.EncodePng");

        var pageActivity = Assert.Single(activities, (q) => q.Name == "Papercraft.SkiaSharp.RenderRasterPage");
        Assert.Equal(0, Convert.ToInt32(pageActivity.Tag("papercraft.page.index"), CultureInfo.InvariantCulture));
        Assert.Equal(1, Convert.ToInt32(pageActivity.Tag("papercraft.page.number"), CultureInfo.InvariantCulture));

        foreach (var pageStream in pageStreams)
        {
            await pageStream.DisposeAsync();
        }
    }

    [Fact]
    public async Task RenderExceptionMarksRendererAndBackendActivitiesAsError()
    {
        using var capture = ActivityCapture.Start();
        var services = new ServiceCollection();
        services.AddPapercraftCore();
        services.AddSingleton<IPapercraftRenderBackend>(new ThrowingRenderBackend());

        await using var serviceProvider = services.BuildServiceProvider();
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<spacer height=\"1px\" />");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await renderer.RenderAsync(
                reader,
                new RenderOutput(PapercraftMediaTypes.ApplicationPdf, output),
                CultureInfo.InvariantCulture));

        Assert.Equal("render failed", exception.Message);
        var activities = capture.Snapshot();
        Assert.Contains(
            activities,
            (q) => q is { Name: "Papercraft.Renderer.Render", Status: ActivityStatusCode.Error });
        Assert.Contains(
            activities,
            (q) => q is { Name: "Papercraft.Renderer.BackendRender", Status: ActivityStatusCode.Error });
    }

    private static XmlReader CreateReader(string body)
    {
        var xml = $$"""
                    <?xml version="1.0" encoding="utf-8"?>
                    <template xmlns="{{Constants.ControlsNamespace}}">
                        <body>
                            {{body}}
                        </body>
                    </template>
                    """;
        return XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
    }

    private sealed class ThrowingRenderBackend : IPapercraftRenderBackend
    {
        public RendererCapabilities Capabilities { get; } = new(
            "throwing",
            "Throwing",
            RendererOutputKind.Pdf,
            new[] { PapercraftMediaTypes.ApplicationPdf });

        public ITextService TextService { get; } = new MinimalTextService();

        public ValueTask<RenderValidationResult> ValidateAsync(
            PapercraftDocument document,
            RenderTarget target,
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult(Capabilities.ValidateTarget(target));

        public ValueTask RenderAsync(
            PapercraftDocument document,
            RenderOutput output,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("render failed");

        public ValueTask RenderRasterPagesAsync(
            PapercraftDocument document,
            RasterPageRenderOutput output,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class MinimalTextService : ITextService
    {
        public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
            => new(Math.Max(1F, text.Length), Math.Max(1F, textStyle.FontSize));

        public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            ArgumentNullException.ThrowIfNull(canvas);
            canvas.DrawText(textStyle, dpi, text.ToString(), 0F, textStyle.FontSize);
        }
    }

    private sealed class ActivityCapture : IDisposable
    {
        private const string TestActivitySourceName = "X39.Solutions.PdfTemplate.Test.PapercraftInstrumentation";

        private readonly ActivityListener _listener;
        private readonly object _gate = new();
        private readonly ActivitySource _testActivitySource;
        private readonly Activity _rootActivity;
        private readonly ActivityTraceId _rootTraceId;

        private ActivityCapture()
        {
            _listener = new ActivityListener
            {
                ShouldListenTo = (source) => string.Equals(
                                             source.Name,
                                             PapercraftInstrumentation.ActivitySourceName,
                                             StringComparison.Ordinal)
                                             || string.Equals(
                                                 source.Name,
                                                 TestActivitySourceName,
                                                 StringComparison.Ordinal),
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = (activity) =>
                {
                    if (!string.Equals(
                            activity.Source.Name,
                            PapercraftInstrumentation.ActivitySourceName,
                            StringComparison.Ordinal)
                        || activity.TraceId != _rootTraceId)
                        return;

                    lock (_gate)
                    {
                        Activities.Add(CapturedActivity.From(activity));
                    }
                },
            };
            ActivitySource.AddActivityListener(_listener);
            _testActivitySource = new ActivitySource(TestActivitySourceName);
            _rootActivity = _testActivitySource.StartActivity("PapercraftInstrumentationTest", ActivityKind.Internal)
                            ?? throw new InvalidOperationException("Failed to start the instrumentation test root activity.");
            _rootTraceId = _rootActivity.TraceId;
        }

        private List<CapturedActivity> Activities { get; } = new();

        public static ActivityCapture Start()
            => new();

        public IReadOnlyCollection<CapturedActivity> Snapshot()
        {
            lock (_gate)
            {
                return Activities.ToArray();
            }
        }

        public void Dispose()
        {
            _rootActivity.Dispose();
            _testActivitySource.Dispose();
            _listener.Dispose();
        }
    }

    private sealed record CapturedActivity(
        string Name,
        ActivityStatusCode Status,
        IReadOnlyDictionary<string, object?> Tags)
    {
        public static CapturedActivity From(Activity activity)
        {
            var tags = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var tag in activity.TagObjects)
            {
                tags[tag.Key] = tag.Value;
            }

            return new CapturedActivity(activity.OperationName, activity.Status, tags);
        }

        public string? Tag(string name)
            => Tags.TryGetValue(name, out var value) ? value?.ToString() : null;
    }
}
