# Template Data

Previous: [First document](first-document.md) | [Manual home](index.md) | Next: [Layout fundamentals](layout-fundamentals.md)

Status: planned.

## What Is This?

Template data is the information supplied by the application and read by the XML template.
A template can display values with expressions such as `@Customer.Name`, and can call functions exposed by the application.

## When Should I Use This?

Use this chapter when a document needs names, addresses, totals, dates, line items or any other value that changes per generated PDF.
Template data keeps the XML design separate from the application code that provides the values.

## How Do I Start?

Look for text that begins with `@`.
That usually means the template is reading a value or calling a function.

The first full version of this chapter will verify examples against the expression and XML tests before documenting detailed syntax.

## Planned Work

- Explain variables and nested property access.
- Explain function calls from a template author's perspective.
- Document supported literal formats for orientation, length, color and thickness.
- Add task examples for inserting values, missing values and repeated table rows.

Previous: [First document](first-document.md) | [Manual home](index.md) | Next: [Layout fundamentals](layout-fundamentals.md)
