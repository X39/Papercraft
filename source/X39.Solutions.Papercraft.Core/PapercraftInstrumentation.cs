using System.Diagnostics;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Papercraft diagnostics and distributed tracing entry point.
/// </summary>
public static class PapercraftInstrumentation
{
    /// <summary>
    /// Activity source name used by Papercraft renderer and backend activities.
    /// </summary>
    public const string ActivitySourceName = "X39.Solutions.Papercraft";

    /// <summary>
    /// Activity source used by Papercraft renderer and backend activities.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
