# List Controls

Previous: [Flow helper controls](controls-flow.md) | [Controls](controls.md) | Next: [Form and signing controls](controls-forms.md)

## What Is This?

List controls render repeated list items with markers.
Use `ul` for unordered lists, `ol` for ordered lists and `li` for each item.

## When Should I Use This?

Use lists when content is naturally a set of short repeated points.
Each `li` can contain normal controls, including nested lists.

Use [`table`](controls-table.md) when the content needs rows and columns.

## How Do I Start?

These fragments mirror `ListControlTests`:

```xml
<ul marker="circle" indent="10px" markerWidth="3px" itemSpacing="2px">
    <li><text>First</text></li>
</ul>

<ol start="5" markerFormat="({0})" indent="12px" markerWidth="4px" itemSpacing="1px">
    <li><text>First</text></li>
</ol>
```

Lists accept only `li` children.
Each `li` can contain normal controls:

```xml
<ul>
    <li>
        <text>Parent</text>
        <ol start="3">
            <li><text>Nested</text></li>
        </ol>
    </li>
</ul>
```

## Supported Controls

| Control | Children | Use |
|---------|----------|-----|
| `ul` | `li` | Unordered list with a repeated marker. |
| `ol` | `li` | Ordered list with numbered markers. |
| `li` | Normal controls | One list item; can contain text, borders, nested lists or other normal controls. |

## Supported Attributes

| Control | Attributes |
|---------|------------|
| `ul` | `marker`, `indent`, `markerWidth`, `itemSpacing`, shared layout attributes. |
| `ol` | `start`, `markerFormat`, `indent`, `markerWidth`, `itemSpacing`, shared layout attributes. |
| `li` | Shared layout attributes. |

`ul marker` accepts `Disc`, `Circle`, `Square` or `None`.
The current renderer uses simple text fallbacks for unordered markers.
`ol markerFormat` is a composite format, such as `({0})`, that receives the item number.

For length values and shared layout attributes, see [Layout fundamentals](layout-fundamentals.md).

## Common Mistakes

- Putting `text` directly inside `ul` or `ol`. Wrap item content in `li`.
- Using a list when the content needs aligned columns. Use [`table`](controls-table.md) for tabular content.
- Expecting graphic bullet symbols. The current unordered markers are text fallbacks.

Previous: [Flow helper controls](controls-flow.md) | [Controls](controls.md) | Next: [Form and signing controls](controls-forms.md)
