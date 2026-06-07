# Checkbox Control Plan

## Goal

Add a checkbox control for forms, approval documents, inspection reports, checklists, and delivery notes.

## Proposed XML

```xml
<checkbox checked="@IsApproved" label="Approved" size="4mm"/>

<checkbox checked="true" size="4mm">
    <text>Include installation service</text>
</checkbox>
```

## Scope

- Add `CheckboxControl`.
- Support a simple label attribute and/or child controls.
- Draw an empty square or checked square.
- Keep it renderer-neutral by using existing line/rect/text draw calls.

## Attributes

- `checked`: bool; default false.
- `size`: length; default `4mm`.
- `label`: string; default empty.
- `gap`: length between box and label/content; default `2mm`.
- `strokeColor`: color; default black.
- `fill`: color; default transparent.
- `checkColor`: color; default black.
- `strokeThickness`: length; default `1pt`.
- shared control attributes.

## Implementation Steps

1. Add `CheckboxControl` deriving from `AlignableContentControl`.
2. Add `Label` as a content-capable or plain attribute? Prefer plain attribute and child controls for richer labels.
3. Measure box size plus label text or child content width.
4. Arrange children to the right of the box.
5. Render fill rectangle, border lines, and check mark lines when checked.
6. Register the control.

## Tests

- unchecked checkbox draws square and no check mark.
- checked checkbox draws two check lines.
- label text is rendered next to the box.
- child controls are arranged next to the box.
- size, gap, and stroke thickness affect drawing coordinates.

## Documentation

- Add `docs/manual/controls-checkbox.md`.
- Update quick reference and controls overview.

## Risks

- If both `label` and child content are supplied, docs should define precedence. Recommended: render label first, then children only if label is empty.
- Checkbox is a visible document mark, not an interactive PDF form field.

