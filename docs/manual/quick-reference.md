# Quick Reference

Previous: [Styles](styles.md) | [Manual home](index.md) | Next: [Controls](controls.md)

Use this page when you already know the basics and need to check the XML element, attribute name or value format.
For explanations and rendered examples, follow the linked control pages.

## Minimal Shape

```xml
<?xml version="1.0" encoding="utf-8"?>
<template>
    <template.style>
        <text fontsize="9" foreground="#0f172a"/>
        <td padding="1mm"/>
    </template.style>
    <header>
        <text>Repeated page header</text>
    </header>
    <body>
        <text>Hello @CustomerName</text>
    </body>
    <footer>
        <pageNumber mode="CurrentTotal" prefix="Page " delimiter=" of "/>
    </footer>
</template>
```

## Page Sections

| Element | Use it for | Notes |
|---------|------------|-------|
| `template` | Root element. | Contains optional styles, page sections and areas. |
| `template.style` | Shared control attributes. | Put it before the controls it should affect. |
| `body` | Main flowing content. | Continues on later pages when content is taller than one page. |
| `header` | Repeated top content. | Repeated on every page and reserves body height. |
| `footer` | Repeated bottom content. | Repeated on every page and reserves body height. |
| `background` | Repeated page background. | Does not reserve body height. |
| `foreground` | Repeated page overlay. | Drawn above normal content and does not reserve body height. |
| `areas` | Fixed-position page areas. | Use when coordinates matter more than flow. |

## Control Picker

| Need | Use | Common attributes |
|------|-----|-------------------|
| Text, labels, values. | [`text`](controls-text.md) | `fontsize`, `foreground`, `weight`, `style`, `fontfamily`, `margin`, `padding` |
| Box, background, callout, one-sided rule. | [`border`](controls-border.md) | `thickness`, `color`, `background`, `padding`, `margin`, `verticalAlignment` |
| Image from template data or a resolver. | [`image`](controls-image.md) | `source`, `width`, `height` |
| Horizontal or vertical separator. | [`line`](controls-line.md) | `length`, `thickness`, `orientation`, `color` |
| Page number text. | [`pageNumber`](controls-page-number.md) | `mode`, `prefix`, `delimiter`, `suffix`, plus text styling attributes |
| Rows and columns. | [`table`](controls-table.md), `th`, `tr`, `td` | `td width`, `td columnspan`, `padding`, `background` through nested `border` |
| Line, bar or pie chart. | [`chart`](controls-chart.md), `lineChart`, `barChart`, `pieChart`, `data` | `width`, `height`, `title`, chart-specific attributes |
| Rich text, links and line breaks. | `paragraph`, `span`, `br`, `hyperlink` | text styling attributes, `href`, `underline` |
| Flow grouping, space and page breaks. | `block`, `spacer`, `pageBreak`, `columns` | `background`, `minHeight`, `pageBreakBefore`, `width`, `height`, `count`, `gap` |
| Lists, checkboxes and signatures. | `ul`, `ol`, `li`, `checkbox`, `signature` | `marker`, `start`, `checked`, `size`, `label`, signature line attributes |
| QR codes. | Optional [`qrCode`](controls-qrcode.md) | `value`, `size`, `quietZone`, `errorCorrection`, colors |
| 1D and 2D barcodes. | Optional [`barcode` and aliases](controls-zxing.md) | `value`, `format`, `width`, `height`, `quietZone`, `gs1Format`, colors |

## Shared Control Attributes

Most visible controls inherit these attributes.

