# Rich Text Controls

Previous: [Text control](controls-text.md) | [Controls](controls.md) | Next: [Flow helper controls](controls-flow.md)

## What Is This?

Rich text controls let one paragraph contain multiple inline text fragments.
Use `paragraph` as the container, `span` for inline text runs and `br` for an explicit line break.
Use `hyperlink` when a single link-style text control is enough.

## When Should I Use This?

Use `paragraph` when a line needs mixed text styling, such as a normal label followed by a bold value.
Use `hyperlink` when the template should draw link-styled text with a target stored in `href`.

Use [`text`](controls-text.md) instead when all words in the control share one style.

## How Do I Start?

This fragment mirrors the activation coverage in `ParagraphControlTests`:

```xml
<paragraph foreground="red" fontsize="14" lineheight="1.5">
    <span>Total: </span>
    <span foreground="blue" weight="bold">Value</span>
    <br/>
</paragraph>
```

`paragraph` accepts only `span` and `br` children.
Do not put `text`, `image`, `table` or other normal controls directly inside a paragraph.

For link-style text, use `hyperlink`.
This fragment mirrors `HyperlinkControlTests`:

```xml
<hyperlink
    href="https://example.test/invoice/123"
    underline="false"
    foreground="red"
    fontsize="14">View invoice</hyperlink>
```

The current renderer draws the visible text and optional underline.
The `href` value is kept as control data; it does not create a PDF navigation annotation by itself.

## Supported Controls

| Control | Children | Use |
|---------|----------|-----|
| `paragraph` | `span`, `br` | Rich text paragraph with inline runs and explicit line breaks. |
| `span` | None | Inline text run inside `paragraph`. |
| `br` | None | Explicit line break inside `paragraph`. |
| `hyperlink` | None | Link-style text with optional underline and an `href` value. |

## Supported Attributes

| Control | Attributes |
|---------|------------|
| `paragraph` | Text styling attributes: `foreground`, `fontsize`, `lineheight`, `scale`, `rotation`, `strokethickness`, `letterspacing`, `weight`, `style`, `fontfamily`; shared layout attributes. |
| `span` | `text` or element content, plus optional text styling overrides: `foreground`, `fontsize`, `lineheight`, `scale`, `rotation`, `strokethickness`, `letterspacing`, `weight`, `style`, `fontfamily`. |
| `br` | Shared layout attributes only; no control-specific attributes. |
| `hyperlink` | `href`, `text` or element content, `underline`, text styling attributes and shared layout attributes. |

For shared `margin`, `padding`, `clip`, `horizontalAlignment` and `verticalAlignment`, see
[Layout fundamentals](layout-fundamentals.md).

## Common Mistakes

- Putting normal block controls inside `paragraph`. Use `span` and `br` only.
- Expecting `span` or `br` to render on their own outside `paragraph`.
- Expecting `href` to create a clickable PDF link. The current control renders visual link text.
- Using `paragraph` for simple text that can be handled by [`text`](controls-text.md).

Previous: [Text control](controls-text.md) | [Controls](controls.md) | Next: [Flow helper controls](controls-flow.md)
