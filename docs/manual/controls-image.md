# Image Control

[Controls](controls.md) | [Manual home](index.md)

## What Is This?

The `image` control renders a raster image in the document.
It is a leaf control: it draws the image selected by its `source` attribute, but it does not contain child controls.

The source is resolved by the application.
With the default resolver, the source must be base64 image data or a `data:image/...;base64,...` value.

## When Should I Use This?

Use `image` for logos, signatures, product pictures, icons or other bitmap artwork that belongs in the generated PDF.

Use normal controls such as `text`, `line`, `border` and `table` for document structure.
Do not use an image for text that should come from template data or for layout spacing.

## How Do I Start?

Ask which image source value the application supplies for your template, then put that value in `source`.
This sample uses a `LogoImage` template-data value supplied by the documentation test.

```xml
<?xml version="1.0" encoding="utf-8"?>
<template>
    <body>
        <image
            source="@LogoImage"
            width="32mm"
            height="18mm"
            horizontalAlignment="left"
            verticalAlignment="top"/>
    </body>
</template>
```

{% include sample-preview.html sample="image-from-template-data" alt="Rendered image sample" %}

## Add An Image From Template Data

Use `source` for the image reference.
The value may be written directly in XML, but templates usually read it from data so the application can choose
the right logo or picture for each document.

```xml
<image source="@LogoImage" width="32mm" height="18mm"/>
```

{% include sample-preview.html sample="image-from-template-data" alt="Rendered image sample" %}

The default resolver accepts base64 data and `data:image/...;base64,...` values.
It does not load arbitrary file paths or internet URLs.
If a template should use a file name, database key, storage key or URL, the application must provide that behavior.

For troubleshooting missing images, see [Troubleshooting](troubleshooting.md).

## Size The Image

Use `width` and `height` to control the rendered image size.
Both attributes use the length formats from [Layout fundamentals](layout-fundamentals.md), such as `20mm`, `2cm`,
`72pt`, `25%` or `auto`.

When both `width` and `height` are set, the image is drawn into that rectangle.
When one side is `auto`, `ImageControl` calculates that side from the bitmap aspect ratio.
When both are `auto`, the bitmap's own pixel size is used.

Use both dimensions when the image must occupy a fixed slot, one dimension with `auto` when the image should keep
its original shape, or a percentage width when the image should scale with the available space:

```xml
<image source="@LogoImage" width="32mm" height="18mm"/>
<image source="@LogoImage" width="45mm" height="auto"/>
<image source="@LogoImage" width="100%" height="auto"/>
```

{% include sample-preview.html sample="image-sizing-options" alt="Rendered image sizing sample" %}

## Supported Attributes

| Attribute | Use it for | Values |
|-----------|------------|--------|
| `source` | Image data or an application-resolved image reference. | String, often a template-data value such as `@LogoImage`. |
| `width` | Rendered image width. | Any supported length, default `auto`. |
| `height` | Rendered image height. | Any supported length, default `auto`. |

The `image` control also supports the shared `margin`, `padding`, `clip`, `horizontalAlignment`
and `verticalAlignment` attributes described in [Layout fundamentals](layout-fundamentals.md).

## Allowed Children

`image` does not allow child controls.
Use a surrounding `border` when the image needs a background, frame or padding that should be visually separate
from the image itself.

## Common Mistakes

- Using a local file path in `source` when the application only supports the default base64 resolver.
- Forgetting that image source values are resolved by the application.
- Setting both `width` and `height` to values that do not match the image's shape, which can stretch the bitmap.
- Putting child controls inside `image`. Use a surrounding `border` or table cell instead.

[Controls](controls.md) | [Manual home](index.md)
