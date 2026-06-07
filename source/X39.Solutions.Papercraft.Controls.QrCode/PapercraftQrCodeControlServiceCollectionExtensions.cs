using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Controls.QrCode.Controls;

namespace X39.Solutions.Papercraft.Controls.QrCode;

public static class PapercraftQrCodeControlServiceCollectionExtensions
{
    public static PapercraftServiceBuilder AddPapercraftQrCodeControls(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var builder = services.AddPapercraftCore();
        return builder.AddQrCodeControls();
    }

    public static PapercraftServiceBuilder AddQrCodeControls(this PapercraftServiceBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.AddControl<QrCodeControl>();
        return builder;
    }
}
