# Spacer And PageBreak Controls Plan

## Goal

Add explicit flow controls for vertical whitespace and forced page advancement.

These remove common template hacks such as empty `text` nodes or oversized invisible borders.

## Proposed XML

```xml
<spacer height="8mm"/>
<pageBreak/>
```

## Scope

- Add `SpacerControl`.
- Add `PageBreakControl`.
- Register both as built-in controls.
- Support body flow first. Header, footer, background, foreground, and fixed areas may parse them, but documentation should discourage `pageBreak` there.

## Attributes

`spacer`:

- `height`: length; default `0`.
- `width`: length; default `100%`.
- shared margin/padding/alignment attributes.

`pageBreak`:

- no attributes initially.

## Implementation Steps

1. Add `SpacerControl` deriving from `AlignableControl`.
2. Make `SpacerControl` measure and arrange to requested width/height and render nothing.
3. Add `PageBreakControl` deriving from `AlignableControl`.
4. During render, make `PageBreakControl` return an additional height equal to the remaining page height.
5. Use `IDeferredCanvas.GetRemainingPageHeight` if available; if it is an extension method, call the existing helper used by `TableControl`.
6. Register both controls.

## Tests

- `spacer` measures to configured height.
- `spacer` renders no draw calls.
- `spacer` works with margin and padding.
- `pageBreak` returns remaining page height when rendered mid-page.
- a document with `text`, `pageBreak`, `text` produces two pages in bitmap generation.

## Documentation

- Add `docs/manual/controls-flow.md` or a dedicated `controls-spacer-page-break.md`.
- Update quick reference and controls overview.

## Risks

- Current top-level body pagination renders one deferred body stream and clips it per page. A page break can be implemented as vertical blank height, but it cannot reset state by itself.
- If `pageBreak` is placed at a page boundary, it should not accidentally create an extra blank page. Tests should cover this.

