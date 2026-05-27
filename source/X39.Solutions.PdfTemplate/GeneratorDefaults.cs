using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Transformers;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Methods to set up default controls and transformers.
/// </summary>
[PublicAPI]
public static class GeneratorDefaults
{
    /// <summary>
    /// Adds the default controls and transformers to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the defaults to.</param>
    /// <returns>The <paramref name="services"/> passed to allow chaining.</returns>
    public static IServiceCollection AddPdfTemplateDefaults(this IServiceCollection services)
    {
        services.AddPdfTemplateControl<Controls.BarChart>();
        services.AddPdfTemplateControl<Controls.BorderControl>();
        services.AddPdfTemplateControl<Controls.ChartControl>();
        services.AddPdfTemplateControl<Controls.ChartDataControl>();
        services.AddPdfTemplateControl<Controls.ImageControl>();
        services.AddPdfTemplateControl<Controls.LineChart>();
        services.AddPdfTemplateControl<Controls.LineControl>();
        services.AddPdfTemplateControl<Controls.PageNumberControl>();
        services.AddPdfTemplateControl<Controls.PieChart>();
        services.AddPdfTemplateControl<Controls.TableCellControl>();
        services.AddPdfTemplateControl<Controls.TableControl>();
        services.AddPdfTemplateControl<Controls.TableHeaderControl>();
        services.AddPdfTemplateControl<Controls.TableRowControl>();
        services.AddPdfTemplateControl<Controls.TextControl>();

        services.AddPdfTemplateTransformer<ForTransformer>();
        services.AddPdfTemplateTransformer<IfTransformer>();
        services.AddPdfTemplateTransformer<ForEachTransformer>();
        services.AddPdfTemplateTransformer<AlternateTransformer>();
        services.AddPdfTemplateTransformer<VariableTransformer>();

        return services;
    }
}
