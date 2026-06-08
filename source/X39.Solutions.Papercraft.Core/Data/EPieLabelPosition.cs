namespace X39.Solutions.Papercraft.Data;

/// <summary>
/// Describes how pie chart labels are positioned.
/// </summary>
public enum EPieLabelPosition : sbyte
{
    /// <summary>
    /// Place labels outside the pie.
    /// </summary>
    Outside = 0,

    /// <summary>
    /// Place labels inside slices.
    /// </summary>
    Inside = 1,

    /// <summary>
    /// Place labels in a legend.
    /// </summary>
    Legend = 2,

    /// <summary>
    /// Use outside labels when they fit, otherwise fall back to a legend.
    /// </summary>
    Auto = 3,
}
