***Note for [NuGet.org](https://www.nuget.org/packages/X39.Solutions.PdfTemplate):***
*This README is intentionally short. For the full template-author documentation, use the
[GitHub Pages user manual](https://x39.github.io/X39.Solutions.PdfTemplate/manual/).*

![A sample output for reference](https://raw.githubusercontent.com/X39/X39.Solutions.PdfTemplate/master/.github/media/sample.png)

# X39.Solutions.PdfTemplate

X39.Solutions.PdfTemplate generates PDF documents and images from XML templates in .NET.
It renders with SkiaSharp and provides built-in controls for text, borders, images, lines,
tables, page numbers and charts.

## User Manual

Template authors should start with the
[GitHub Pages user manual](https://x39.github.io/X39.Solutions.PdfTemplate/manual/).
The manual explains document structure, template data, layout, controls, transformers,
complete examples and troubleshooting from the perspective of people who edit XML templates.
The Pages site includes a persistent manual table of contents so chapters can be reached
without returning to the landing page.

Useful manual entry points:

- [First document](https://x39.github.io/X39.Solutions.PdfTemplate/manual/first-document.html)
- [Template data](https://x39.github.io/X39.Solutions.PdfTemplate/manual/template-data.html)
- [Layout fundamentals](https://x39.github.io/X39.Solutions.PdfTemplate/manual/layout-fundamentals.html)
- [Quick reference](https://x39.github.io/X39.Solutions.PdfTemplate/manual/quick-reference.html)
- [Controls](https://x39.github.io/X39.Solutions.PdfTemplate/manual/controls.html)
- [Template language](https://x39.github.io/X39.Solutions.PdfTemplate/manual/template-language.html)
- [Complete examples](https://x39.github.io/X39.Solutions.PdfTemplate/manual/complete-examples.html)
- [Developer integration appendix](https://x39.github.io/X39.Solutions.PdfTemplate/manual/developer-integration.html)

## Requirements

- .NET 8.0 or later
- A dependency injection container that can provide the services registered by `AddPdfTemplateService`
- On Linux, the SkiaSharp native Linux assets package

The package is marked trim-compatible and depends on SkiaSharp,
`Microsoft.Extensions.DependencyInjection.Abstractions` and `X39.Util`.
Issues are tracked in the
[GitHub repository](https://github.com/X39/X39.Solutions.PdfTemplate/issues).

## Install

Install the [NuGet package](https://www.nuget.org/packages/X39.Solutions.PdfTemplate/):

```shell
dotnet add package X39.Solutions.PdfTemplate
```

On Linux, also install the
[SkiaSharp native Linux assets](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux):

```shell
dotnet add package SkiaSharp.NativeAssets.Linux
```

## Minimal Template

Templates are XML documents:

```xml
<template>
    <body>
        <text>Hello, world!</text>
    </body>
</template>
```

For template-author guidance, use the
[manual](https://x39.github.io/X39.Solutions.PdfTemplate/manual/).
For application setup, use the
[developer integration appendix](https://x39.github.io/X39.Solutions.PdfTemplate/manual/developer-integration.html).

## Generate a PDF

Register the library services at startup:

```csharp
services.AddPdfTemplateService();
```

Then resolve a `Generator` and render the template:

```csharp
using System.Globalization;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate;

await using var scope = serviceProvider.CreateAsyncScope();
using var generator = scope.ServiceProvider.GetRequiredService<Generator>();

using var reader = XmlReader.Create(xmlTemplateStream);
await using var output = File.Create("document.pdf");

await generator.GeneratePdfAsync(
    output,
    reader,
    CultureInfo.CurrentUICulture);
```

Common extension points are documented in the
[developer integration appendix](https://x39.github.io/X39.Solutions.PdfTemplate/manual/developer-integration.html):

- Set template variables with `generator.TemplateData.SetVariable("Name", value)`.
- Add custom functions with `services.AddPdfTemplateService((builder) => builder.AddFunction<MyFunction>())`.
- Add custom controls with `services.AddPdfTemplateService((builder) => builder.AddControl<TControl>())`.
- Add custom transformers with `services.AddPdfTemplateService((builder) => builder.AddTransformer<TTransformer>())`.
- Configure document-level options such as margin through `DocumentOptions`.

## Building and Testing

Restore and build locally:

```shell
dotnet restore
dotnet build --no-restore
```

Run tests:

```shell
dotnet test --framework net8.0 --no-build --verbosity normal
```

Create a local package:

```shell
dotnet pack --configuration Release
```

The pull-request workflow is defined in `.github/workflows/run-dotnet-tests.yml`.
The publish workflow is defined in `.github/workflows/main.yml`.

## Documentation

The GitHub Pages source lives under [`docs`](docs/index.md).
The Pages table of contents is maintained in [`docs/_data/navigation.yml`](docs/_data/navigation.yml).
Executable documentation samples live under `test/X39.Solutions.PdfTemplate.Test/Samples`
and write generated preview assets under `docs/assets/samples`.

## Contributing

Contributions are welcome.
Please submit a pull request or create a discussion to discuss changes.

Add yourself to [`CONTRIBUTORS`](CONTRIBUTORS.md) for your first pull request and include this agreement text:

```text
By contributing to this project, you agree to the following terms:
- You grant me and any other person who receives a copy of this project the right to use your contribution under the
  terms of the GNU Lesser General Public License v3.0.
- You grant me and any other person who receives a copy of this project the right to relicense your contribution under
  any other license.
- You grant me and any other person who receives a copy of this project the right to change your contribution.
- You waive your right to your contribution and transfer all rights to me and every user of this project.
- You agree that your contribution is free of any third-party rights.
- You agree that your contribution is given without any compensation.
- You agree that I may remove your contribution at any time for any reason.
- You confirm that you have the right to grant the above rights and that you are not violating any third-party rights
  by granting these rights.
- You confirm that your contribution is not subject to any license agreement or other agreement or obligation, which
  conflicts with the above terms.
```

Additional controls are welcome when they do not add dependencies to the core library.
Controls that need additional libraries should usually live in separate packages.

## Semantic Versioning

This library follows the principles of [Semantic Versioning](https://semver.org/).

| Change | Meaning |
|--------|---------|
| Patch | Backwards-compatible bug fixes or small internal changes. |
| Minor | Backwards-compatible features or additions. |
| Major | Breaking changes. |

## License

This project is licensed under the GNU Lesser General Public License v3.0.
See the [LICENSE](LICENSE) file for details.
