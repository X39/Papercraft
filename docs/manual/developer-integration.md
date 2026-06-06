# Developer Integration Appendix

Previous: [Troubleshooting](troubleshooting.md) | [Manual home](index.md)

## What Is This?

The developer integration appendix is the place for application setup and extension details.
It keeps service registration, custom controls, custom transformers, functions, resource resolvers and public interfaces out of the template-author chapters.

## When Should I Use This?

Use this appendix when a template change requires application code, such as adding a new function,
supplying new data, loading images from a custom location, or registering a custom control.

## How Do I Start?

Start with the package and service registration.
`ServiceCollectionExtensions`, `PdfTemplateServiceBuilder`, `Generator`, `DocumentOptions`,
`IFunction`, `ITransformer`, `IControl`, `ITemplateData`, `IResourceResolver`, `IDrawableCanvas`,
`IDeferredCanvas`, `IImmediateCanvas`, `IPropertyAccessCache`, `ITextService`, `IParameterConverter`

## Install

The library targets .NET 10.0.
Install the main package in the application that generates the PDF:

```shell
dotnet add package X39.Solutions.PdfTemplate
```

On Linux, also install the SkiaSharp native Linux assets package:

```shell
dotnet add package SkiaSharp.NativeAssets.Linux
```

## Register Services

Register the built-in generator, controls, transformers and supporting services at application startup:

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate;

services.AddPdfTemplateService();
```

`AddPdfTemplateService` registers the built-in controls, the built-in transformers, `Generator`,
`ITemplateData`, `IControlFactory`, the default text service, property-access cache and default image resolver.

Use the builder overload when the application needs to add or replace template features:

```csharp
services.AddPdfTemplateService(
    (builder) => builder
        .AddFunction<MyFunction>()
        .AddControl<MyControl>()
        .AddTransformer<MyTransformer>());
```

## Generate A PDF

Resolve a `Generator`, create an `XmlReader` for the template, and write the PDF to a stream:

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

`Generator` is registered as transient and is not thread-safe.
Use a fresh resolved generator for each document render, or keep concurrent use outside the same instance.

## Supply Template Data

Template authors can only use data that the application supplies.
Set variables before rendering:

```csharp
generator.TemplateData.SetVariable("CustomerName", customer.Name);
generator.TemplateData.SetVariable("InvoiceTotal", invoice.Total);
```

The template can then read those values:

```xml
<template>
    <body>
        <text>@CustomerName</text>
        <text>Total: @InvoiceTotal</text>
    </body>
</template>
```

If a template author needs a new value, the application should expose it as a variable or function instead of asking the author to hard-code business logic in XML.

## Configure Document Options

Pass `DocumentOptions` when page setup or per-request context must differ from the default:

```csharp
using System.Globalization;
using X39.Solutions.PdfTemplate;
using X39.Solutions.PdfTemplate.Data;

await generator.GeneratePdfAsync(
    output,
    reader,
    CultureInfo.CurrentUICulture,
    new DocumentOptions
    {
        Margin = new Thickness(new Length(1, ELengthUnit.Centimeters)),
        Producer = "Invoice service",
        Context = new PrintRequestContext(invoice.Id),
    });
```

Useful options include:

| Option | Use |
|--------|-----|
| `PageWidthInMillimeters` and `PageHeightInMillimeters` | Change the page size. Defaults match A4 dimensions. |
| `DotsPerInch`, `DotsPerCentimeter` and `DotsPerMillimeter` | Change render density. |
| `Margin` | Reserve document margin before header, body and footer layout. |
| `Producer` and `Modified` | Set PDF metadata. |
| `Context` | Pass request-specific information to context-aware extension points. |
| `IgnoreErrors` | Instruct the generator to ignore errors where possible. Use carefully, because XML can still become invalid. |

## Add A Function

Use a function when a template needs a reusable value or calculation such as a formatted total, lookup result or application-specific label.

```csharp
using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;

public sealed class CustomerLabelFunction : IFunction
{
    public string Name => "customerLabel";
    public int Arguments => 1;
    public bool IsVariadic => false;

    public ValueTask<object?> ExecuteAsync(
        CultureInfo cultureInfo,
        object?[] arguments,
        CancellationToken cancellationToken = default)
    {
        var customerId = Convert.ToString(arguments[0], cultureInfo);
        return ValueTask.FromResult<object?>($"Customer {customerId}");
    }
}
```

Register the function:

```csharp
services.AddPdfTemplateService(
    (builder) => builder.AddFunction<CustomerLabelFunction>());
```

A template author can call it after the application exposes it:

```xml
<text>@customerLabel(CustomerId)</text>
```

## Add A Custom Control

Use a custom control when the built-in XML controls cannot render a required visual element.
Most custom controls should derive from `Control` or an existing base control instead of implementing `IControl` directly.

```csharp
using System.Globalization;
using X39.Solutions.PdfTemplate;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;

[Control(Constants.ControlsNamespace, "approvalStamp")]
public sealed class ApprovalStampControl : Control
{
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => Size.Zero;

    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => Size.Zero;

    protected override Size DoRender(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo)
        => Size.Zero;
}
```

Register the control:

```csharp
services.AddPdfTemplateService(
    (builder) => builder.AddControl<ApprovalStampControl>());
```

Use the registered element name in the template:

```xml
<template>
    <body>
        <approvalStamp/>
    </body>
