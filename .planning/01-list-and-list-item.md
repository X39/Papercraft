# List And ListItem Controls Plan

## Goal

Add document-list controls using HTML-like terminology: `ul`, `ol`, and `li`.

The controls should cover normal document use cases: bullet lists, numbered lists, nested lists, compact checklists, and list items that contain normal controls.

## Proposed XML

```xml
<ul marker="disc" indent="6mm" marker-width="5mm" item-spacing="1mm">
    <li><text>First item</text></li>
    <li><text>Second item</text></li>
</ul>

<ol start="1" marker-format="{0}." indent="7mm">
    <li><text>First step</text></li>
    <li><text>Second step</text></li>
</ol>
```

## Scope

- Add `UnorderedListControl` registered as `ul`.
- Add `OrderedListControl` registered as `ol`.
- Add `ListItemControl` registered as `li`.
- Allow `li` to contain any normal control.
- Allow nested `ul` and `ol` inside `li`.
- Render markers as text, not as custom vector glyphs, so it reuses existing text infrastructure.

## Attributes

`ul`:

- `marker`: `disc`, `circle`, `square`, `none`; default `disc`.
- `indent`: length from list left edge to item content; default `6mm`.
- `markerWidth`: length reserved for marker; default `4mm`.
- `itemSpacing`: length between list items; default `0`.
- shared control attributes.

`ol`:

- `start`: integer start number; default `1`.
- `markerFormat`: composite format such as `{0}.`; default `{0}.`.
- `indent`, `markerWidth`, `itemSpacing`.
- shared control attributes.

`li`:

- shared control attributes.
- no marker attributes initially; marker is owned by parent list.

## Implementation Steps

1. Add enum `EListMarkerStyle`.
2. Add an abstract `ListControlBase` deriving from `AlignableContentControl`.
3. Add `UnorderedListControl`, `OrderedListControl`, and `ListItemControl`.
4. Register new controls in `ServiceRegistrationOperations.AddDefaultControls`.
5. Measure each item by measuring marker text and item content with reduced available width.
6. Arrange each `li` with content width equal to `remainingWidth - indent`.
7. Render marker at the list item baseline/top and render item content translated by `indent`.
8. Treat list items as keep-together blocks; do not split a single `li` yet.

## Tests

- `ul` accepts only `li` children.
- `li` accepts normal controls and nested lists.
- measured width accounts for marker plus content indent.
- marker draw calls are emitted for each item.
- `ol start` and `markerFormat` produce expected markers.
- nested list translates content by cumulative indentation.
- template creation rejects direct `text` child under `ul`/`ol`.

## Documentation

- Add `docs/manual/controls-list.md`.
- Update `docs/manual/controls.md`.
- Update `docs/manual/quick-reference.md`.
- Update `docs/_data/navigation.yml`.

## Risks

- Current text service measures a single style at a time. Marker rendering should keep its own simple style and avoid full rich text.
- Pagination behavior is coarse: list items can move as normal vertical blocks, but a long item cannot split across pages until the layout engine grows splittable controls.

