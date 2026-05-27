# Template Language

Previous: [Controls](controls.md) | [Manual home](index.md) | Next: [Complete examples](complete-examples.md)

Status: started. Starter transformer examples are checked against `IfTransformerTests`,
`SwitchTransformerTests`, `ForTransformerTests`, `ForEachTransformerTests`, `VariableTransformerTests`
and `AlternateTransformer`.

## What Is This?

The template language changes XML before controls are created.
Its transformer blocks can include content conditionally, choose between alternatives,
repeat content for a range or list, alternate values, and define temporary variables.

Built-in transformer names are `alternate`, `var`, `if`, `switch`, `for` and `foreach`.

## When Should I Use This?

Use this chapter when the document needs different content for different data,
or when the same XML pattern should be repeated.
Typical examples include optional sections, status-specific labels, invoice rows and alternating table row colors.

## How Do I Start?

Start with the simplest transformer that matches the task:

- Use `@if` for optional content.
- Use `@switch` for several alternatives.
- Use `@foreach` for a list supplied by template data.
- Use `@for` for a numeric range.
- Use `@var` to give a short temporary name to a value.
- Use `@alternate` to rotate between values, such as repeated row colors.

In transformer lines, write expression variable names without `@`.
Inside rendered text or attributes, keep using `@VariableName` to insert the value.

For example, this `@if` line reads the `ShowDiscount` value, while the `text` control prints normal text:

```xml
<template>
    <body>
        @if ShowDiscount {
            <text>Discount included</text>
        }
    </body>
</template>
```

The application must supply `ShowDiscount` as a Boolean value.

## Conditions With `@if`

Use `@if` when a section should appear only when a condition is true.
Use `@else if` and `@else` when the template needs a fallback.

```xml
<template>
    <body>
        @if BalanceDue &gt; 0 {
            <text>Payment required</text>
        }
        @else if BalanceDue == 0 {
            <text>Paid in full</text>
        }
        @else {
            <text>Credit balance</text>
        }
    </body>
</template>
```

Because templates are XML, write `<` as `&lt;` and `>` as `&gt;` inside XML text.
Supported comparison operators are `>`, `<`, `>=`, `<=`, `==`, `!=`, `===`, `!==` and `in`.
String equality with `==` and `!=` is case-insensitive; use `===` or `!==` when exact casing matters.

## Choices With `@switch`

Use `@switch` when one value chooses between several branches.
The first matching `@case` is used.
If no case matches, the optional `@default` branch is used.

```xml
<template>
    <body>
        @switch Status {
            @case "paid" {
                <text>Paid</text>
            }
            @case "pending" {
                <text>Pending</text>
            }
            @default {
                <text>Status needs review</text>
            }
        }
    </body>
</template>
```

`@case` and `@default` belong inside `@switch`; they are not standalone transformers.
Put `@default` last, and use it only once.
Cases without an explicit operator use `==`.
Cases can also use operators such as `@case &gt; 3` or `@case in AllowedStatuses`.

## Lists With `@foreach`

Use `@foreach` when the application supplies a list and the template should render one block per item.

```xml
<template>
    <body>
        @foreach Line in Lines {
            <text>@Line</text>
        }
    </body>
</template>
```

The application must supply `Lines` as a collection.
The transformer creates a temporary variable named `Line` for each item.
For a table-row example backed by generated output, see [Repeat rows from data](controls-table.md#repeat-rows-from-data).

Add `with Index` when the template also needs a zero-based counter:

```xml
<template>
    <body>
        @foreach Line in Lines with Index {
            <text>@Index: @Line</text>
        }
    </body>
</template>
```

## Numeric Ranges With `@for`

Use `@for` for a simple numeric range that is part of the template design.
Use `@foreach` for real data lists such as invoice rows.

```xml
<template>
    <body>
        @for Step from 1 to 4 {
            <text>Step @Step</text>
        }
    </body>
</template>
```

The end value is not included, so this example emits steps 1, 2 and 3.
Use `step` to skip values:

```xml
<template>
    <body>
        @for Step from 0 to 10 step 2 {
            <text>Step @Step</text>
        }
    </body>
</template>
```

When counting down, use a negative step:

```xml
<template>
    <body>
        @for Step from 3 to 0 step -1 {
            <text>Step @Step</text>
        }
    </body>
</template>
```

## Temporary Values With `@var`

Use `@var` to give a short temporary name to an expression inside one block.
This can make repeated text easier to read.

```xml
<template>
    <body>
        @var Label = "Invoice total" {
            <text>@Label</text>
        }
    </body>
</template>
```

You can define more than one temporary value in the same block:

```xml
<template>
    <body>
        @var Label = "Customer", Value = CustomerName {
            <text>@Label: @Value</text>
        }
    </body>
</template>
```

`CustomerName` must be supplied by the application.

## Alternating Values With `@alternate`

Use `@alternate` when repeated XML should rotate through a small set of values.
The common document use is alternating table row colors.

```xml
<template>
    <body>
        @alternate on RowLabel with ["Odd", "Even"] {
            <text>@RowLabel row</text>
        }
        @alternate on RowLabel {
            <text>@RowLabel row</text>
        }
        @alternate on RowLabel {
            <text>@RowLabel row</text>
        }
    </body>
</template>
```

The first block supplies the value list.
Later `@alternate on RowLabel` blocks advance to the next value.
The list starts over when it reaches the end.

Use `repeat` when a second block should reuse the current value instead of advancing:

```xml
<template>
    <body>
        @alternate on RowLabel with ["Odd", "Even"] {
            <text>@RowLabel title</text>
        }
        @alternate repeat on RowLabel {
            <text>@RowLabel detail</text>
        }
    </body>
</template>
```

## Common Mistakes

- Do not write `@` before variable names in transformer expressions. Use `@if ShowDiscount`, not `@if @ShowDiscount`.
- Use `@else if`, not bare `else if`.
- Put `@default` last inside `@switch`.
- Use `@foreach` only with a value that is a collection.
- In XML text, write `<` as `&lt;` and `>` as `&gt;`.

## Planned Work

- Add task examples for optional sections.
- Add troubleshooting notes for expression errors and missing values.
- Keep transformer examples small and focused on document-authoring tasks.

Previous: [Controls](controls.md) | [Manual home](index.md) | Next: [Complete examples](complete-examples.md)
