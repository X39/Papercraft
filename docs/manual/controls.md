# Controls

Previous: [Layout fundamentals](layout-fundamentals.md) | [Manual home](index.md) | Next: [Template language](template-language.md)

Status: planned.

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
- Use `table`, `tr`, `td` and `th` for rows and columns.

The first complete controls reference will verify each attribute name against source or tests.

## Planned Work

- Decide whether reference-heavy controls should be split into one file per control.
- Add purpose, allowed children, attributes, examples and common mistakes for each built-in control.
- Add generated samples for each visual control.
- Link shared layout attributes back to [Layout fundamentals](layout-fundamentals.md).

Previous: [Layout fundamentals](layout-fundamentals.md) | [Manual home](index.md) | Next: [Template language](template-language.md)
