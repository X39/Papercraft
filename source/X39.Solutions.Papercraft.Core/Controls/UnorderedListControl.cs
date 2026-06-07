using System.ComponentModel;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// A list control that renders unordered item markers.
/// </summary>
[Control(Constants.ControlsNamespace, "ul")]
public sealed class UnorderedListControl : ListControlBase
{
    /// <summary>
    /// Creates a new instance of <see cref="UnorderedListControl"/>.
    /// </summary>
    /// <param name="textService">The text service used for marker measurement and rendering.</param>
    [ControlConstructor]
    public UnorderedListControl(ITextService textService)
        : base(textService)
    {
    }

    /// <summary>
    /// The unordered marker style.
    /// </summary>
    [Parameter]
    public EListMarkerStyle Marker { get; set; } = EListMarkerStyle.Disc;

    /// <inheritdoc />
    protected override string GetMarkerText(int itemIndex, CultureInfo cultureInfo)
        => Marker switch
        {
            EListMarkerStyle.Disc => "*",
            EListMarkerStyle.Circle => "o",
            EListMarkerStyle.Square => "[]",
            EListMarkerStyle.None => string.Empty,
            _ => throw new InvalidEnumArgumentException(nameof(Marker), (int) Marker, typeof(EListMarkerStyle)),
        };
}
