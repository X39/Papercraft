using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Services.TextService;

internal interface ITextLayoutService : ITextService
{
    IReadOnlyList<TextLineLayout> Layout(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth);
}

internal readonly record struct TextLineLayout(
    string Text,
    float X,
    float BaselineY,
    float Top,
    float Height,
    float Width);
