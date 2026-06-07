using System.ComponentModel;

namespace X39.Solutions.Papercraft.Data;

/// <summary>
/// Defines the marker style used by unordered lists.
/// </summary>
[TypeConverter(typeof(EListMarkerStyleConverter))]
public enum EListMarkerStyle
{
    /// <summary>
    /// Uses the default filled marker. Papercraft currently renders this as the ASCII "*" marker.
    /// </summary>
    Disc,

    /// <summary>
    /// Uses an open circle marker. Papercraft currently renders this as the ASCII "o" marker.
    /// </summary>
    Circle,

    /// <summary>
    /// Uses a square marker. Papercraft currently renders this as the ASCII "[]" marker.
    /// </summary>
    Square,

    /// <summary>
    /// Suppresses marker rendering.
    /// </summary>
    None,
}

/// <summary>
/// Converts unordered list marker style names from XML attributes.
/// </summary>
public sealed class EListMarkerStyleConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType.IsEquivalentTo(typeof(string)) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => (destinationType?.IsEquivalentTo(typeof(string)) ?? false) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string source)
            return base.ConvertFrom(context, culture, value);

        if (Enum.TryParse<EListMarkerStyle>(source.Trim(), ignoreCase: true, out var result))
            return result;

        throw new FormatException($"The given string '{source}' is not a valid {nameof(EListMarkerStyle)}.");
    }

    /// <inheritdoc />
    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType)
    {
        if (value is not EListMarkerStyle markerStyle || !destinationType.IsEquivalentTo(typeof(string)))
            return base.ConvertTo(context, culture, value, destinationType);

        return markerStyle.ToString().ToLowerInvariant();
    }
}