| Attribute | Values | Use |
|-----------|--------|-----|
| `margin` | [Thickness](#value-formats), such as `2mm` or `0 0 2mm 0`. | Space outside the control. |
| `padding` | [Thickness](#value-formats), such as `1mm`. | Space inside the control before content is drawn. |
| `clip` | `true`, `false`. | Keep drawing inside the arranged control bounds. Default is `true`. |
| `horizontalAlignment` | `Left`, `Center`, `Right`, `Stretch`. | Place the arranged control inside available width. |
| `verticalAlignment` | `Top`, `Center`, `Bottom`, `Stretch`. | Place the arranged control inside available height. |

Attribute names are matched case-insensitively.
The manual uses lower-case or lower-camel-case names for readability.

## Text And Page Numbers

| Control | Attribute | Values |
|---------|-----------|--------|
| `text`, `pageNumber` | `foreground` | Supported color. |
| `text`, `pageNumber` | `fontsize` | Number, in points. Default `12`. |
| `text`, `pageNumber` | `lineheight` | Number relative to font size. Default `1`. |
| `text`, `pageNumber` | `scale` | Number. Default `1`. |
| `text`, `pageNumber` | `rotation` | Number, in degrees. Default `0`. |
| `text`, `pageNumber` | `strokethickness` | Number. Default `1`. |
| `text`, `pageNumber` | `letterspacing` | Number. |
| `text`, `pageNumber` | `weight` | Number or names such as `normal`, `semiBold`, `bold`. |
| `text`, `pageNumber` | `style` | `normal`, `upright`, `italic`, `oblique`. |
| `text`, `pageNumber` | `fontfamily` | Font family name available to the renderer. |
| `text` | `text` | Text as an attribute. Prefer element content for normal use. |
| `pageNumber` | `mode` | `Current`, `Total`, `CurrentTotal`, `TotalCurrent`. |
| `pageNumber` | `prefix`, `delimiter`, `suffix` | Text around or between page numbers. |

```xml
<text fontsize="14" weight="bold" foreground="#1d4ed8">Order @OrderNumber</text>
<pageNumber mode="CurrentTotal" prefix="Page " delimiter=" of " fontsize="8"/>
```

## Borders, Images And Lines

| Control | Attribute | Values |
|---------|-----------|--------|
| `border` | `thickness` | [Thickness](#value-formats). Use `0 0 0 1pt` for a bottom rule. |
| `border` | `color` | Border color. |
| `border` | `background` | Fill color behind children. |
| `image` | `source` | Resource string resolved by `IResourceResolver`; default resolver accepts base64 images. |
| `image` | `width`, `height` | [Length](#value-formats). Use `auto` to preserve intrinsic size or aspect ratio. |
| `line` | `length` | [Length](#value-formats), often `100%`. |
| `line` | `thickness` | [Length](#value-formats), often `1pt`. |
| `line` | `orientation` | `Horizontal`, `Vertical`. |
| `line` | `color` | Line color. |

```xml
<border thickness="1pt" color="#94a3b8" background="#f8fafc" padding="2mm" verticalAlignment="top">
    <text>Content in a visible box.</text>
</border>
<line length="100%" thickness="1pt" color="#cbd5e1"/>
<image source="@LogoImage" width="32mm" height="auto"/>
```

## Tables

| Element | Parent | Children | Attributes |
|---------|--------|----------|------------|
| `table` | `body`, `border`, `td`, other containers. | `th`, `tr`. | Shared attributes. |
| `th` | `table`. | `td`. | Shared attributes; repeats when the table spans pages. |
| `tr` | `table`. | `td`. | Shared attributes. |
| `td` | `th`, `tr`. | Any normal control. | `width`, `columnspan`, shared attributes. |

`td width` accepts fixed lengths, `auto` or star parts such as `1*` and `2*`.
Rows are kept whole; a large row moves to the next page instead of splitting.

```xml
<table>
    <th>
        <td width="2*"><text weight="bold">Item</text></td>
        <td width="25mm"><text weight="bold" horizontalAlignment="right">Total</text></td>
    </th>
    <tr>
        <td><text>@ItemName</text></td>
        <td><text horizontalAlignment="right">@ItemTotal</text></td>
    </tr>
</table>
```

## Additional Built-In Controls

These controls are registered by the current Papercraft core service setup, but some do not yet have focused manual
pages.

| Control | Use | Key attributes and children |
|---------|-----|-----------------------------|
| `block` | Group stacked controls and optional background/page-break behavior. | `background`, `minHeight`, `pageBreakBefore`, `pageBreakAfter`, `keepTogether`, shared attributes; contains normal controls. `keepTogether` is parsed as a future pagination hint. |
| `spacer` | Reserve empty space without drawing. | `width`, `height`, shared alignment attributes. |
| `pageBreak` | Advance body flow to the next page when not already at a page boundary. | Shared attributes. |
| `columns` | Flow whole child controls through multiple columns. | `count`, `gap`, `balance`, `ruleThickness` or `rule-thickness`, `ruleColor` or `rule-color`; `balance` is parsed as a future layout hint. |
| `paragraph` | Rich text made from inline fragments. | Text styling attributes; contains `span` and `br`. |
| `span` | Inline text fragment inside `paragraph`. | `text` or content, plus text styling overrides. |
| `br` | Explicit line break inside `paragraph`. | No control-specific attributes. |
| `hyperlink` | Visual hyperlink-style text. | `href`, `text` or content, `underline`, plus text styling attributes. |
| `ul`, `ol`, `li` | Unordered and ordered lists. | `ul marker`; `ol start`, `markerFormat`; `indent`, `markerWidth`, `itemSpacing`; list children must be `li`. |
| `checkbox` | Checkbox mark with plain label or child content. | `checked`, `size`, `label` or content, `gap`, `strokeColor`, `fill`, `checkColor`, `strokeThickness`. |
| `signature` | Signature line with optional helper text. | `height`, `lineWidth`, `lineThickness`, `lineColor`, `label`, `subtext`, `textPlacement`, plus text styling attributes. |

## Charts

Use `chart` as a wrapper when you want to stack one or more chart controls.
Each chart control contains `data` children.

| Control | Attributes |
|---------|------------|
| `chart` | Shared attributes. Contains `lineChart`, `barChart` or `pieChart`. |
| `lineChart`, `barChart`, `pieChart` | `width`, `height`, `title`, `show-grid`, `grid-color`, `axis-color`, `show-x-axis`, `show-y-axis`, `x-axis-label`, `y-axis-label`, `show-data-labels`. |
| `lineChart` | `line-thickness`, `line-color`, `show-points`, `point-size`. |
| `barChart` | `orientation`, `bar-width`, `bar-spacing`, `bar-color`. |
| `pieChart` | `start-angle`, `inner-radius`, `show-percentages`, `show-labels`, `pie-label-position`. |
| `data` | `x`, `y`, `x-label`, `y-label`, `label`, `color`. |

`x-axis-label` and `y-axis-label` draw visible axis labels on line and bar charts.
Explicit `data label`, `x-label` and `y-label` draw visible line or bar data labels.
Numeric `y` value labels render when `show-data-labels="true"`.

```xml
<chart>
    <barChart title="Revenue" height="55mm" bar-color="#2563eb">
        <data x="0" y="12" label="Jan"/>
        <data x="1" y="19" label="Feb"/>
    </barChart>
</chart>
```

## Optional Control Packages

QR and barcode controls require optional package registration.
`X39.Solutions.Papercraft.Controls.QrCode` adds [`qrCode`](controls-qrcode.md).
`X39.Solutions.Papercraft.Controls.ZXing` adds [`barcode` and alias elements](controls-zxing.md).

| Control | Attributes |
|---------|------------|
| `qrCode` | `value`, `size`, `foreground`, `background`, `quietZone`, `errorCorrection`. Content can supply the value. |
| `barcode` | `format`, `value`, `width`, `height`, `foreground`, `background`, `quietZone`, `gs1Format`. Content can supply the value. |
| `code128`, `gs1-128`, `code39`, `code93`, `codabar`, `ean13`, `ean8`, `upcA`, `upcE`, `itf`, `dataMatrix`, `pdf417`, `aztec` | Same sizing, color, quiet-zone and value attributes as `barcode`; format comes from the element name. |

```xml
<qrCode value="https://example.test/order/123" size="24mm" quietZone="4"/>
<barcode format="Code128" value="ABC123" width="42mm" height="12mm"/>
<dataMatrix width="22mm" height="22mm">ABC123</dataMatrix>
```

## Template Data

| Syntax | Use |
|--------|-----|
| `@CustomerName` | Insert a variable into text or an attribute. |
| `@format(Total)` | Insert the result of a function supplied by the application. |
| `source="@LogoImage"` | Use a variable as an attribute value. |
| `@if HasDiscount { ... }` | Include XML conditionally. |
| `@foreach Line in Lines { ... }` | Repeat XML for each item in a supplied collection. |

```xml
<text>Hello @CustomerName</text>
<text>Total: @format(Total)</text>
<border background="@StatusColor" padding="1mm">
    <text>@StatusText</text>
</border>
```

## Template Language

| Transformer | Pattern | Use |
|-------------|---------|-----|
| `@if` | `@if Condition { ... } @else { ... }` | Optional content and fallback content. |
| `@switch` | `@switch Value { @case "paid" { ... } @default { ... } }` | Several alternatives. |
| `@foreach` | `@foreach Item in Items { ... }` | Repeat over application data. |
| `@foreach` | `@foreach Item in Items with Index { ... }` | Repeat with a zero-based counter. |
| `@for` | `@for Step from 1 to 4 { ... }` | Repeat a numeric range; end is excluded. |
| `@var` | `@var Label = "Total" { ... }` | Temporary values inside a block. |
| `@alternate` | `@alternate on RowColor with ["#fff", "#f1f5f9"] { ... }` | Alternating repeated values. |

In transformer lines, variable names are written without `@`.
Inside normal text and attributes, use `@VariableName`.

## Value Formats

| Type | Accepted values | Examples |
|------|-----------------|----------|
| Length | Number, `px`, `pt`, `mm`, `cm`, `in`, `%`, `auto`. | `12`, `1pt`, `5mm`, `50%`, `auto` |
| Thickness | One, two or four lengths. | `2mm`, `3mm 1mm`, `0 0 1pt 0` |
| Color | `#RGB`, `#RGBA`, `#RRGGBB`, `#RRGGBBAA`, named colors. | `#f00`, `#ff000080`, `red`, `transparent` |
| Orientation | `Horizontal`, `Vertical`. | `orientation="Vertical"` |
| Alignment | `Left`, `Center`, `Right`, `Stretch`, `Top`, `Bottom`. | `horizontalAlignment="right"` |
| Table width | Length, `auto`, star parts. | `30mm`, `auto`, `1*`, `2*` |
| QR error correction | `L`, `M`, `Q`, `H`, or `Low`, `Medium`, `Quartile`, `High`. | `errorCorrection="Q"` |
| Barcode format | `Aztec`, `Codabar`, `Code39`, `Code93`, `Code128`, `GS1-128`, `DataMatrix`, `EAN8`, `EAN13`, `ITF`, `PDF417`, `QRCode`, `UPCA`, `UPCE`. | `format="Code128"` |

Previous: [Styles](styles.md) | [Manual home](index.md) | Next: [Controls](controls.md)
