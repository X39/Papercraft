# Columns Plan

## Goal

Add true multi-column document flow for sections that should fill one column, continue in the next, and then continue on later pages.

This is not the same as a table. A table aligns rows and cells; columns flow content across page regions.

## Proposed XML

```xml
<columns count="2" gap="6mm" rule-thickness="0.5pt" rule-color="#cbd5e1">
    <text>Long body copy...</text>
    <text>More body copy...</text>
</columns>
```

## Scope

- Add `ColumnsControl` only if it can provide automatic flow.
- Do not add a weak side-by-side-only `column` control unless intentionally scoped as a separate feature.
- Initial implementation may support block-level children only; true text splitting can be staged.

## Attributes

- `count`: integer; default `2`.
- `gap`: length; default `5mm`.
- `balance`: bool; default false.
- `ruleThickness`: length; default `0`.
- `ruleColor`: color; default transparent.
- shared control attributes.

## Implementation Steps

1. Audit the current body rendering model and identify whether child controls can be split.
2. Add a page-aware layout abstraction if needed: remaining region, forced break, and continuation.
3. Implement `ColumnsControl` as a section-flow container.
4. Compute column width: `(availableWidth - gaps) / count`.
5. Lay out children into column regions, moving to the next column when a child no longer fits.
6. For non-splittable children, move whole child to the next column.
7. For text/paragraph controls, add a future splittable interface so long text can continue within columns.
8. Render optional vertical rules between columns.
9. Register control once behavior is usable.

## Tests

- short content stays in first column.
- content that exceeds first column continues in second column.
- content that exceeds all columns contributes additional height/pages.
- gap and rule drawing coordinates are correct.
- oversized non-splittable child moves as a whole.

## Documentation

- Add `docs/manual/controls-columns.md`.
- Explain difference from table.
- Document current splitting limitations honestly.

## Risks

- This is a layout-engine feature, not just a control. The current top-level body uses one vertical deferred stream clipped per page; true columns need either a new page-aware flow pass or a control-local pagination model.
- Without splittable text, columns are only useful for multiple short blocks, not long prose. That may not justify shipping the control yet.

## Recommended Staging

1. First milestone: plan engine hooks and add tests around current pagination.
2. Second milestone: non-splitting `ColumnsControl` for block-level content.
3. Third milestone: splittable text/paragraph support for true long-form columns.

