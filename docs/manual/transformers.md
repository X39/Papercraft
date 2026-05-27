# Transformers

Previous: [Controls](controls.md) | [Manual home](index.md) | Next: [Template language](template-language.md)

Status: complete for current supported behavior. Built-in transformer names, block syntax and common mistakes are checked against
`XmlTemplateReader`, `IfTransformer`, `SwitchTransformer`, `ForTransformer`, `ForEachTransformer`,
`VariableTransformer`, `AlternateTransformer` and the transformer tests under `ExpressionTests`.

## What Is This?

A transformer is a template-language block that rewrites part of the XML before the PDF controls are created.
Transformers are written as text lines that start with `@`, followed by a block in braces.

Use transformers to include, choose, repeat or prepare XML:

| Transformer | Use it for |
|-------------|------------|
| `@if` | Include content only when a condition is true. |
| `@switch` | Choose one branch from several cases. |
| `@foreach` | Repeat XML for each item in a list supplied by the application. |
| `@for` | Repeat XML for a numeric range. |
| `@var` | Create a temporary value for one block. |
| `@alternate` | Rotate through values, usually row colors. |

## When Should I Use This?

Use a transformer when the XML structure itself needs to change.
For example, use `@if` when a whole notice should appear or disappear, and use `@foreach` when one table row pattern
should repeat for a list.

Do not use a transformer just to print a changing value.
Use [Template data](template-data.md) for simple values such as `@CustomerName`, `@InvoiceNumber` or `@StatusLabel`.

When logic starts to become business-specific, ask the application team to prepare simpler values.
Templates are easier to maintain when they choose between ready-to-print values instead of calculating them.

## How Do I Start?

Start with one block and one visible result.
This `@if` shape is checked against `IfTransformerTests` and the generated conditional sample in
`TemplateLanguageDocumentationSamples.TemplateLanguage_ConditionalSection`.

```xml
<template>
    <body>
        @if ShowNotice {
            <text>Review required</text>
        }
    </body>
</template>
```

The application must supply `ShowNotice` as a Boolean value.
For a rendered version of this pattern, see [Conditions with `@if`](template-language.md#conditions-with-if).

## Transformer Lines

A transformer line starts in XML text, not as an XML element.
The line names the transformer and gives it arguments:

```xml
@foreach Item in Items {
    <text>@Item</text>
}
```

In the transformer line, write expression names without `@`.
Inside normal text or attributes, use `@Item` to print the temporary value.

The opening `{` starts the block.
The closing `}` ends the block.
Keep braces easy to see; most transformer mistakes are easier to find when each block is small.

## Choose The Smallest Transformer

| Need | Start with |
|------|------------|
| Show or hide one section. | `@if` |
| Choose one label from a status value. | `@switch` |
| Repeat rows from application data. | `@foreach` |
| Repeat a fixed design pattern a known number of times. | `@for` |
| Name a value once inside a block. | `@var` |
| Alternate repeated row colors. | `@alternate` |

Prefer `@foreach` over `@for` for real business lists.
Prefer prepared data over complicated template expressions.

## Small Verified Shapes

These fragments are source/test-checked starting shapes.
Use the full examples in [Template language](template-language.md) when you need more detail.

```xml
@switch Status {
    @case "paid" {
        <text>Paid</text>
    }
    @default {
        <text>Status needs review</text>
    }
}
```

`@case` and `@default` only belong inside `@switch`.
`@default` must be last.

```xml
@foreach Line in Lines with Index {
    <text>@Index: @Line</text>
}
```

`Lines` must be a collection supplied by the application.
`Index` is zero-based.

```xml
@alternate on RowBackground with ["#ffffff", "#f8fafc"] {
    <border background="@RowBackground">
        <text>First row</text>
    </border>
}
@alternate on RowBackground {
    <border background="@RowBackground">
        <text>Second row</text>
    </border>
}
```

The first `@alternate` block supplies the list.
Later blocks using the same variable advance through that list.

## Common Transformer Mistakes

- Writing `@if @ShowNotice`. In transformer expressions, use `@if ShowNotice`.
- Using `else if` without `@`. Use `@else if`.
- Putting regular XML directly inside `@switch` instead of inside `@case` or `@default`.
- Using `@foreach` with a number or text value instead of a collection.
- Using `@for` with a step that moves away from the end value.
- Letting one transformer block do too much. Split large decisions into prepared data or smaller blocks.

## Next Steps

Read [Template language](template-language.md) for the full starter reference and task examples.
Read [Troubleshooting](troubleshooting.md#a-value-prints-empty-or-does-not-change) when expressions do not evaluate.
Read [Table control](controls-table.md#repeat-rows-from-data) for a generated repeated-row example.

Previous: [Controls](controls.md) | [Manual home](index.md) | Next: [Template language](template-language.md)
