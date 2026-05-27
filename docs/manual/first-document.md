# First Document

Previous: [Introduction](introduction.md) | [Manual home](index.md) | Next: [Template data](template-data.md)

Status: planned.

## What Is This?

A template is an XML document that describes the generated PDF.
The root element name can be flexible, and the examples in this manual use `template`.

Inside the root, the main document sections are `background`, `header`, `body`, `footer`,
`foreground` and `areas`.
The `body` contains the main content. Repeated page content belongs in the header, footer,
background or foreground when those sections are needed.

## When Should I Use This?

Use this chapter when you need to create a new template or understand where content belongs in an existing one.
It should answer beginner questions such as where the main text goes, how repeated page content works,
and when named `area` blocks are useful.

## How Do I Start?

Start with only the section you need:

```xml
<template>
    <body>
        <text>Hello, world!</text>
    </body>
</template>
```

This example is copied from the README `Create a template` section.

Only add `header`, `footer`, `background`, `foreground` or `areas` when the document needs that behavior.

## Planned Work

- Add a complete first-template walkthrough.
- Explain `background`, `header`, `body`, `footer`, `foreground` and `areas`.
- Explain page margin and padding from the template author's point of view.
- Add generated samples for a minimal document, a header/footer document and page margins.

Previous: [Introduction](introduction.md) | [Manual home](index.md) | Next: [Template data](template-data.md)
