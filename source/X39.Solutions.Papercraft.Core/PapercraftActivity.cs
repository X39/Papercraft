using System.Diagnostics;

namespace X39.Solutions.Papercraft;

internal static class PapercraftActivity
{
    public const string BackendIdTag = "papercraft.backend.id";
    public const string DisplayListCommandCountTag = "papercraft.display_list.command_count";
    public const string DocumentPageCountTag = "papercraft.document.page_count";
    public const string LayerTag = "papercraft.layer";
    public const string PageIndexTag = "papercraft.page.index";
    public const string PageNumberTag = "papercraft.page.number";
    public const string TargetMediaTypeTag = "papercraft.target.media_type";
    public const string TargetOutputKindTag = "papercraft.target.output_kind";
    public const string ValidationDiagnosticCountTag = "papercraft.validation.diagnostic_count";
    public const string ValidationSupportLevelTag = "papercraft.validation.support_level";

    public static Activity? Start(string name)
        => PapercraftInstrumentation.ActivitySource.StartActivity(name, ActivityKind.Internal);

    public static void SetBackend(Activity? activity, IPapercraftRenderBackend backend)
        => activity?.SetTag(BackendIdTag, backend.Capabilities.RendererId);

    public static void SetDocument(Activity? activity, PapercraftDocument document)
        => activity?.SetTag(DocumentPageCountTag, document.Pages.Count);

    public static void SetRenderTarget(Activity? activity, RenderTarget target)
    {
        if (activity is null)
            return;

        activity.SetTag(TargetMediaTypeTag, target.MediaType);
        activity.SetTag(TargetOutputKindTag, target.OutputKind.ToString());
    }

    public static void SetValidation(Activity? activity, RenderValidationResult validation)
    {
        if (activity is null)
            return;

        activity.SetTag(ValidationSupportLevelTag, validation.SupportLevel.ToString());
        activity.SetTag(ValidationDiagnosticCountTag, validation.Diagnostics.Count);
    }

    public static void SetError(Activity? activity, Exception exception)
    {
        if (activity is null)
            return;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("error.type", exception.GetType().FullName);
        activity.SetTag("error.message", exception.Message);
    }
}
