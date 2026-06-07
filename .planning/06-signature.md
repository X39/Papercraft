# Signature Control Plan

## Goal

Add a signature placeholder control that reserves space, draws a signing line, and renders optional subtext.

This covers common document signatures without making template authors assemble spacer, line, and text manually.

## Proposed XML

```xml
<signature height="22mm" line-width="60mm" label="Signature" subtext="@SignerName"/>
```

## Scope

- Add `SignatureControl`.
- Reserve a configurable block of empty vertical space.
- Draw a horizontal line near the bottom.
- Draw optional label/subtext below or above the line.

## Attributes

- `height`: length; default `20mm`.
- `lineWidth`: length; default `100%`.
- `lineThickness`: length; default `1pt`.
- `lineColor`: color; default black.
- `label`: string; default `Signature`.
- `subtext`: string; default empty.
- `textPlacement`: `Below`, `Above`; default `Below`.
- text style attributes, matching `TextBaseControl` where practical.
- shared alignment attributes.

## Implementation Steps

1. Add `SignatureControl` deriving from `AlignableControl`.
2. Inject `ITextService` for label/subtext measurement and drawing.
3. Measure to configured width/height.
4. Arrange using line width and text sizes.
5. Render the line, then label/subtext.
6. Register the control.

## Tests

- signature measures to configured height.
- signature draws one line with expected coordinates.
- label text is drawn.
- subtext text is drawn when supplied.
- centered/right alignment works through base alignment behavior.

## Documentation

- Add `docs/manual/controls-signature.md`.
- Update quick reference and controls overview.

## Risks

- This is not a digital signature feature and should not be documented as one.
- Text placement must not consume the reserved signing space unexpectedly.

