using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Controls.ZXing.Controls;

namespace X39.Solutions.Papercraft.Controls.ZXing;

public static class PapercraftZxingBarcodeControlServiceCollectionExtensions
{
    public static PapercraftServiceBuilder AddPapercraftZxingBarcodeControls(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var builder = services.AddPapercraftCore();
        return builder.AddZxingBarcodeControls();
    }

    public static PapercraftServiceBuilder AddZxingBarcodeControls(this PapercraftServiceBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.AddControl<BarcodeControl>()
               .AddControl<Code128BarcodeControl>()
               .AddControl<Gs1128BarcodeControl>()
               .AddControl<Code39BarcodeControl>()
               .AddControl<Code93BarcodeControl>()
               .AddControl<CodabarBarcodeControl>()
               .AddControl<Ean13BarcodeControl>()
               .AddControl<Ean8BarcodeControl>()
               .AddControl<UpcABarcodeControl>()
               .AddControl<UpcEBarcodeControl>()
               .AddControl<ItfBarcodeControl>()
               .AddControl<DataMatrixBarcodeControl>()
               .AddControl<Pdf417BarcodeControl>()
               .AddControl<AztecBarcodeControl>();
        return builder;
    }
}
