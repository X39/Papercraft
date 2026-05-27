# Manual Roadmap

This page is the target structure for the user manual. Each section should be written for a template author first, with developer details moved into clearly marked integration notes.

The manual should combine concepts and practical tasks. A reader should first be able to understand what a feature is and why it exists, then search for a question like "How do I add page numbers?" and find a small XML sample, a rendered output image and a link to the full reference.

For implementation progress, sample backlog and agent assignments, see the [documentation roadmap](work-plan.md).

## Documentation approach

- Start with a user task.
- For major topics, explain "What is this?" and "When should I use this?" before examples.
- Show the smallest useful XML sample.
- Show the generated output image when the task changes visible document layout.
- Explain only the concepts needed for that task.
- Link to deeper reference material for attributes, supported values and developer extension points.

## 1. Introduction

- What X39.Solutions.PdfTemplate is used for.
- What a template author can change without rebuilding an application.
- The difference between XML controls, template data, functions and transformers.
- A minimal "Hello, world" template.

## 2. First document

- Basic XML document structure.
- `background`, `header`, `body` and `footer`.
- What areas are and why they exist.
- When to use named areas and when regular body content is enough.
- Page size, margin and padding concepts.
- Rendering flow from template to PDF.
- Complete first-template example.

## 3. Template data

- Variables such as `@Customer.Name`.
- Property access and nested values.
- Template data types and supported literal formats.
- Functions such as `@total()`.
- Practical guidance for naming values.

## 4. Layout fundamentals

- What available space means.
- Available space and measurement.
- Why margin, border and padding behave differently.
- Horizontal and vertical alignment.
- Length values and units.
- Colors.
- Thickness values.
- Text styles and fonts.

## 5. Controls

Each control chapter should include purpose, allowed children, attributes, examples and common mistakes.

- `text`
- `border`
- `image`
- `line`
- `pageNumber`
- `table`
- `th`
- `tr`
- `td`
- `chart`
- `lineChart`
- `barChart`
- `pieChart`
- `data`

## 6. Template language

- What transformers are and why they exist.
- Transformer syntax.
- Conditional content with `if` and `switch`.
- Repeated content with `for` and `foreach`.
- Alternating output with `alternate`.
- Defining values with `var`.
- Combining transformers with controls.

## 7. Complete examples

- Invoice.
- Report with header, footer and page numbers.
- Table-heavy document.
- Document with images.
- Document with charts.

## 8. Troubleshooting

- XML syntax errors.
- Unknown controls or attributes.
- Missing variables or functions.
- Image loading issues.
- Layout overflow and unexpected page breaks.

## 9. Developer integration appendix

- Installing the NuGet package.
- Registering services.
- Supplying template data.
- Custom functions.
- Custom controls.
- Custom transformers.
- Resource resolvers.
- Relevant public interfaces.

## Source material

The first version of each chapter should be migrated from the repository README, then rewritten for a non-developer audience where necessary. Tests and samples under `test/X39.Solutions.PdfTemplate.Test/Samples` should be used as executable examples.
