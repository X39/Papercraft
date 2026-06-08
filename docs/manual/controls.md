# Controls

Previous: [Quick reference](quick-reference.md) | [Manual home](index.md) | Next: [Transformers](transformers.md)

## What Is This?

Controls are XML elements that measure, arrange and render document content.
`AddPapercraftCore()`, `AddPapercraft()` and the legacy `AddPdfTemplateService()` register the built-in
Papercraft Core controls listed on this page.
Optional control packages add QR code and ZXing barcode controls only after the application installs and
registers those packages.

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
- Use `paragraph`, `span`, `br` and `hyperlink` for rich text fragments and link-style text.
- Use [`border`](controls-border.md) when content needs a surrounding box, background or border line.
- Use `block`, `spacer`, `pageBreak` and `columns` for flow grouping, empty space, forced body breaks and multi-column flow.
- Use [`line`](controls-line.md) for separators.
- Use [`pageNumber`](controls-page-number.md) for current page and total page count text.
- Use [`table`, `tr`, `td` and `th`](controls-table.md) for rows and columns.
- Use `ul`, `ol`, `li`, `checkbox` and `signature` for lists, checklist marks and signature lines.
- Use [`chart`, `lineChart`, `barChart` and `pieChart`](controls-chart.md) for compact data visuals.
- Use optional [`qrCode`](controls-qrcode.md) for dedicated QR codes.
- Use optional [`barcode` and ZXing alias controls](controls-zxing.md) for common 1D and 2D barcode formats.

Use the [Quick reference](quick-reference.md) when you need a compact attribute table.
Read [Control concepts](controls-concepts.md) first if you are unsure whether a control can contain other controls.

## Built-In Core Controls

These XML elements are registered by Papercraft Core.

| Area | Controls | Use | Reference |
|------|----------|-----|-----------|
| Text | `text` | Plain text, labels, headings and values. | [Text control](controls-text.md) |
| Rich text | `paragraph`, `span`, `br`, `hyperlink` | Inline text fragments, explicit line breaks and link-style text. | [Quick reference](quick-reference.md#additional-built-in-controls) |
| Containers and flow | `border`, `block`, `spacer`, `pageBreak`, `columns` | Boxes, backgrounds, grouping, spacing, body page breaks and multi-column flow. | [Border control](controls-border.md), [Quick reference](quick-reference.md#additional-built-in-controls) |
| Media and rules | `image`, `line` | Raster images and horizontal or vertical separator rules. | [Image control](controls-image.md), [Line control](controls-line.md) |
| Page text | `pageNumber` | Current page number, total page count or both. | [Page number control](controls-page-number.md) |
| Tables | `table`, `th`, `tr`, `td` | Table layout, repeated headers, rows and cells. | [Table control](controls-table.md) |
| Lists and marks | `ul`, `ol`, `li`, `checkbox`, `signature` | Bulleted lists, numbered lists, checklist marks and signature lines. | [Quick reference](quick-reference.md#additional-built-in-controls) |
| Charts | `chart`, `lineChart`, `barChart`, `pieChart`, `data` | Line, bar and pie chart visuals with XML data points. | [Chart controls](controls-chart.md) |

Control names are matched case-insensitively by the runtime, but this manual uses the XML names shown above.
Elements without an XML namespace are treated as built-in controls.

## Optional Control Packages

The optional control packages are separate NuGet packages and separate manual pages.
They depend on `X39.Solutions.Papercraft.Core`, but they are not registered automatically by Core,
the Papercraft facade or the PdfTemplate compatibility package.

| Package | Controls | Use | Reference |
|---------|----------|-----|-----------|
| `X39.Solutions.Papercraft.Controls.QrCode` | `qrCode` | Dedicated QR codes backed by `Net.Codecrete.QrCodeGenerator`. | [QR code package](controls-qrcode.md) |
| `X39.Solutions.Papercraft.Controls.ZXing` | `barcode`, `code128`, `gs1-128`, `code39`, `code93`, `codabar`, `ean13`, `ean8`, `upcA`, `upcE`, `itf`, `dataMatrix`, `pdf417`, `aztec` | Generic and alias barcode controls backed by `ZXing.Net`. | [ZXing barcode package](controls-zxing.md) |

The older [barcode overview](controls-barcode.md) remains as a compatibility entry point and points to the
package-specific pages.

Previous: [Quick reference](quick-reference.md) | [Manual home](index.md) | Next: [Transformers](transformers.md)
