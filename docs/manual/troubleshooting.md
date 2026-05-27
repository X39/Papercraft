# Troubleshooting

Previous: [Complete examples](complete-examples.md) | [Manual home](index.md) | Next: [Developer integration appendix](developer-integration.md)

Status: started. The first entries are checked against `XmlTemplateReader`, `Template`,
`ControlRegistry`, `ControlActivationCache`, `ImageControl`, `DefaultResourceResolver`,
`TableControl`, `TableRowControlBase`, `TableSample.LongTableRows`, `GeneralExpressionTests`,
`TableControlTest.RowTallerThanPageStartsAfterRepeatedHeaderAndIsNotSplit`, `TroubleshootingExpressionTests`,
`TroubleshootingTransformerTests`, `TroubleshootingImageTests` and the existing XML/control activation tests.

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
and [Table control](controls-table.md), plus [Chart controls](controls-chart.md).

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
- [Chart controls](controls-chart.md)

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

If a text value still shows the `@` name, such as `@OrderNumber`, the XML reader did not find a matching variable.
`TroubleshootingExpressionTests.MissingVariableInTextRemainsLiteral` verifies this behavior.

If an attribute value starts with `@` and the variable is missing, the evaluated attribute value becomes empty before
the control receives it. `TroubleshootingExpressionTests.MissingVariableInAttributeBecomesEmptyString` verifies this
behavior. This can later make the control reject the attribute if an empty value is not valid for that attribute type.

Common checks:

- Compare the template name with the data name supplied by the application.
- Check casing, spelling and underscores.
- Do not use `@Customer.Name` for nested object fields. In text, the dot stops the variable name, so this is read as
  `@Customer` followed by `.Name`. `GeneralExpressionTests.DottedTextExpressionReadsOnlyNameBeforeDot` verifies this.
- Do not use a text value as an `@if` condition. Ask for a Boolean flag such as `HasOrderNumber`, or a Boolean
  function supplied by the application. `IfTransformerTests.IfConditionWithoutOperatorRejectsNonBooleanVariable`
  verifies that `@if OrderNumber` is not a supported existence check.
- For attributes such as `color="@AccentColor"`, confirm that the supplied value is a valid value for that attribute.
- Keep a visible fallback label in normal text when missing data would otherwise be hard to notice.

