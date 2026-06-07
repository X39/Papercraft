# Hyperlink Control Plan

## Goal

Add document hyperlinks for URLs, mail links, and future internal destinations.

## Proposed XML

```xml
<hyperlink href="https://example.test/invoice/@InvoiceId">View invoice</hyperlink>
```

For rich text integration later:

```xml
<paragraph>
    <span>Open </span>
    <hyperlink href="@Url">customer portal</hyperlink>
    <span>.</span>
</paragraph>
```

## Scope

- Add `HyperlinkControl` as a text-like leaf control first.
- Render visible text with text styling.
- Add backend support for PDF link annotations only if the renderer/canvas can expose annotation commands cleanly.
- If annotation support is not ready, ship visible styled text and document annotation support as pending.

## Attributes

- `href`: target URI.
- `text`: visible text; element content can also supply it.
- `foreground`: default link blue.
- `underline`: bool; default true.
- text style attributes similar to `TextBaseControl`.
- shared control attributes.

## Implementation Steps

1. Add a canvas/display command concept for link annotations, or document that first implementation is visual-only.
2. Add `HyperlinkControl`, likely deriving from `TextBaseControl`.
3. Add underline support either in `HyperlinkControl` render code or as a reusable text feature.
4. For PDF annotations, extend `IDeferredCanvas`, `DisplayList`, and SkiaSharp/PDF backend if viable.
5. Register control.
6. Later, allow `hyperlink` as an inline child inside `paragraph`.

## Tests

- hyperlink measures like text.
- hyperlink draws visible text.
- underline line is drawn when enabled.
- href is required or empty href is handled predictably.
- annotation command is emitted if annotation support exists.

## Documentation

- Add `docs/manual/controls-hyperlink.md`.
- Clarify visual-only vs clickable PDF behavior depending on implemented stage.
- Update quick reference and controls overview.

## Risks

- Existing canvas abstraction has drawing commands but no PDF annotation concept.
- SkiaSharp PDF link annotation support may require backend-specific APIs that do not fit the renderer-neutral canvas cleanly.
- Hyperlink as inline rich text depends on the paragraph/span model.

