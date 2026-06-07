namespace X39.Solutions.Papercraft;

/// <summary>
/// Identifies where a render diagnostic originated in a template.
/// </summary>
/// <param name="Line">The one-based XML line number, if known.</param>
/// <param name="Column">The one-based XML column number, if known.</param>
/// <param name="ControlPath">A logical control path, if known.</param>
public sealed record TemplateLocation(int? Line = null, int? Column = null, string? ControlPath = null);
