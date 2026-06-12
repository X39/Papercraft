using System.ComponentModel;
using SkiaSharp;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;

/// <summary>
/// Cache for SkPaint.
/// </summary>
public sealed class SkPaintCache : IDisposable
{
    // ReSharper disable NotAccessedPositionalProperty.Local -- Disabled as this is a key-only record
    private readonly record struct StrokePaintKey(Color Color, float Thickness);

    private readonly record struct TextPaintKey(TextStyle TextStyle, float Dpi);
    // ReSharper restore NotAccessedPositionalProperty.Local

    private readonly Dictionary<StrokePaintKey, SKPaint>   _strokePaints     = new();
    private readonly ReaderWriterLockSlim                  _strokePaintsLock = new();
    private readonly Dictionary<TextPaintKey, SkTextPaint> _textPaints       = new();
    private readonly ReaderWriterLockSlim                  _textPaintsLock   = new();
    private readonly Dictionary<Color, SKPaint>            _fillPaintKey     = new();
    private readonly ReaderWriterLockSlim                  _fillPaintKeyLock = new();

    /// <inheritdoc />
    public void Dispose()
    {
        WithWriteLock(
            _strokePaintsLock,
            () =>
            {
                foreach (var skPaint in _strokePaints.Values)
                    skPaint.Dispose();
            });
        WithWriteLock(
            _textPaintsLock,
            () =>
            {
                foreach (var textPaint in _textPaints.Values)
                    textPaint.Dispose();
            });
    }

    /// <summary>
    /// Method to receive the <see cref="SKPaint"/> for a stroke.
    /// </summary>
    /// <remarks>
    /// Gets a <see cref="SKPaint"/> from the cache or adds it if it does not exist.
    /// </remarks>
    /// <param name="color">The color of the paint.</param>
    /// <param name="thickness">The thickness of the color</param>
    /// <returns>A <see cref="SKPaint"/> with the given parameters.</returns>
    public SKPaint Get(Color color, float thickness)
    {
        var key = new StrokePaintKey(color, thickness);
        return WithUpgradeableReadLock(
            _strokePaintsLock,
            () =>
            {
                if (_strokePaints.TryGetValue(key, out var skPaint1))
                    return skPaint1;
                return WithWriteLock(
                    _strokePaintsLock,
                    () =>
                    {
                        if (_strokePaints.TryGetValue(key, out var skPaint2))
                            return skPaint2;
                        return _strokePaints[key] = new SKPaint
                        {
                            Color       = color.ToSkColor(),
                            StrokeWidth = thickness,
                            IsStroke    = true,
                        };
                    });
            });
    }

    /// <summary>
    /// Method to receive the <see cref="SKPaint"/> for a filled color.
    /// </summary>
    /// <remarks>
    /// Gets a <see cref="SKPaint"/> from the cache or adds it if it does not exist.
    /// </remarks>
    /// <param name="color">The color of the paint.</param>
    /// <returns>A <see cref="SKPaint"/> with the given parameters.</returns>
    public SKPaint Get(Color color)
    {
        var key = color;
        return WithUpgradeableReadLock(
            _fillPaintKeyLock,
            () =>
            {
                if (_fillPaintKey.TryGetValue(key, out var skPaint1))
                    return skPaint1;
                return WithWriteLock(
                    _fillPaintKeyLock,
                    () =>
                    {
                        if (_fillPaintKey.TryGetValue(key, out var skPaint2))
                            return skPaint2;
                        return _fillPaintKey[key] = new SKPaint
                        {
                            Color    = color.ToSkColor(),
                            IsStroke = false,
                        };
                    });
            });
    }

    /// <summary>
    /// Returns a <see cref="SKPaint"/> for the given <see cref="TextStyle"/>.
    /// </summary>
    /// <param name="textStyle">The <see cref="TextStyle"/> to get the <see cref="SKPaint"/> for.</param>
    /// <param name="dpi">The DPI to use.</param>
    /// <returns>A <see cref="SKPaint"/> for the given <see cref="TextStyle"/>.</returns>
    /// <exception cref="InvalidEnumArgumentException">Thrown when the <see cref="EFontStyle"/> is not supported, indicating a programming error, not a user one.</exception>
    public SKPaint Get(TextStyle textStyle, float dpi)
        => GetText(textStyle, dpi).Paint;

    internal SkTextPaint GetText(TextStyle textStyle, float dpi)
    {
        var key = new TextPaintKey(textStyle, dpi);
        return WithUpgradeableReadLock(
            _textPaintsLock,
            () =>
            {
                if (_textPaints.TryGetValue(key, out var textPaint))
                    return textPaint;
                return WithWriteLock(
                    _textPaintsLock,
                    () =>
                    {
                        if (_textPaints.TryGetValue(key, out var textPaint2))
                            return textPaint2;
                        return _textPaints[key] = CreateTextPaint(textStyle, dpi);
                    });
            });
    }

    private static SkTextPaint CreateTextPaint(TextStyle textStyle, float dpi)
    {
        var typeface = SKTypeface.FromFamilyName(
            textStyle.FontFamily.Family,
            textStyle.FontFamily.Weight,
            textStyle.FontFamily.LetterSpacing,
            textStyle.FontFamily.Style switch
            {
                EFontStyle.Upright => SKFontStyleSlant.Upright,
                EFontStyle.Italic  => SKFontStyleSlant.Italic,
                EFontStyle.Oblique => SKFontStyleSlant.Oblique,
                _ => throw new InvalidEnumArgumentException(
                    nameof(textStyle.FontFamily.Style),
                    (int) textStyle.FontFamily.Style,
                    typeof(EFontStyle)),
            });

        return new SkTextPaint(
            new SKPaint
            {
                Color       = textStyle.Foreground.ToSkColor(),
                StrokeWidth = textStyle.StrokeThickness,
                StrokeCap   = SKStrokeCap.Round,
            },
            new SKFont(
                typeface,
                textStyle.FontSize * dpi / 72.272F,
                textStyle.Scale,
                textStyle.Rotation),
            typeface);
    }

    private static T WithUpgradeableReadLock<T>(ReaderWriterLockSlim readerWriterLockSlim, Func<T> action)
    {
        readerWriterLockSlim.EnterUpgradeableReadLock();
        try
        {
            return action();
        }
        finally
        {
            readerWriterLockSlim.ExitUpgradeableReadLock();
        }
    }

    private static void WithWriteLock(ReaderWriterLockSlim readerWriterLockSlim, Action action)
    {
        readerWriterLockSlim.EnterWriteLock();
        try
        {
            action();
        }
        finally
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    private static T WithWriteLock<T>(ReaderWriterLockSlim readerWriterLockSlim, Func<T> action)
    {
        readerWriterLockSlim.EnterWriteLock();
        try
        {
            return action();
        }
        finally
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }
}

internal sealed class SkTextPaint : IDisposable
{
    private readonly SKTypeface? _typeface;

    public SkTextPaint(SKPaint paint, SKFont font, SKTypeface? typeface)
    {
        Paint     = paint;
        Font      = font;
        _typeface = typeface;
    }

    public SKPaint Paint { get; }

    public SKFont Font { get; }

    public void Dispose()
    {
        Paint.Dispose();
        Font.Dispose();
        _typeface?.Dispose();
    }
}
