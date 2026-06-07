namespace X39.Solutions.Papercraft;

/// <summary>
/// A validation diagnostic produced before rendering.
/// </summary>
/// <param name="Code">A stable diagnostic code.</param>
/// <param name="Level">The support level represented by this diagnostic.</param>
/// <param name="Feature">The renderer feature involved.</param>
/// <param name="Message">A user-facing diagnostic message.</param>
/// <param name="BackendLimitation">The backend limitation that caused the diagnostic, if known.</param>
/// <param name="Location">The template location that caused the diagnostic, if known.</param>
public sealed record RenderDiagnostic(
    string Code,
    RendererSupportLevel Level,
    string Feature,
    string Message,
    string? BackendLimitation = null,
    TemplateLocation? Location = null);