For supported naming patterns, missing-data behavior, optional-value flags and nested-data guidance, see
[Template data](template-data.md#when-data-is-missing).

## A Function Or Transformer Fails

Function calls need a closing parenthesis.
Transformer blocks such as `@if` and `@foreach` need opening and closing braces.
The XML reader has dedicated exceptions for missing function brackets and missing transformer braces.

`TroubleshootingExpressionTests` verifies the common function-expression cases:

- An unknown function in text, such as `@formatTotal()`, is reported as a missing function.
- An unknown function in an attribute, such as `color="@statusColor()"`, is reported as a failed expression evaluation.
- A function call without a closing `)` is reported as a missing closing function bracket.

Keep transformer blocks small while debugging.
Remove unrelated content until only the failing condition or loop remains, then compare it with
[Template language](template-language.md).

## Common Transformer Mistakes

These checks are verified by the existing transformer tests and `TroubleshootingTransformerTests`.

For `@if`:

- Use `@else if`, not bare `else if`.
- Put every `@else if` before the final `@else`.
- Use only one `@else`.
- When there is no comparison operator, the expression must evaluate to `true` or `false`.

`IfTransformerTests.ElseIfAfterElseThrows`, `IfTransformerTests.DuplicateElseThrows` and
`IfTransformerTests.IfTheory` cover these cases.

For `@switch`:

- Put only `@case` and `@default` clauses directly inside the `@switch` block.
- Put `@default` last.
- Use `@default` only once.
- Give every `@case` a value or comparison.

`SwitchTransformerTests.SwitchThrowsForDirectContent`, `SwitchTransformerTests.SwitchThrowsForDuplicateDefault`,
`SwitchTransformerTests.SwitchThrowsForCaseAfterDefault` and `SwitchTransformerTests.SwitchThrowsForEmptyCase`
cover these cases.

For `@for`:

- Use the form `@for Name from Start to End`.
- Use `step` only when the loop should skip values.
- Use a positive `step` when counting up and a negative `step` when counting down.

`ForTransformerTests.ForLoopWithNumbers` verifies normal ranges.
`TroubleshootingTransformerTests.ForLoopStepMustMoveTowardEnd` verifies the step-direction error.

For `@foreach`:

- The value after `in` must be a collection supplied by the application or returned by a function.
- An empty collection renders no repeated content.
- The optional `with IndexName` part creates a zero-based counter for the block.

`ForEachTransformerTests.ForEachLoopWithVariableSourceAndIndex`,
`ForEachTransformerTests.ForEachLoopWithEmptyVariableSource` and
`TroubleshootingTransformerTests.ForEachSourceMustBeCollection` cover these cases.

## An Image Does Not Appear

The built-in `image` control uses the application resource resolver.
The default resolver accepts base64 image data and `data:image/...;base64,...` values.
It does not load arbitrary file paths or internet URLs.

If a template needs file, database, object storage or HTTP images, the application must provide a custom resolver.
Ask the application team which image source format is supported for your templates.
For the template-author reference, see [Image control](controls-image.md).

Common checks:

- If `source` looks like `https://example.com/logo.png` or `C:\Images\logo.png`, the default resolver rejects it.
- If `source` starts with `data:image/`, it must include the comma and base64 payload, such as `data:image/png;base64,...`.
- If `source` is plain base64, it still must decode to real image bytes. Base64 text that is not an image fails during image initialization.
- If `source="@LogoImage"` is used, confirm that the application supplies the image value in the format its resolver expects.

These cases are verified by `TroubleshootingImageTests`.

## Content Moves To Another Page

Content in `body` flows through the available page space and can continue on later pages.
Headers, footers, page margins, padding, borders and fixed areas all reduce or change the space available to content.

Start with [Layout fundamentals](layout-fundamentals.md):

- Reduce large `margin`, `padding` and `thickness` values.
- Check whether a header or footer leaves less body space than expected.
- Use normal body content for flowing paragraphs and tables.
- Use `areas` only for fixed-position content that should not affect the body flow.

## A Table Breaks Or Overflows Unexpectedly

Tables are laid out row by row.
`TableControl` checks the remaining page height before each body row is rendered.
When the next row is taller than the space left on the current page, the table advances to the next page and renders
the table header again before the row.
`TableSample.LongTableRows` exercises a long table with a header, repeated rows, header content and footer content.

Common checks:

- Keep table rows reasonably short. A row with several tall cells may move earlier than expected because the whole row height matters.
- Put repeated column labels in `th` so they can appear again after a table page break.
- Reduce padding, margins and border thickness inside `td` contents when only a few rows fit on each page.
- Check whether the page header, page footer or document margin has left less body height than the table needs.
- Split very large text blocks into more rows before placing them in a table.

If one body row is taller than a full available page, the table does not split that row into smaller row fragments.
The row is still moved to a fresh page when it does not fit in the remaining space. If the table has a `th` header,
the header is rendered again before the oversized row. The row then keeps its arranged height, so it can continue
past one page of body space instead of behaving like normal flowing body text.
`TableControlTest.RowTallerThanPageStartsAfterRepeatedHeaderAndIsNotSplit` verifies this against
`TableControl.PreRender`, `TableControl.DoRender` and `TableRowControlBase.DoArrange`.

For long readable content, split the content into several rows or move it out of the table into normal body content.

## Planned Work

- Add more source-backed troubleshooting entries as new recurring template-author issues are identified.

Previous: [Complete examples](complete-examples.md) | [Manual home](index.md) | Next: [Developer integration appendix](developer-integration.md)
