using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls.QrCode;
using X39.Solutions.Papercraft.Controls.QrCode.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public sealed class QrCodeControlTests
{
    private const float Dpi = 96;
    private static readonly Size PageSize = new(200, 200);

    [Fact]
    public void AddQrCodeControlsRegistersControlIdempotently()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore()
                .AddQrCodeControls()
                .AddQrCodeControls();

        var qrCodeRegistrations = services
            .Where((q) => q.ServiceType == typeof(ControlRegistration))
            .Select((q) => ((ControlRegistration)q.ImplementationInstance!).Type)
            .Where((q) => q == typeof(QrCodeControl))
            .ToArray();

        Assert.Single(qrCodeRegistrations);
    }

    [Fact]
    public void QrCodeControlCanBeActivatedFromXmlNameAndContent()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore()
                .AddQrCodeControls();
        using var serviceProvider = services.BuildServiceProvider();

        var control = serviceProvider.GetRequiredService<IControlFactory>()
            .Create(
                Constants.ControlsNamespace,
                "qrCode",
                new Dictionary<string, string>
                {
                    ["value"] = "attribute",
                    ["errorCorrection"] = "Q",
                },
                "content",
                CultureInfo.InvariantCulture);

        var qrCode = Assert.IsType<QrCodeControl>(control);
        Assert.Equal("content", qrCode.Value);
        Assert.Equal(QrCodeErrorCorrectionLevel.Quartile, qrCode.ErrorCorrection);
    }

    [Fact]
    public void QrCodeControlRendersVectorRectanglesWithRequestedColorsAndQuietZone()
    {
        var control = new QrCodeControl
        {
            Value = "https://example.test/qr",
            Size = 100,
            Foreground = Colors.Green,
            Background = Colors.White,
            QuietZone = 4,
        };
        var canvas = new DeferredCanvasMock();

        Assert.Equal(new Size(100, 100), control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture));
        Assert.Equal(new Size(100, 100), control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture));
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        canvas.AssertDrawRect(new Rectangle(0, 0, 100, 100), Colors.White);
        canvas.AssertAnyDrawRectWithColor(Colors.Green);
        canvas.AssertDrawRectCountAtLeast(2);
        canvas.AssertAllDrawRectsWithin(new Rectangle(0, 0, 100, 100));
        canvas.AssertState();
    }

    [Fact]
    public void QrCodeControlRejectsEmptyValue()
    {
        var control = new QrCodeControl { Size = 100 };
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        var exception = Assert.Throws<InvalidOperationException>(
            () => control.Render(new DeferredCanvasMock(), Dpi, PageSize, CultureInfo.InvariantCulture));

        Assert.Contains("must not be empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QrCodeControlRejectsPayloadsTooLargeForQrCode()
    {
        var control = new QrCodeControl
        {
            Value = new string('A', 8_000),
            Size = 100,
            ErrorCorrection = QrCodeErrorCorrectionLevel.High,
        };
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Throws<global::Net.Codecrete.QrCodeGenerator.DataTooLongException>(
            () => control.Render(new DeferredCanvasMock(), Dpi, PageSize, CultureInfo.InvariantCulture));
    }
}
