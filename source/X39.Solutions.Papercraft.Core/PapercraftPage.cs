using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft;

/// <summary>
/// A generated backend-neutral Papercraft page.
/// </summary>
/// <param name="PageIndex">Zero-based page index.</param>
/// <param name="PageNumber">One-based page number.</param>
/// <param name="TotalPages">Total number of pages in the document.</param>
/// <param name="PageSize">The generated page size in drawing units.</param>
/// <param name="DotsPerMillimeter">The render density used to generate the page.</param>
/// <param name="DisplayList">The page drawing commands.</param>
public sealed record PapercraftPage(
    int PageIndex,
    int PageNumber,
    int TotalPages,
    Size PageSize,
    float DotsPerMillimeter,
    DisplayList DisplayList);
