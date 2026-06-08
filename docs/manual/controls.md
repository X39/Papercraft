# Controls

Previous: [Quick reference](quick-reference.md) | [Manual home](index.md) | Next: [Transformers](transformers.md)

## What Is This?

Controls are XML elements that measure, arrange and render document content.
Common controls include [`text`](controls-text.md), [`border`](controls-border.md), [`image`](controls-image.md),
[`line`](controls-line.md), [`pageNumber`](controls-page-number.md), [`table`](controls-table.md) and
[chart controls](controls-chart.md). Optional packages add [barcode controls](controls-barcode.md).

If you only need to check names, attributes and value formats, use the
[Quick reference](quick-reference.md).

Some controls can contain other controls.
For example, a `border` can contain content inside it, while a simple text control renders text directly.

## When Should I Use This?

Use this chapter when you need to choose the right XML element for a visible part of the document.
It should answer questions such as how to add text, how to draw a separator line,
how to build a table and how to show page numbers.

## How Do I Start?

Choose the smallest control that matches the document part:

- Use [`text`](controls-text.md) for words and values.
- Use [`border`](controls-border.md) when content needs a surrounding box, background or border line.
- Use [`line`](controls-line.md) for separators.
- Use [`pageNumber`](controls-page-number.md) for current page and total page count text.
- Use [`table`, `tr`, `td` and `th`](controls-table.md) for rows and columns.
- Use [`chart`, `lineChart`, `barChart` and `pieChart`](controls-chart.md) for compact data visuals.
- Use optional [`qrCode`, `barcode` and barcode alias controls](controls-barcode.md) for QR codes and machine-readable labels.

Use the [Quick reference](quick-reference.md) when you need a compact attribute table.
Read [Control concepts](controls-concepts.md) first if you are unsure whether a control can contain other controls.

## Documented Built-In Controls

| Control | Use it for |
|---------|------------|
| [Control concepts](controls-concepts.md) | How controls, containers and child rules fit together. |
| [`text`](controls-text.md) | Words, labels, headings and values. |
| [`border`](controls-border.md) | Boxes, backgrounds and border lines around content. |
| [`line`](controls-line.md) | Separators and simple rules. |
| [`image`](controls-image.md) | Raster images loaded by the application. |
| [`pageNumber`](controls-page-number.md) | Current page and total page count text. |
| [`table`, `tr`, `td`, `th`](controls-table.md) | Rows, columns and table cells. |
| [`chart`, `lineChart`, `barChart`, `pieChart`, `data`](controls-chart.md) | Line, bar and pie chart visuals. |

## Optional Control Packages

| Control package | Use it for |
|-----------------|------------|
| [`X39.Solutions.Papercraft.Controls.QrCode`](controls-barcode.md) | `qrCode` controls backed by `Net.Codecrete.QrCodeGenerator`. |
| [`X39.Solutions.Papercraft.Controls.ZXing`](controls-barcode.md) | Generic `barcode` plus common aliases such as `code128`, `ean13`, `dataMatrix`, `pdf417` and `aztec`. |

Previous: [Quick reference](quick-reference.md) | [Manual home](index.md) | Next: [Transformers](transformers.md)
