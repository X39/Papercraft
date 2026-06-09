using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

internal static class TableBoxStyle
{
    public static Size GetBorderOffset(Thickness borderThickness, Size bounds, float dpi)
    {
        var border = borderThickness.ToRectangle(bounds, dpi);
        return new Size(border.Left + border.Width, border.Top + border.Height);
    }

    public static Size Deflate(Size size, Size offset)
        => new(Math.Max(0F, size.Width - offset.Width), Math.Max(0F, size.Height - offset.Height));

    public static void Draw(
        IDeferredCanvas canvas,
        Rectangle arrangement,
        Rectangle arrangementInner,
        Color background,
        Thickness borderThickness,
        Color borderColor,
        Size parentSize,
        float dpi)
    {
        using var state = canvas.CreateState();
        canvas.Translate(-arrangementInner);
        canvas.Translate(arrangement);

        if (background.Alpha is not 0)
            canvas.DrawRect(new Rectangle(0F, 0F, arrangement.Width, arrangement.Height), background);

        if (borderColor.Alpha is 0)
            return;

        var border = borderThickness.ToRectangle(parentSize, dpi);
        DrawRectangle(canvas, new Rectangle(0F, 0F, Math.Min(border.Left, arrangement.Width), arrangement.Height), borderColor);
        DrawRectangle(canvas, new Rectangle(0F, 0F, arrangement.Width, Math.Min(border.Top, arrangement.Height)), borderColor);
        DrawRectangle(
            canvas,
            new Rectangle(
                Math.Max(0F, arrangement.Width - border.Width),
                0F,
                Math.Min(border.Width, arrangement.Width),
                arrangement.Height),
            borderColor);
        DrawRectangle(
            canvas,
            new Rectangle(
                0F,
                Math.Max(0F, arrangement.Height - border.Height),
                arrangement.Width,
                Math.Min(border.Height, arrangement.Height)),
            borderColor);
    }

    private static void DrawRectangle(IDeferredCanvas canvas, Rectangle rectangle, Color color)
    {
        if (rectangle is {Width: > 0F, Height: > 0F})
            canvas.DrawRect(rectangle, color);
    }
}