</template>
```

The current XML reader does not support namespace-prefixed element names for controls.
is rejected before control activation.
If a custom control must appear beside built-in controls, register it with `Constants.ControlsNamespace`
and a unique element name, as shown above.

## Add A Transformer

Use a transformer only when XML nodes need to be included, removed, repeated or rewritten before controls are created.
For simple calculated values, prefer a function.

```csharp
using System.Globalization;
using System.Runtime.CompilerServices;
using X39.Solutions.PdfTemplate.Abstraction;
using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

public sealed class KeepChildrenTransformer : ITransformer
{
    public string Name => "keep";

    public async IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        foreach (var node in nodes)
        {
            yield return node.DeepCopy();
        }
    }
}
```

Register the transformer:

```csharp
services.AddPdfTemplateService(
    (builder) => builder.AddTransformer<KeepChildrenTransformer>());
```

The transformer is then available by name:

```xml
@keep {
    <text>This node is returned by the custom transformer.</text>
}
```

## Add An Image Resolver

The default image resolver accepts base64 image data and `data:image/...;base64,...` sources.
It does not read the file system or the internet.
Register a custom `IResourceResolver` after `AddPdfTemplateService` when templates need images from application storage:

```csharp
using X39.Solutions.PdfTemplate.Services.ResourceResolver;

services.AddPdfTemplateService();
services.AddScoped<IResourceResolver, ApplicationImageResolver>();
```

```csharp
public sealed class ApplicationImageResolver : IResourceResolver
{
    public async ValueTask<byte[]> ResolveImageAsync(
        string source,
        object? context,
        CancellationToken cancellationToken = default)
    {
        var request = context as PrintRequestContext;
        return await LoadImageBytesAsync(source, request, cancellationToken);
    }
}
```

`DocumentOptions.Context` is passed unchanged to `IResourceResolver.ResolveImageAsync`
and to controls that implement `IInitializeControlAsync`.

## Extension Point Map

Choose the smallest extension point that solves the template author's request.
Most requests fit one of these rows:

| Interface or type | Use when | How it is wired |
|-------------------|----------|-----------------|
| `Generator` | The application needs to render a template to PDF or bitmaps. | Resolve it from dependency injection for each render. |
| `ITemplateData` | The application supplies variables, registers functions or evaluates expressions inside custom extensions. | Use `generator.TemplateData` before rendering; custom transformers and functions receive it through their APIs. |
| `IFunction` | The template needs a reusable calculated value. | Implement `Name`, argument metadata and `ExecuteAsync`, then register with `AddFunction<TFunction>()`. |
| `IControl` | The application must render a new XML element. | Add `[Control(...)]`, implement measure/arrange/render behavior and register with `AddControl<TControl>()`. |
| `IContentControl` | A custom control must contain child controls. | Implement `CanAdd` and child storage, or derive from an existing content-control base. |
| `IInitializeControlAsync` | A control needs async setup or request context before rendering. | Implement it on the control; `DocumentOptions.Context` is passed to `InitializeControlAsync`. |
| `ITransformer` | XML nodes must be conditionally rewritten, removed or repeated before controls are created. | Implement `Name` and `TransformAsync`, then register with `AddTransformer<TTransformer>()`. |
| `IResourceResolver` | Image sources must be resolved from application-specific storage. | Register an `IResourceResolver` implementation after `AddPdfTemplateService`. |
| `IParameterConverter<T>` | A custom control attribute needs custom parsing. | Set the converter on the control property with `ParameterAttribute.Converter`. |
| `IControlFactory` | Advanced activation behavior is needed. | Replace the DI service only for unusual activation needs; most applications should use `AddControl<TControl>()`. |

Source-checked registration notes:

- `Generator` is registered as transient.
- `ITemplateData`, `IResourceResolver` and `IControlFactory` are registered as scoped services.
- Built-in transformer registrations are transient; custom transformers added with `AddTransformer<TTransformer>()` use the same path.
- Functions added with `AddFunction<TFunction>()` are scoped by default, and the builder accepts another `ServiceLifetime`.
- Control registration stores control metadata; a fresh control instance is created for template nodes through the control factory.
- `DocumentOptions.Context` is opaque to the library and is passed unchanged to resource resolvers and async control initialization.

## Advanced Infrastructure Interfaces

These interfaces are public because controls and extension points use them.
Most applications should not replace them directly.

| Interface | Use |
|-----------|-----|
| `IDrawableCanvas` | Low-level drawing surface used by controls. Custom controls may call it through the deferred canvas. |
| `IDeferredCanvas` | Canvas passed to `IControl.Render`; it records drawing work until page-specific values are available. |
| `IImmediateCanvas` | Canvas exposed inside deferred drawing for page-specific information such as the current page and total pages. |
| `ITextService` | Shared text measurement and drawing service used by text-based controls. Replacing it changes text behavior globally. |
| `IPropertyAccessCache` | Expression-evaluation infrastructure that caches property access. It is not a normal application extension point. |

## When Template Authors Need Developer Help

Ask for application work when a manual page or template needs:

- a variable that is not already present,
- a function that is not already registered,
- an image source that the current resolver cannot load,
- a control that does not exist in the built-in controls,
- a transformer beyond the built-in template language,
- document options that the application does not expose.

Keep those decisions in application code.
The user-facing template should stay focused on XML structure, layout, data placeholders and small task examples.

Previous: [Troubleshooting](troubleshooting.md) | [Manual home](index.md)
