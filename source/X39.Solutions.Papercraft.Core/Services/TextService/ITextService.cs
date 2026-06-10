using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Services.TextService;

/// <summary>
/// Service abstraction for measuring text.
/// </summary>
public interface ITextService
{
    /// <summary>
    /// Measures the given <paramref name="text"/> with the given <paramref name="textStyle"/>.
    /// </summary>
    /// <param name="textStyle">The text style to use.</param>
    /// <param name="dpi"></param>
    /// <param name="text">The text to measure.</param>
    /// <param name="maxWidth">The maximum measured width of a single line.</param>
    /// <returns>The corresponding <see cref="Size"/>.</returns>
    Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth);

    /// <summary>
    /// Renders the given <paramref name="text"/> with the given <paramref name="textStyle"/>.
    /// </summary>
    /// <param name="canvas">The canvas to render on.</param>
    /// <param name="textStyle">The text style to use.</param>
    /// <param name="dpi"></param>
    /// <param name="text">The text to render.</param>
    /// <param name="maxWidth">The maximum width of a single line.</param>
    void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth);
}
