# Troubleshooting

Previous: [Complete examples](complete-examples.md) | [Manual home](index.md) | Next: [Developer integration appendix](developer-integration.md)

Status: started. The first entries are checked against `XmlTemplateReader`, `Template`,
`ControlRegistry`, `ControlActivationCache`, `ImageControl`, `DefaultResourceResolver`
and the existing XML/control activation tests.

## What Is This?

Troubleshooting collects common reasons a template does not render as expected.
It focuses on problems a template author can recognize from the XML, data values or visible output.

## When Should I Use This?

Use this chapter when a control is missing, an attribute seems ignored, a value prints empty,
an image does not appear, or content moves to another page unexpectedly.

## How Do I Start?

Start from the symptom:

- XML errors usually come from invalid XML syntax or unsupported structure.
- Missing visible content may be a data, control or layout problem.
- Unexpected page breaks usually need layout and available-space checks.

Each troubleshooting entry will link back to the concept or reference page that explains the underlying behavior.

## The XML Does Not Load

Check basic XML syntax first:

- Every opening element needs a matching closing element unless it is self-closing.
- Attribute values need quotes.
- Special text characters such as `<` and `&` need XML escaping.
- Transformer blocks still live inside XML text, so the surrounding XML must remain valid.

If the error mentions an invalid node name, check for a dot in a normal control name.
`XmlTemplateReaderTests.NoDotInName` verifies that dotted names are rejected for normal nodes.
Style nodes are the special case documented in [Styles](styles.md).

## A Control Is Unknown

If the error says a control does not exist, check the element name first.
Built-in controls are listed in [Controls](controls.md).
Focused pages currently include [Text control](controls-text.md), [Border control](controls-border.md),
[Image control](controls-image.md), [Line control](controls-line.md), [Page number control](controls-page-number.md)
and [Table control](controls-table.md).

Common causes:

- A typo in the element name.
- A custom control that the application has not registered.
- A template namespace change that prevents the built-in controls from being found.

Developer setup and custom control registration belong in the
[developer integration appendix](developer-integration.md).

## An Attribute Is Rejected

Attributes are not free-form labels.
Each control accepts only the parameters implemented for that control.
`ControlActivationCacheTests.CreateControl_ThrowsForUnknownParameter` verifies that unknown parameters are rejected
and that the error can list available parameters.

Check the focused control page for the supported attribute names:

- [Text control](controls-text.md)
- [Border control](controls-border.md)
- [Image control](controls-image.md)
- [Line control](controls-line.md)
- [Page number control](controls-page-number.md)
- [Table control](controls-table.md)

For shared spacing, clipping and alignment attributes, see [Layout fundamentals](layout-fundamentals.md).

## A Control Does Not Allow Children

Some controls are containers and some are leaf controls.
For example, `border` can contain other controls, while `text` and `line` render their own content.

If the error says a control does not support child controls, move the child content into a container such as
`border`, `table` or the document `body`.
`Template.CreateControlAsync` verifies this while building controls from the parsed XML.
For the control-authoring mental model, see [Control concepts](controls-concepts.md).

## A Value Prints Empty Or Does Not Change

Check the exact variable or function name with the application team.
Template data names must match what the application supplies.
The verified [Template data](template-data.md) examples currently cover simple variables and data-backed attributes.

TODO: Add dedicated troubleshooting tests for missing variables in text and attributes before documenting exact
missing-value output. Source areas: `TemplateData.GetVariable`, `XmlTemplateReader.TransformNodeAsync`
and `XmlTemplateReader.TransformNodeTreeExpressionCandidateAsync`.

## A Function Or Transformer Fails

Function calls need a closing parenthesis.
Transformer blocks such as `@if` and `@foreach` need opening and closing braces.
The XML reader has dedicated exceptions for missing function brackets and missing transformer braces.

Keep transformer blocks small while debugging.
Remove unrelated content until only the failing condition or loop remains, then compare it with
[Template language](template-language.md).

TODO: Add focused troubleshooting examples after the transformer syntax examples are verified in
[Template language](template-language.md).

## An Image Does Not Appear

The built-in `image` control uses the application resource resolver.
The default resolver accepts base64 image data and `data:image/...;base64,...` values.
It does not load arbitrary file paths or internet URLs.

If a template needs file, database, object storage or HTTP images, the application must provide a custom resolver.
Ask the application team which image source format is supported for your templates.
For the template-author reference, see [Image control](controls-image.md).

## Content Moves To Another Page

Content in `body` flows through the available page space and can continue on later pages.
Headers, footers, page margins, padding, borders and fixed areas all reduce or change the space available to content.

Start with [Layout fundamentals](layout-fundamentals.md):

- Reduce large `margin`, `padding` and `thickness` values.
- Check whether a header or footer leaves less body space than expected.
- Use normal body content for flowing paragraphs and tables.
- Use `areas` only for fixed-position content that should not affect the body flow.

## Planned Work

- Add dedicated missing-value and expression-error tests.
- Add transformer-specific troubleshooting after the transformer reference examples are written.
- Add image troubleshooting examples for bad source values and unsupported resolver input.
- Add table overflow and row layout troubleshooting after the table control page exists.

Previous: [Complete examples](complete-examples.md) | [Manual home](index.md) | Next: [Developer integration appendix](developer-integration.md)
