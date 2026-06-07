using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls.ZXing;
using X39.Solutions.Papercraft.Controls.ZXing.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public sealed class ZxingBarcodeControlTests
{
    private const float Dpi = 96;
    private static readonly Size PageSize = new(200, 200);

    [Fact]
    public void AddZxingBarcodeControlsRegistersGenericAndAliasControlsIdempotently()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore()
                .AddZxingBarcodeControls()
                .AddZxingBarcodeControls();

        var registrationTypes = services
            .Where((q) => q.ServiceType == typeof(ControlRegistration))
            .Select((q) => ((ControlRegistration)q.ImplementationInstance!).Type)
            .ToArray();

        Assert.Single(registrationTypes, (q) => q == typeof(BarcodeControl));
        Assert.Single(registrationTypes, (q) => q == typeof(Code128BarcodeControl));
        Assert.Single(registrationTypes, (q) => q == typeof(Gs1128BarcodeControl));
        Assert.Single(registrationTypes, (q) => q == typeof(DataMatrixBarcodeControl));
    }

    [Fact]
    public void BarcodeControlCanBeActivatedFromGenericXmlName()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore()
                .AddZxingBarcodeControls();
        using var serviceProvider = services.BuildServiceProvider();

        var control = serviceProvider.GetRequiredService<IControlFactory>()
            .Create(
                Constants.ControlsNamespace,
                "barcode",
                new Dictionary<string, string>
                {
                    ["format"] = "PDF-417",
                    ["value"] = "ABC123",
                    ["quietZone"] = "2",
                    ["gs1Format"] = "true",
                },
                null,
                CultureInfo.InvariantCulture);

        var barcode = Assert.IsType<BarcodeControl>(control);
        Assert.Equal("ABC123", barcode.Value);
        Assert.Equal(ZxingBarcodeFormat.Pdf417, barcode.Format);
        Assert.Equal(2, barcode.QuietZone);
        Assert.True(barcode.Gs1Format);
    }

    [Fact]
    public void BarcodeAliasControlsCanBeActivatedFromXmlNames()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore()
                .AddZxingBarcodeControls();
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IControlFactory>();

        var code128 = Assert.IsType<Code128BarcodeControl>(
            factory.Create(
                Constants.ControlsNamespace,
                "code128",
                new Dictionary<string, string> { ["value"] = "ABC123" },
                null,
                CultureInfo.InvariantCulture));
        var gs1128 = Assert.IsType<Gs1128BarcodeControl>(
            factory.Create(
                Constants.ControlsNamespace,
                "gs1-128",
                new Dictionary<string, string> { ["value"] = "0101234567890128" },
                null,
                CultureInfo.InvariantCulture));
        var dataMatrix = Assert.IsType<DataMatrixBarcodeControl>(
            factory.Create(
                Constants.ControlsNamespace,
                "dataMatrix",
                new Dictionary<string, string> { ["value"] = "ABC123" },
                null,
                CultureInfo.InvariantCulture));

        Assert.Equal(ZxingBarcodeFormat.Code128, code128.Format);
        Assert.Equal(ZxingBarcodeFormat.Gs1128, gs1128.Format);
        Assert.True(gs1128.Gs1Format);
        Assert.Equal(ZxingBarcodeFormat.DataMatrix, dataMatrix.Format);
    }

    [Fact]
    public void BarcodeControlRendersBitMatrixAsVectorRectangles()
    {
        var control = new BarcodeControl
        {
            Value = "ABC123",
            Width = 100,
            Height = 30,
            Foreground = Colors.Green,
            Background = Colors.White,
            QuietZone = 2,
        };
        var canvas = new DeferredCanvasMock();

        Assert.Equal(new Size(100, 30), control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture));
        Assert.Equal(new Size(100, 30), control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture));
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        canvas.AssertDrawRect(new Rectangle(0, 0, 100, 30), Colors.White);
        canvas.AssertAnyDrawRectWithColor(Colors.Green);
        canvas.AssertDrawRectCountAtLeast(2);
        canvas.AssertAllDrawRectsWithin(new Rectangle(0, 0, 100, 30));
        canvas.AssertState();
    }

    [Fact]
    public void BarcodeControlPreservesSquareModulesForMatrixCodes()
    {
        var control = new BarcodeControl
        {
            Value = "ABC123",
            Format = ZxingBarcodeFormat.QrCode,
            Width = 100,
            Height = 40,
            Foreground = Colors.Green,
            Background = Colors.White,
        };
        var canvas = new DeferredCanvasMock();

        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        canvas.AssertDrawRect(new Rectangle(0, 0, 100, 40), Colors.White);
        canvas.AssertAllDrawRectsWithColorWithin(Colors.Green, new Rectangle(30, 0, 40, 40), tolerance: 1F);
        canvas.AssertState();
    }

    [Fact]
    public void BarcodeControlRejectsInvalidEanPayloads()
    {
        var control = new Ean13BarcodeControl
        {
            Value = "ABC123",
            Width = 100,
            Height = 30,
        };
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);

        Assert.Throws<ArgumentException>(
            () => control.Render(new DeferredCanvasMock(), Dpi, PageSize, CultureInfo.InvariantCulture));
    }
}
