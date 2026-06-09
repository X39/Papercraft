using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;
using X39.Solutions.PdfTemplate.Test.Mock;
using X39.Util;

namespace X39.Solutions.PdfTemplate.Test.Samples;

public abstract class SampleBase : IAsyncDisposable
{
    private readonly List<ServiceProvider> _serviceProviders = new();

    public Generator CreateGenerator(params IFunction[] functions)
        => CreateGenerator(
            (builder) => builder.AddControl<MockControl>(),
            functions);

    public Generator CreateGenerator(Action<PdfTemplateServiceBuilder> configureServices, params IFunction[] functions)
    {
        ArgumentNullException.ThrowIfNull(configureServices);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateService(configureServices);
        foreach (var function in functions)
        {
            serviceCollection.AddSingleton<IFunction>(function);
        }

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _serviceProviders.Add(serviceProvider);
        return serviceProvider.GetRequiredService<Generator>();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var serviceProvider in _serviceProviders)
        {
            await serviceProvider.DisposeAsync();
        }
    }

    public static IDisposable CreateStream(out Stream stream)
    {
        if (!Debugger.IsAttached)
            return stream = new VoidStream();
        var tmpPath = Path.GetTempPath();
        while (true)
        {
            try
            {
                var tmpFile = Path.Combine(tmpPath, Path.GetRandomFileName() + ".pdf");
                var tmp = stream = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                return new Disposable(
                    () =>
                    {
                        tmp.Dispose();
                        var process = Process.Start(
                            new ProcessStartInfo
                            {
                                FileName        = tmpFile,
                                UseShellExecute = true,
                            });
                        if (process is null)
                            throw new InvalidOperationException("Could not start process.");
                        var now = DateTime.Now;
                        process.WaitForExit();
                        var then = DateTime.Now;
                        if (now - then < TimeSpan.FromMilliseconds(1000) && Debugger.IsAttached)
                            Thread.Sleep(1000); // Yes, we sleep here if a debugger is attached, because sample jobs are for opening the PDF
                        File.Delete(tmpFile);
                    });
            }
            catch (IOException)
            {
                /* empty */
            }
        }
    }
}
