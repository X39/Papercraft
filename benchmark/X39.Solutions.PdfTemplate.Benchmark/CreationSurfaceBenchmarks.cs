using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Services;
using X39.Solutions.Papercraft.Xml;
using X39.Solutions.PdfTemplate;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.TemplateCreation, BenchmarkCategories.Parsing, BenchmarkCategories.Transformers)]
public class CreationSurfaceBenchmarks
{
    private readonly ITransformer[] _transformers =
    [
        new Papercraft.Transformers.ForTransformer(),
    ];

    private ServiceProvider _serviceProvider = null!;
    private IControlFactory _controlFactory = null!;
    private byte[] _parameterHeavyTemplate = null!;
    private byte[] _completedAsyncTemplate = null!;
    private byte[] _yieldAsyncTemplate = null!;
    private byte[] _syncFunctionTemplate = null!;
    private byte[] _yieldFunctionTemplate = null!;

    [Params(25, 100, 250)]
    public int ControlCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _serviceProvider = BenchmarkServices.CreateServiceProvider();
        _controlFactory = _serviceProvider.GetRequiredService<IControlFactory>();
        BenchmarkServices.WarmBenchmarkControlCache(
            _serviceProvider,
            _serviceProvider.GetRequiredService<ControlActivationCache>());
        _parameterHeavyTemplate = CreateRepeatedControlTemplate("parameter", ControlCount, includeContent: false);
        _completedAsyncTemplate = CreateRepeatedControlTemplate("completedInit", ControlCount, includeContent: true);
        _yieldAsyncTemplate = CreateRepeatedControlTemplate("yieldInit", ControlCount, includeContent: true);
        _syncFunctionTemplate = CreateFunctionHeavyTemplate("completedInit", "benchmarkFormat", ControlCount);
        _yieldFunctionTemplate = CreateFunctionHeavyTemplate("yieldInit", "benchmarkYieldFormat", ControlCount);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _serviceProvider.Dispose();
    }

    [Benchmark(Baseline = true)]
    public async Task<int> ParseAndCreateParameterHeavyControls()
    {
        var root = await ParseAsync(_parameterHeavyTemplate, useTransformers: false, useFunctions: false)
            .ConfigureAwait(false);
        await using var template = await Template.CreateAsync(
                root,
                _controlFactory,
                BenchmarkServices.Culture,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        return CountControls(template.BodyControls);
    }

    [Benchmark]
    public async Task<int> ParseAndCreateCompletedAsyncInitializedControls()
    {
        var root = await ParseAsync(_completedAsyncTemplate, useTransformers: false, useFunctions: false)
            .ConfigureAwait(false);
        await using var template = await Template.CreateAsync(
                root,
                _controlFactory,
                BenchmarkServices.Culture,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        return CountControls(template.BodyControls);
    }

    [Benchmark]
    public async Task<int> ParseAndCreateYieldAsyncInitializedControls()
    {
        var root = await ParseAsync(_yieldAsyncTemplate, useTransformers: false, useFunctions: false)
            .ConfigureAwait(false);
        await using var template = await Template.CreateAsync(
                root,
                _controlFactory,
                BenchmarkServices.Culture,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        return CountControls(template.BodyControls);
    }

    [Benchmark]
    public async Task<int> ParseTransformAndCreateSyncFunctionControls()
    {
        var root = await ParseAsync(_syncFunctionTemplate, useTransformers: true, useFunctions: true)
            .ConfigureAwait(false);
        await using var template = await Template.CreateAsync(
                root,
                _controlFactory,
                BenchmarkServices.Culture,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        return CountControls(template.BodyControls);
    }

    [Benchmark]
    public async Task<int> ParseTransformAndCreateYieldFunctionControls()
    {
        var root = await ParseAsync(_yieldFunctionTemplate, useTransformers: true, useFunctions: true)
            .ConfigureAwait(false);
        await using var template = await Template.CreateAsync(
                root,
                _controlFactory,
                BenchmarkServices.Culture,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        return CountControls(template.BodyControls);
    }

    private async Task<XmlNodeInformation> ParseAsync(byte[] bytes, bool useTransformers, bool useFunctions)
    {
        var templateData = new TemplateData();
        if (useFunctions)
        {
            templateData.RegisterFunction(new BenchmarkFormatFunction());
            templateData.RegisterFunction(new BenchmarkYieldFormatFunction());
        }

        using var reader = BenchmarkTemplates.CreateXmlReader(bytes);
        using var templateReader = new XmlTemplateReader(
            DocumentOptions.Default,
            BenchmarkServices.Culture,
            templateData,
            useTransformers ? _transformers : Array.Empty<ITransformer>());
        return await templateReader.ReadAsync(reader)
            .ConfigureAwait(false);
    }

    private static byte[] CreateRepeatedControlTemplate(string controlName, int count, bool includeContent)
    {
        var builder = CreateTemplateBuilder();
        for (var i = 0; i < count; i++)
        {
            builder.Append($"""      <{controlName} title="Item {i}" count="{i}" ratio="1.25" width="{10 + i % 4}px">""");
            if (includeContent)
                builder.Append($"Content {i}");
            builder.AppendLine($"""</{controlName}>""");
        }

        return CloseTemplateBuilder(builder);
    }

    private static byte[] CreateFunctionHeavyTemplate(string controlName, string functionName, int count)
    {
        var builder = CreateTemplateBuilder();
        builder.AppendLine($"      @for i from 0 to {count} {{");
        builder.AppendLine($"""        <{controlName} title="@{functionName}(i)" count="@i" ratio="1.25" width="10px">Content @{functionName}(i)</{controlName}>""");
        builder.AppendLine("      }");
        return CloseTemplateBuilder(builder);
    }

    private static StringBuilder CreateTemplateBuilder()
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        builder.AppendLine($"""<template xmlns="{BenchmarkControlNames.Namespace}">""");
        builder.AppendLine("  <body>");
        builder.AppendLine("    <container>");
        return builder;
    }

    private static byte[] CloseTemplateBuilder(StringBuilder builder)
    {
        builder.AppendLine("    </container>");
        builder.AppendLine("  </body>");
        builder.AppendLine("</template>");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static int CountControls(IEnumerable<IControl> controls)
    {
        var count = 0;
        foreach (var control in controls)
        {
            count++;
            if (control is IContentControl contentControl)
                count += CountControls(contentControl);
        }

        return count;
    }
}
