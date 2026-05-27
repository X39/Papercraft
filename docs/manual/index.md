# X39.Solutions.PdfTemplate Manual

This manual is for template authors who design PDF documents with XML templates.
It explains the document concepts first, then points to small task examples and reference pages.

## What Is This?

X39.Solutions.PdfTemplate turns XML templates into PDF documents and images.
A template author writes document parts as XML elements such as [`text`](controls-text.md),
[`border`](controls-border.md), [`table`](controls-table.md), [`image`](controls-image.md),
[`line`](controls-line.md), [`pageNumber`](controls-page-number.md) and
[chart controls](controls-chart.md).

Templates can also read values supplied by the application, such as `@CustomerName`,
and can use template-language blocks such as [`@if`](template-language.md#conditions-with-if) and
[`@foreach`](template-language.md#lists-with-foreach) to include or repeat content.

## When Should I Use This?

Use this manual when you need to design or adjust the XML template for a PDF document without changing application code.
Start here if you need to know where page content belongs, which control to use, how to insert data,
or how to make visible layout changes such as spacing, borders and tables.

Developer setup, custom controls and resource resolvers belong in the
[developer integration appendix](developer-integration.md), not in the beginner chapters.

## How Do I Start?

Begin with the smallest visible template:

```xml
<?xml version="1.0" encoding="utf-8"?>
<template>
    <body>
        <text fontsize="18">Hello from a template</text>
    </body>
</template>
```


![Rendered text sample](../assets/samples/text-basic.png)

Next, read [Introduction](introduction.md), then [First document](first-document.md).

## Manual Chapters

1. [Introduction](introduction.md): what a template author controls and the basic vocabulary.
2. [First document](first-document.md): template structure, page sections and the first complete XML file.
3. [Areas](areas.md): fixed-position page rectangles, coordinates, clipping and when not to use areas.
4. [Template data](template-data.md): variables, data-backed attributes, functions and data value formats.
5. [Layout fundamentals](layout-fundamentals.md): available space, margin, padding, borders, alignment, lengths and colors.
6. [Styles](styles.md): shared attributes with `template.style` and related style blocks.
7. [Quick reference](quick-reference.md): compact lookup tables for sections, controls, attributes, value formats and transformer syntax.
8. [Controls](controls.md): built-in XML controls such as [`text`](controls-text.md), [`border`](controls-border.md), [`image`](controls-image.md), [`line`](controls-line.md), [`table`](controls-table.md), [`pageNumber`](controls-page-number.md) and [charts](controls-chart.md). Focused pages now include [Control concepts](controls-concepts.md), [Text control](controls-text.md), [Border control](controls-border.md), [Image control](controls-image.md), [Line control](controls-line.md), [Page number control](controls-page-number.md), [Table control](controls-table.md) and [Chart controls](controls-chart.md).
9. [Transformers](transformers.md): the concept behind [`@if`](template-language.md#conditions-with-if), [`@switch`](template-language.md#choices-with-switch), [`@foreach`](template-language.md#lists-with-foreach), [`@for`](template-language.md#numeric-ranges-with-for), [`@var`](template-language.md#temporary-values-with-var) and [`@alternate`](template-language.md#alternating-values-with-alternate).
10. [Template language](template-language.md): transformer starter syntax and task examples.
11. [Complete examples](complete-examples.md): full document examples such as invoices, reports, table-heavy sheets, product sheets and dashboards.
12. [Troubleshooting](troubleshooting.md): common XML, data, image and layout problems.
13. [Developer integration appendix](developer-integration.md): installation, service registration and extension points.
