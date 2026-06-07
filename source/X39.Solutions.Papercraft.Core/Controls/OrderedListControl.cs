using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// A list control that renders ordered item markers.
/// </summary>
[Control(Constants.ControlsNamespace, "ol")]
public sealed class OrderedListControl : ListControlBase
{
    /// <summary>
    /// Creates a new instance of <see cref="OrderedListControl"/>.
    /// </summary>
    /// <param name="textService">The text service used for marker measurement and rendering.</param>
    [ControlConstructor]
    public OrderedListControl(ITextService textService)
        : base(textService)
    {
    }

    /// <summary>
    /// The number assigned to the first list item.
    /// </summary>
    [Parameter]
    public int Start { get; set; } = 1;

    /// <summary>
    /// Composite format used to render ordered markers.
    /// </summary>
    [Parameter]
    public string MarkerFormat { get; set; } = "{0}.";

    /// <inheritdoc />
    protected override string GetMarkerText(int itemIndex, CultureInfo cultureInfo)
        => string.Format(cultureInfo, MarkerFormat, Start + itemIndex);
}
