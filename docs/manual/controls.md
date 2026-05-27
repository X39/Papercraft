# Controls

Previous: [Styles](styles.md) | [Manual home](index.md) | Next: [Transformers](transformers.md)

Status: started. [Control concepts](controls-concepts.md) and the focused [Text control](controls-text.md), [Border control](controls-border.md),
[Image control](controls-image.md), [Line control](controls-line.md), [Page number control](controls-page-number.md),
[Table control](controls-table.md) and [Chart controls](controls-chart.md) pages now have verified examples.

## What Is This?

Controls are XML elements that measure, arrange and render document content.
Common controls include `text`, `border`, `image`, `line`, `pageNumber`, `table` and chart controls.

Some controls can contain other controls.
For example, a `border` can contain content inside it, while a simple text control renders text directly.

## When Should I Use This?

Use this chapter when you need to choose the right XML element for a visible part of the document.
It should answer questions such as how to add text, how to draw a separator line,
how to build a table and how to show page numbers.

## How Do I Start?

Choose the smallest control that matches the document part:

- Use `text` for words and values.
- Use `border` when content needs a surrounding box, background or border line.
- Use `line` for separators.
- Use `pageNumber` for current page and total page count text.
- Use `table`, `tr`, `td` and `th` for rows and columns.
- Use `chart`, `lineChart`, `barChart` and `pieChart` for compact data visuals.

Read [Control concepts](controls-concepts.md) first if you are unsure whether a control can contain other controls.
The focused reference pages verify attribute names against source or tests.

## Built-In Controls

| Control | Use it for | Status |
|---------|------------|--------|
| [Control concepts](controls-concepts.md) | How controls, containers and child rules fit together. | Started with verified source checks and reused generated samples. |
| [`text`](controls-text.md) | Words, labels, headings and values. | Started with verified examples. |
| [`border`](controls-border.md) | Boxes, backgrounds and border lines around content. | Started with verified examples. |
| [`line`](controls-line.md) | Separators and simple rules. | Started with verified examples. |
| [`image`](controls-image.md) | Raster images loaded by the application. | Started with a verified generated sample. |
| [`pageNumber`](controls-page-number.md) | Current page and total page count text. | Started with a verified footer example. |
| [`table`, `tr`, `td`, `th`](controls-table.md) | Rows, columns and table cells. | Started with verified table examples. |
| [`chart`, `lineChart`, `barChart`, `pieChart`, `data`](controls-chart.md) | Line, bar and pie chart visuals. | Started with verified chart examples. |

## Planned Work

- Keep this page as the controls landing page, then split detailed built-in control references into focused pages as they are written.
- Use the reference split described in the [documentation style guide](style-guide.md).
- Tighten purpose, allowed children, attributes, examples and common mistakes as each focused page matures.
- Keep generated samples current as visual control pages expand.
- Link shared layout attributes back to [Layout fundamentals](layout-fundamentals.md).
- Link repeated shared attributes back to [Styles](styles.md).

Previous: [Styles](styles.md) | [Manual home](index.md) | Next: [Transformers](transformers.md)
