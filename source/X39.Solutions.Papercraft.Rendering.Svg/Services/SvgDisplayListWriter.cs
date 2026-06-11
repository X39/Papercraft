using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft.Rendering.Svg.Services;

internal sealed class SvgDisplayListWriter
{
    private int _clipPathIndex;

    public void WritePage(
        XmlWriter writer,
        PapercraftPage page,
        float offsetY,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(page);

        var pageClipId = CreateClipPathId();
        WriteClipPathDefinition(
            writer,
            pageClipId,
            new DisplayRectangle(0, offsetY, page.PageSize.Width, page.PageSize.Height));

        SvgXml.WriteStartSvgElement(writer, "g");
        writer.WriteAttributeString("id", $"page-{page.PageNumber.ToString(CultureInfo.InvariantCulture)}");
        writer.WriteAttributeString("data-page-index", page.PageIndex.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("data-page-number", page.PageNumber.ToString(CultureInfo.InvariantCulture));

        SvgXml.WriteStartSvgElement(writer, "g");
        writer.WriteAttributeString("clip-path", SvgXml.FormatUrlReference(pageClipId));
        WriteDisplayList(writer, page.DisplayList, offsetY, cancellationToken);
        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    private void WriteDisplayList(
        XmlWriter writer,
        DisplayList displayList,
        float offsetY,
        CancellationToken cancellationToken)
    {
        var stateStack = new Stack<StateFrame>();
        stateStack.Push(new StateFrame(0F, offsetY));

        foreach (var command in displayList.Commands)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteCommand(writer, command, stateStack);
        }

        while (stateStack.Count > 0)
            CloseFrame(writer, stateStack.Pop());
    }

    private void WriteCommand(
        XmlWriter writer,
        DisplayCommand command,
        Stack<StateFrame> stateStack)
    {
        switch (command)
        {
            case PushStateCommand:
                PushState(writer, stateStack);
                break;
            case PopStateCommand:
                PopState(writer, stateStack);
                break;
            case TranslateCommand translate:
                stateStack.Peek().Translate(translate.Offset.X, translate.Offset.Y);
                break;
            case ClipCommand clip:
                WriteClip(writer, clip, stateStack.Peek());
                break;
            case DrawRectangleCommand rectangle:
                WriteRectangle(writer, rectangle, stateStack.Peek());
                break;
            case DrawLineCommand line:
                WriteLine(writer, line, stateStack.Peek());
                break;
            case DrawTextCommand text:
                WriteText(writer, text, stateStack.Peek());
                break;
            case DrawImageCommand image:
                WriteImage(writer, image, stateStack.Peek());
                break;
            case LinkAnnotationCommand link:
                WriteLinkAnnotation(writer, link, stateStack.Peek());
                break;
            case BackendDrawCommand backendDraw:
                SvgXml.WriteComment(writer, $"Skipped backend draw command: {backendDraw.Description}");
                break;
            default:
                SvgXml.WriteComment(writer, $"Skipped unsupported display command: {command.GetType().Name}");
                break;
        }
    }

    private static void PushState(XmlWriter writer, Stack<StateFrame> stateStack)
    {
        var currentFrame = stateStack.Peek();
        var frame = new StateFrame(currentFrame.TranslateX, currentFrame.TranslateY);
        SvgXml.WriteStartSvgElement(writer, "g");
        frame.OpenElementCount++;
        stateStack.Push(frame);
    }

    private static void PopState(XmlWriter writer, Stack<StateFrame> stateStack)
    {
        if (stateStack.Count <= 1)
        {
            SvgXml.WriteComment(writer, "Ignored unmatched drawing state restore.");
            return;
        }

        CloseFrame(writer, stateStack.Pop());
    }

    private void WriteClip(
        XmlWriter writer,
        ClipCommand clip,
        StateFrame frame)
    {
        var clipPathId = CreateClipPathId();
        WriteClipPathDefinition(writer, clipPathId, TransformRectangle(clip.Rectangle, frame));

        SvgXml.WriteStartSvgElement(writer, "g");
        writer.WriteAttributeString("clip-path", SvgXml.FormatUrlReference(clipPathId));
        frame.OpenElementCount++;
    }

    private static void WriteRectangle(
        XmlWriter writer,
        DrawRectangleCommand rectangle,
        StateFrame frame)
    {
        SvgXml.WriteStartSvgElement(writer, "rect");
        WriteRectangleAttributes(writer, TransformRectangle(rectangle.Rectangle, frame));
        WriteFillAttributes(writer, rectangle.Color);
        writer.WriteEndElement();
    }

    private static void WriteLine(
        XmlWriter writer,
        DrawLineCommand line,
        StateFrame frame)
    {
        SvgXml.WriteStartSvgElement(writer, "line");
        writer.WriteAttributeString("x1", SvgXml.FormatNumber(line.StartX + frame.TranslateX));
        writer.WriteAttributeString("y1", SvgXml.FormatNumber(line.StartY + frame.TranslateY));
        writer.WriteAttributeString("x2", SvgXml.FormatNumber(line.EndX + frame.TranslateX));
        writer.WriteAttributeString("y2", SvgXml.FormatNumber(line.EndY + frame.TranslateY));
        writer.WriteAttributeString("fill", "none");
        WriteStrokeAttributes(writer, line.Color, line.Thickness);
        writer.WriteEndElement();
    }

    private static void WriteText(
        XmlWriter writer,
        DrawTextCommand text,
        StateFrame frame)
    {
        if (string.IsNullOrEmpty(text.Text))
            return;

        var x = text.X + frame.TranslateX;
        var y = text.Y + frame.TranslateY;
        var hasTransform = !SvgXml.IsNearlyEqual(text.TextStyle.Scale, 1F)
                           || !SvgXml.IsNearlyZero(text.TextStyle.Rotation);
        if (hasTransform)
        {
            SvgXml.WriteStartSvgElement(writer, "g");
            writer.WriteAttributeString(
                "transform",
                SvgXml.FormatTextTransform(x, y, text.TextStyle.Scale, text.TextStyle.Rotation));
        }

        SvgXml.WriteStartSvgElement(writer, "text");
        writer.WriteAttributeString("x", SvgXml.FormatNumber(hasTransform ? 0 : x));
        writer.WriteAttributeString("y", SvgXml.FormatNumber(hasTransform ? 0 : y));
        writer.WriteAttributeString("font-family", text.TextStyle.FontFamily.Family);
        writer.WriteAttributeString("font-size", SvgXml.FormatNumber(GetTextSize(text)));
        WriteFontAttributes(writer, text.TextStyle.FontFamily);
        WriteFillAttributes(writer, text.TextStyle.Foreground);
        WriteTextDecorationAttributes(writer, text.TextStyle.Decoration);
        writer.WriteAttributeString("xml", "space", SvgXml.XmlNamespace, "preserve");
        writer.WriteString(text.Text);
        writer.WriteEndElement();

        if (hasTransform)
            writer.WriteEndElement();
    }

    private static void WriteImage(
        XmlWriter writer,
        DrawImageCommand image,
        StateFrame frame)
    {
        var mediaType = SvgImageMediaTypeDetector.Detect(image.Bytes);
        if (mediaType is null)
        {
            SvgXml.WriteComment(writer, "Skipped image command with unrecognized encoded image bytes.");
            return;
        }

        SvgXml.WriteStartSvgElement(writer, "image");
        WriteRectangleAttributes(writer, TransformRectangle(image.Rectangle, frame));
        writer.WriteAttributeString("preserveAspectRatio", "none");
        var uri = $"data:{mediaType};base64,{Convert.ToBase64String(image.Bytes)}";
        writer.WriteAttributeString("href", uri);
        writer.WriteAttributeString("xlink", "href", SvgXml.XlinkNamespace, uri);
        writer.WriteEndElement();
    }

    private static void WriteLinkAnnotation(
        XmlWriter writer,
        LinkAnnotationCommand link,
        StateFrame frame)
    {
        SvgXml.WriteStartSvgElement(writer, "a");
        writer.WriteAttributeString("href", link.Uri);
        writer.WriteAttributeString("xlink", "href", SvgXml.XlinkNamespace, link.Uri);

        SvgXml.WriteStartSvgElement(writer, "rect");
        WriteRectangleAttributes(writer, TransformRectangle(link.Rectangle, frame));
        writer.WriteAttributeString("fill", "#000000");
        writer.WriteAttributeString("fill-opacity", "0");
        writer.WriteAttributeString("pointer-events", "all");
        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    private static void WriteClipPathDefinition(
        XmlWriter writer,
        string clipPathId,
        DisplayRectangle rectangle)
    {
        SvgXml.WriteStartSvgElement(writer, "defs");
        SvgXml.WriteStartSvgElement(writer, "clipPath");
        writer.WriteAttributeString("id", clipPathId);
        writer.WriteAttributeString("clipPathUnits", "userSpaceOnUse");
        SvgXml.WriteStartSvgElement(writer, "rect");
        WriteRectangleAttributes(writer, rectangle);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteRectangleAttributes(XmlWriter writer, DisplayRectangle rectangle)
    {
        writer.WriteAttributeString("x", SvgXml.FormatNumber(rectangle.Left));
        writer.WriteAttributeString("y", SvgXml.FormatNumber(rectangle.Top));
        writer.WriteAttributeString("width", SvgXml.FormatNumber(Math.Max(0, rectangle.Width)));
        writer.WriteAttributeString("height", SvgXml.FormatNumber(Math.Max(0, rectangle.Height)));
    }

    private static DisplayRectangle TransformRectangle(DisplayRectangle rectangle, StateFrame frame)
        => new(
            rectangle.Left + frame.TranslateX,
            rectangle.Top + frame.TranslateY,
            rectangle.Width,
            rectangle.Height);

    private static void WriteFillAttributes(XmlWriter writer, DisplayColor color)
    {
        writer.WriteAttributeString("fill", SvgXml.FormatColor(color));
        if (color.Alpha < byte.MaxValue)
            writer.WriteAttributeString("fill-opacity", SvgXml.FormatOpacity(color.Alpha));
    }

    private static void WriteStrokeAttributes(XmlWriter writer, DisplayColor color, float thickness)
    {
        writer.WriteAttributeString("stroke", SvgXml.FormatColor(color));
        if (color.Alpha < byte.MaxValue)
            writer.WriteAttributeString("stroke-opacity", SvgXml.FormatOpacity(color.Alpha));
        writer.WriteAttributeString("stroke-width", SvgXml.FormatNumber(Math.Max(0, thickness)));
        writer.WriteAttributeString("stroke-linecap", "butt");
    }

    private static void WriteFontAttributes(XmlWriter writer, DisplayFont font)
    {
        if (font.Weight is not 0)
            writer.WriteAttributeString("font-weight", font.Weight.ToString(CultureInfo.InvariantCulture));

        var fontStretch = GetFontStretch(font.LetterSpacing);
        if (fontStretch is not null)
            writer.WriteAttributeString("font-stretch", fontStretch);

        switch (font.Style)
        {
            case DisplayFontStyle.Italic:
                writer.WriteAttributeString("font-style", "italic");
                break;
            case DisplayFontStyle.Oblique:
                writer.WriteAttributeString("font-style", "oblique");
                break;
        }
    }

    private static void WriteTextDecorationAttributes(XmlWriter writer, TextDecoration decoration)
    {
        if (decoration is TextDecoration.None)
            return;

        var lines = new List<string>(2);
        if (decoration.HasFlag(TextDecoration.Underline)
            || decoration.HasFlag(TextDecoration.DoubleUnderline))
        {
            lines.Add("underline");
        }

        if (decoration.HasFlag(TextDecoration.StrikeThrough))
            lines.Add("line-through");

        if (lines.Count is 0)
            return;

        var textDecoration = string.Join(" ", lines);
        writer.WriteAttributeString("text-decoration", textDecoration);
        if (decoration.HasFlag(TextDecoration.DoubleUnderline))
        {
            writer.WriteAttributeString(
                "style",
                $"text-decoration-line: {textDecoration}; text-decoration-style: double;");
        }
    }

    private static float GetTextSize(DrawTextCommand text)
        => text.TextStyle.FontSize * text.Dpi / 72.272F;

    private static string? GetFontStretch(ushort width)
        => width switch
        {
            1 => "ultra-condensed",
            2 => "extra-condensed",
            3 => "condensed",
            4 => "semi-condensed",
            5 => "normal",
            6 => "semi-expanded",
            7 => "expanded",
            8 => "extra-expanded",
            9 => "ultra-expanded",
            _ => null,
        };

    private static void CloseFrame(XmlWriter writer, StateFrame frame)
    {
        for (var i = 0; i < frame.OpenElementCount; i++)
            writer.WriteEndElement();
    }

    private string CreateClipPathId()
    {
        _clipPathIndex++;
        return $"papercraft-clip-{_clipPathIndex.ToString(CultureInfo.InvariantCulture)}";
    }

    private sealed class StateFrame
    {
        public StateFrame(float translateX, float translateY)
        {
            TranslateX = translateX;
            TranslateY = translateY;
        }

        public int OpenElementCount { get; set; }

        public float TranslateX { get; private set; }

        public float TranslateY { get; private set; }

        public void Translate(float x, float y)
        {
            TranslateX += x;
            TranslateY += y;
        }
    }
}

internal static class SvgXml
{
    public const string Namespace = "http://www.w3.org/2000/svg";
    public const string XlinkNamespace = "http://www.w3.org/1999/xlink";
    public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
    public const string PapercraftNamespace = "https://x39.solutions/papercraft";

    public static void WriteStartSvgElement(XmlWriter writer, string localName)
        => writer.WriteStartElement(null, localName, Namespace);

    public static void WriteComment(XmlWriter writer, string value)
    {
        var safeValue = value.Replace("--", "- -", StringComparison.Ordinal);
        if (safeValue.EndsWith("-", StringComparison.Ordinal))
            safeValue += " ";
        writer.WriteComment(safeValue);
    }

    public static string FormatNumber(float value)
        => float.IsFinite(value)
            ? value.ToString("G9", CultureInfo.InvariantCulture)
            : "0";

    public static string FormatViewBox(float x, float y, float width, float height)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{FormatNumber(x)} {FormatNumber(y)} {FormatNumber(width)} {FormatNumber(height)}");

    public static string FormatTranslate(float x, float y)
        => string.Create(CultureInfo.InvariantCulture, $"translate({FormatNumber(x)} {FormatNumber(y)})");

    public static string FormatTextTransform(float x, float y, float scale, float rotation)
    {
        var transform = string.Create(CultureInfo.InvariantCulture, $"translate({FormatNumber(x)} {FormatNumber(y)})");
        if (!IsNearlyZero(rotation))
            transform += string.Create(CultureInfo.InvariantCulture, $" rotate({FormatNumber(rotation)})");
        if (!IsNearlyEqual(scale, 1F))
            transform += string.Create(CultureInfo.InvariantCulture, $" scale({FormatNumber(scale)} 1)");
        return transform;
    }

    public static string FormatUrlReference(string id)
        => $"url(#{id})";

    public static string FormatColor(DisplayColor color)
        => string.Create(CultureInfo.InvariantCulture, $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}");

    public static string FormatOpacity(byte alpha)
        => (alpha / 255D).ToString("0.###", CultureInfo.InvariantCulture);

    public static bool IsNearlyZero(float value)
        => Math.Abs(value) < 0.0001F;

    public static bool IsNearlyEqual(float left, float right)
        => Math.Abs(left - right) < 0.0001F;
}

internal static class SvgImageMediaTypeDetector
{
    public static string? Detect(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (StartsWith(bytes, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
            return "image/png";
        if (StartsWith(bytes, [0xFF, 0xD8, 0xFF]))
            return "image/jpeg";
        if (StartsWith(bytes, "GIF87a"u8.ToArray()) || StartsWith(bytes, "GIF89a"u8.ToArray()))
            return "image/gif";
        if (StartsWith(bytes, [0x42, 0x4D]))
            return "image/bmp";
        if (bytes.Length >= 12
            && StartsWith(bytes, "RIFF"u8.ToArray())
            && bytes[8] is 0x57
            && bytes[9] is 0x45
            && bytes[10] is 0x42
            && bytes[11] is 0x50)
        {
            return "image/webp";
        }

        if (LooksLikeSvg(bytes))
            return "image/svg+xml";

        return null;
    }

    private static bool StartsWith(byte[] bytes, byte[] prefix)
    {
        if (bytes.Length < prefix.Length)
            return false;

        for (var i = 0; i < prefix.Length; i++)
        {
            if (bytes[i] != prefix[i])
                return false;
        }

        return true;
    }

    private static bool LooksLikeSvg(byte[] bytes)
    {
        var length = Math.Min(bytes.Length, 512);
        if (length is 0)
            return false;

        var prefix = Encoding.UTF8.GetString(bytes, 0, length)
            .TrimStart('\uFEFF', ' ', '\t', '\r', '\n');
        return prefix.StartsWith("<svg", StringComparison.OrdinalIgnoreCase)
               || (prefix.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)
                   && prefix.Contains("<svg", StringComparison.OrdinalIgnoreCase));
    }
}
