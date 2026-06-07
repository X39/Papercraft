# Section Or Block Control Plan

## Goal

Add a generic grouping container for document sections that need common spacing, background, borders, and future pagination hints without requiring a visible `border`.

Use `block` as the initial XML name because it is neutral and maps well to existing document flow.

## Proposed XML

```xml
<block margin="0 0 4mm 0" padding="2mm" background="#f8fafc" keep-together="true">
    <text weight="bold">Summary</text>
    <text>@Summary</text>
</block>
```

## Scope

- Add `BlockControl`.
- Allow all normal controls as children.
- Provide optional background drawing.
- Provide optional pagination hints as parameters even if full support is staged.

## Attributes

- `background`: fill color; default transparent.
- `keepTogether`: bool; default false.
- `minHeight`: length; default `0`.
- `pageBreakBefore`: bool; default false.
- `pageBreakAfter`: bool; default false.
- shared margin/padding/alignment attributes.

## Implementation Steps

1. Add `BlockControl` deriving from `AlignableContentControl`.
2. Measure children vertically, taking maximum child width and summed child height.
3. Arrange children vertically.
4. Render optional background over arranged bounds.
5. Render child controls in sequence.
6. Implement `minHeight` immediately.
7. Implement `pageBreakBefore` and `pageBreakAfter` by returning additional height similar to `PageBreakControl`.
8. Treat `keepTogether` as a future engine hint unless a minimal page-boundary implementation is practical.

## Tests

- block accepts arbitrary child controls.
- block measures to max child width and summed child height.
- block honors `minHeight`.
- block background draws behind children.
- `pageBreakBefore` and `pageBreakAfter` affect total rendered page count.
- nested block layout translates children correctly.

## Documentation

- Add `docs/manual/controls-block.md`.
- Update controls overview and quick reference.
- Explain difference from `border`: `block` groups and spaces content; `border` is for explicit border drawing.

## Risks

- `keepTogether` is hard with the current non-splitting top-level flow. It should either be clearly documented as a hint or deferred until the layout engine has first-class page-aware arrange.
- `block` overlaps with `border`; docs need a crisp picker rule.

