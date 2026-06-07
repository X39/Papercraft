# Rich Text Paragraph Plan

## Goal

Add mixed inline text formatting so template authors can style fragments inside one wrapping paragraph.

Plain `text` remains the right control for single-style text. `paragraph` is only valuable if it supports inline children.

## Proposed XML

```xml
<paragraph fontsize="10">
    <span>Total: </span>
    <span weight="bold">@Total</span>
    <span> due by </span>
    <span foreground="red">@DueDate</span>
    <br/>
    <span style="italic">Thank you.</span>
</paragraph>
```

## Scope

- Add `ParagraphControl`.
- Add `SpanControl`.
- Add `BrControl` registered as `br`.
- Support inline `span` and `br` children only.
- Support paragraph-level inherited text style.
- Support span-level overrides for foreground, font size, weight, style, line height, letter spacing, and font family.

## Implementation Steps

1. Add a small immutable rich text model: text run plus `TextStyle`, and hard line break run.
2. Add `ParagraphControl` as a content control that only accepts `SpanControl` and `BrControl`.
3. Add `SpanControl` with content text and optional style override parameters.
4. Implement paragraph style inheritance by combining paragraph style with span overrides at layout time.
5. Extend text service or add `IRichTextService` to measure and draw styled runs.
6. Implement line wrapping over runs; split at whitespace while preserving run styles.
7. Draw each run at its computed line x/y position.
8. Register controls.

## Tests

- paragraph rejects non-inline child controls.
- span content is rendered.
- multiple spans render on the same line when width allows.
- wrapping preserves run order and style.
- `br` forces a new line.
- span style overrides paragraph style.
- current `text` behavior remains unchanged.

## Documentation

- Add `docs/manual/controls-rich-text.md`.
- Update text docs to direct mixed-style content to `paragraph`.
- Update quick reference.

## Risks

- This is the deepest text change. The current `ITextService` accepts one `TextStyle` and one string. Rich text requires a real run layout algorithm.
- Skia measurement can handle per-run width, but wrapping across runs must be implemented carefully.
- Inline hyperlinks will likely build on this model.

## Recommended Staging

1. First milestone: `paragraph`, `span`, and `br` with simple left-aligned wrapping.
2. Second milestone: alignment, indentation, tabs, and inline hyperlink annotations.
3. Third milestone: richer typographic behavior if needed.

