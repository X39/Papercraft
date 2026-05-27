# Template Language

Previous: [Controls](controls.md) | [Manual home](index.md) | Next: [Complete examples](complete-examples.md)

Status: planned.

## What Is This?

The template language changes XML before controls are created.
Its transformer blocks can include content conditionally, choose between alternatives,
repeat content for a range or list, alternate values, and define temporary variables.

Built-in transformer names from the README include `alternate`, `var`, `if`, `switch`, `for` and `foreach`.

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

Detailed syntax examples will be added after they are checked against expression and XML tests.

## Planned Work

- Add verified XML examples for `alternate`, `var`, `if`, `switch`, `for` and `foreach`.
- Add task examples for optional sections and repeated table rows.
- Add troubleshooting notes for expression errors and missing values.
- Keep transformer examples small and focused on document-authoring tasks.

Previous: [Controls](controls.md) | [Manual home](index.md) | Next: [Complete examples](complete-examples.md)
