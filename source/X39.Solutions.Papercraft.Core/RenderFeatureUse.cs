namespace X39.Solutions.Papercraft;

/// <summary>
/// Describes one renderer-relevant feature used by a prepared template.
/// </summary>
/// <param name="Feature">The renderer feature used by the template.</param>
/// <param name="Location">Where the feature was observed, if known.</param>
public sealed record RenderFeatureUse(string Feature, TemplateLocation? Location = null);
