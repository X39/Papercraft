***Note for [NuGet.org](https://www.nuget.org/packages/X39.Solutions.PdfTemplate):***
*Some XML comments in this README are not rendered by the NuGet Markdown parser.
For the best reading experience, use the [GitHub README](https://github.com/X39/X39.Solutions.PdfTemplate).*

![A sample output for reference](https://raw.githubusercontent.com/X39/X39.Solutions.PdfTemplate/master/.github/media/sample.png)

<!-- TOC -->
* [X39.Solutions.PdfTemplate](#x39solutionspdftemplate)
  * [Getting Started](#getting-started)
    * [Requirements](#requirements)
    * [Install](#install)
    * [Create a template](#create-a-template)
    * [Generate a PDF](#generate-a-pdf)
    * [Useful next steps](#useful-next-steps)
  * [Core concepts](#core-concepts)
  * [Template structure](#template-structure)
    * [About `areas`](#about-areas)
  * [Integration](#integration)
    * [Functions](#functions)
    * [Variables](#variables)
    * [Template data types](#template-data-types)
      * [`Orientation`](#orientation)
      * [`Length`](#length)
      * [`Color`](#color)
      * [`Thickness`](#thickness)
    * [Controls](#controls)
      * [Creating your own control](#creating-your-own-control)
      * [`text`](#text)
      * [`border`](#border)
      * [`image`](#image)
      * [`line`](#line)
      * [`pageNumber`](#pagenumber)
      * [`table`](#table)
        * [`th`](#th)
        * [`tr`](#tr)
        * [`td`](#td)
      * [`chart`](#chart)
        * [`lineChart`](#linechart)
        * [`barChart`](#barchart)
        * [`pieChart`](#piechart)
        * [`data`](#data)
    * [Transformers](#transformers)
      * [Creating your own transformer](#creating-your-own-transformer)
        * [Evaluating user data](#evaluating-user-data)
        * [Introducing new variables or changing existing](#introducing-new-variables-or-changing-existing)
      * [`alternate`](#alternate)
      * [`var`](#var)
      * [`if`](#if)
      * [`for`](#for)
      * [`foreach`](#foreach)
    * [Interfaces](#interfaces)
      * [`IDrawableCanvas`](#idrawablecanvas)
      * [`IDeferredCanvas`](#ideferredcanvas)
      * [`IImmediateCanvas`](#iimmediatecanvas)
      * [`IControl`](#icontrol)
      * [`IControlFactory`](#icontrolfactory)
      * [`IContentControl`](#icontentcontrol)
      * [`IFunction`](#ifunction)
      * [`IInitializeControlAsync`](#iinitializecontrolasync)
      * [`IParameterConverter`](#iparameterconverter)
      * [`ITemplateData`](#itemplatedata)
      * [`ITransformer`](#itransformer)
      * [`IPropertyAccessCache`](#ipropertyaccesscache)
      * [`ITextService`](#itextservice)
      * [`IResourceResolver`](#iresourceresolver)
  * [Building and Testing](#building-and-testing)
  * [Documentation status](#documentation-status)
  * [Contributing](#contributing)
    * [Code of Conduct](#code-of-conduct)
    * [Contributors Agreement](#contributors-agreement)
    * [Additional controls](#additional-controls)
  * [Semantic Versioning](#semantic-versioning)
  * [License](#license)
<!-- TOC -->

# X39.Solutions.PdfTemplate

X39.Solutions.PdfTemplate generates PDF documents and images from XML templates in .NET.
It renders with SkiaSharp and provides built-in controls for text, borders, images, lines,
tables, page numbers and charts.

Templates can use application data through variables such as `@customer.Name`, reusable
logic through functions such as `@total()`, and preprocessing blocks such as `@if`,
`@foreach` and `@var`.
If the built-in controls are not enough, you can add your own controls, transformers,
functions and resource resolvers.

Use this library when you want XML templates that can be edited outside your compiled
application, but still need access to strongly typed .NET data and extension points.

## Getting Started

### Requirements

- .NET 8.0 or later
- A dependency injection container that can provide the services registered by
  `AddPdfTemplateService`
- On Linux, the SkiaSharp native Linux assets package

The package is marked trim-compatible and depends on SkiaSharp,
`Microsoft.Extensions.DependencyInjection.Abstractions` and `X39.Util`.
Issues are tracked in the GitHub repository at
<https://github.com/X39/X39.Solutions.PdfTemplate/issues>.

### Install

Install the [NuGet package](https://www.nuget.org/packages/X39.Solutions.PdfTemplate/)
into your project:

```shell
dotnet add package X39.Solutions.PdfTemplate
```

On Linux, also install the
[SkiaSharp native Linux assets](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux):

```shell
dotnet add package SkiaSharp.NativeAssets.Linux
```

### Create a template

Templates are XML documents. The root element name is flexible, but `template` is used
throughout this README:

```xml
<template>
    <body>
        <text>Hello, world!</text>
    </body>
</template>
```

### Generate a PDF

Register the library services at startup:

```csharp
services.AddPdfTemplateService();
```

Then resolve the registered generator from your application `IServiceProvider` and render:

```csharp
using System.Globalization;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate;

// IServiceProvider serviceProvider
// Stream xmlTemplateStream
await using var scope = serviceProvider.CreateAsyncScope();
var generator = scope.ServiceProvider.GetRequiredService<Generator>();

using var reader = XmlReader.Create(xmlTemplateStream);
using var pdfStream = new MemoryStream();

await generator.GeneratePdfAsync(pdfStream, reader, CultureInfo.CurrentUICulture);

// pdfStream now contains the PDF
```

`AddPdfTemplateService` registers the supporting services, `Generator`, and the built-in controls and transformers.
Custom controls, transformers and functions can be registered through the setup builder before building the service
provider.

### Useful next steps

- Set template variables with `generator.TemplateData.SetVariable("Name", value)`.
- Add custom functions with `services.AddPdfTemplateService((builder) => builder.AddFunction<MyFunction>())`.
- Add custom controls with `services.AddPdfTemplateService((builder) => builder.AddControl<TControl>())`.
- Add custom transformers with `services.AddPdfTemplateService((builder) => builder.AddTransformer<TTransformer>())`.
- Configure document-level options such as margin through `DocumentOptions`.
- Use the samples in `test/X39.Solutions.PdfTemplate.Test/Samples` as executable examples.

## Core concepts

| Concept | What it is used for |
|---------|----------------------|
| Template | XML document that describes the generated output. |
| Control | XML element that measures, arranges and renders content, for example `text`, `table` or `image`. |
| Variable | Value supplied by application code and read in a template with `@VariableName` or property access expressions. |
| Function | Reusable .NET logic exposed to templates through calls such as `@myFunction()`. |
| Transformer | Preprocessor block that can conditionally include, repeat or rewrite XML before rendering. |
| Resource resolver | Service used by controls, currently mainly `image`, to load external resources. |

## Template structure

A template is an XML document with a lightweight preprocessor.
It can contain the following top-level sections:

```xml
<!-- The root node name is ignored and can be changed to your preference. -->
<template>
    <background>
        <!--
           Background is rendered on every page and can be used to add fold lines,
           watermarks or other page-wide content.
           All background contents are measured against the first page
           (to clarify: the available space only accounts for the first page,
            it is rendered on all pages, but only ever the first page of
            the contents).
           Background also ignores page margin and padding configuration,
           working with the initial size.
        -->
    </background>
    <header>
        <!--
           Header defines repeated content at the top of every page.
           It may use up to 25% of the page height after margin and padding.
        -->
    </header>
    <body>
        <!--
           Body contains the main document contents.
           It is rendered across as many pages as required.
           Depending on the header/footer sections, the available size on the page
           may be 100% or 50%, minus page margin and padding.
        -->
    </body>
    <footer>
        <!--
           Footer defines repeated content at the bottom of every page.
           It may use up to 25% of the page height after margin and padding.
        -->
    </footer>
    <foreground>
      <!--
         Foreground is rendered on every page and can be used for overlays.
         All foreground contents are measured against the first page
         (to clarify: the available space only accounts for the first page,
          it is rendered on all pages, but only ever the first page of
          the contents).
         Foreground also ignores page margin and padding configuration,
         working with the initial size.
      -->
    </foreground>
    <areas>
      <area left="10cm" right="10cm" height="10cm" top="10cm">
        <!-- See "About areas". -->
      </area>
    </areas>
</template>
```

The template automatically references the default XML namespace
`X39.Solutions.PdfTemplate.Controls`, allowing the use of its controls without
requiring an `xmlns` prefix.
This means the actual root node for the example template appears to the library as
`<template xmlns="X39.Solutions.PdfTemplate.Controls">`.
This implicit reference simplifies template creation for end-users by
omitting the need for the `xmlns` attribute.
However, if a template overrides the default `xmlns`,
you must use a different prefix for the controls,
such as `xmlns:prefix="X39.Solutions.PdfTemplate.Controls"`.
For instance, `<text>` would then be written as `<prefix:text>`.



### About `areas`

The `areas` section renders content at a designated page position.
Each area is identified by coordinates on an `area` node and ignores the normal margin rules.

Areas are rendered above body but below foreground.

It has the following attributes:

| Attribute | Description                                                                                                                         | Values              | Default |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------|---------------------|---------|
| `Width`   | The width of the area.                                                                                                              | [`Length`](#length) | `0`     |
| `Height`  | The height of the area.                                                                                                             | [`Length`](#length) | `0`     |
| `Left`    | The distance from the left side of a page for the area. If both `Left` and `Right` values are provided, `Width` will be ignored.    | [`Length`](#length) | `0`     |
| `Top`     | The distance from the top side of a page for the area. If both `Top` and `Bottom` values are provided, `Height` will be ignored.    | [`Length`](#length) | `0`     |
| `Right`   | The distance from the right side of a page for the area. If both `Left` and `Right` values are provided, `Width` will be ignored.   | [`Length`](#length) | `null`  |
| `Bottom`  | The distance from the bottom side of a page for the area. If both `Top` and `Bottom` values are provided, `Height` will be ignored. | [`Length`](#length) | `null`  |



## Integration

### Functions

The library supports custom functions for use in templates and comes with two built-in functions: `allFunctions()`
and `allVariables()`.
These functions are used to list all available functions and variables, respectively.

To create your own function, derive a class from the `IFunction` interface and implement the `ExecuteAsync` method.
Register the function in dependency injection before resolving a generator. Here is an example:

```csharp
public class MyFunction : IFunction
{
    public MyFunction(ISomeDependency someDependency) // Pass any dependencies when constructing the function.
    {
        // ...
    }

    public string Name => "my"; // The name of your function.
    public int Arguments => 0; // The number of arguments your function takes.
    public bool IsVariadic => false; // Whether your function takes a variable number of arguments. If true, `Arguments` is the minimum number of arguments.

    public ValueTask<object?> ExecuteAsync(
        CultureInfo cultureInfo,
        object?[] arguments,
        CancellationToken cancellationToken = default)
    {
        // Execute your function here.
        return ValueTask.FromResult<object?>("Hello, world!");
    }
}
```

```csharp
services.AddPdfTemplateService((builder) => builder.AddFunction<MyFunction>());
```

### Variables

You can use variables in your templates to access .NET objects. To do this, you just need to add the variable to
the `Generator` instance:

```csharp
generator.TemplateData.SetVariable("MyVariable", "Hello World!");
```

### Template data types

The library uses a variety of data types to represent values in the template.
The following list gives an overview of the data types template authors will usually see.

#### `Orientation`

The `Orientation` enum is used to specify the orientation of a control.

It can have one of the following values:

| Value        | Description                           |
|--------------|---------------------------------------|
| `Horizontal` | The control is oriented horizontally. |
| `Vertical`   | The control is oriented vertically.   |

#### `Length`

A `Length` is a value that represents a length.

It can have one of the following units:

| Unit   | Description                             | Example |
|--------|-----------------------------------------|---------|
| `px`   | The length is in pixels.                | `100px` |
| `pt`   | The length is in points (font size).    | `12pt`  |
| `cm`   | The length is in centimeters.           | `1cm`   |
| `mm`   | The length is in millimeters.           | `10mm`  |
| `in`   | The length is in inches.                | `1in`   |
| `%`    | The length is in percent.               | `100%`  |
| `auto` | The length is automatically determined. | `auto`  |

#### `Color`

A `Color` is a value that represents a color.

It can have one of the following formats:

| Format      | Description                      | Example     |
|-------------|----------------------------------|-------------|
| `#RGB`      | The color is in RGB format.      | `#F00`      |
| `#RGBA`     | The color is in RGBA format.     | `#F00F`     |
| `#RRGGBB`   | The color is in RRGGBB format.   | `#FF0000`   |
| `#RRGGBBAA` | The color is in RRGGBBAA format. | `#FF0000FF` |
| color name  | The color is a named color.      | `red`       |

#### `Thickness`

A `Thickness` is a value that represents a thickness.
It consists of four [`Length`](#length)s, one for each side.

It can have one of the following formats:

| Format                | Description                                                                                                                                                     | Example           |
|-----------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------|
| all                   | All sides have the same thickness.                                                                                                                              | `1px`             |
| horizontal vertical   | The horizontal sides have the first thickness, the vertical sides have the second thickness.                                                                    | `1px 2px`         |
| left top right bottom | The left side has the first thickness, the top side has the second thickness, the right side has the third thickness, the bottom side has the fourth thickness. | `1px 2px 3px 4px` |

### Controls

The library supports a variety of controls for creating complex layouts. Each control is represented by a class in
the `X39.Solutions.PdfTemplate.Controls` namespace.

All controls inherit the following attributes:

| Attribute | Description | Values | Default |
|-----------|-------------|--------|---------|
| `Margin`  | The outer spacing around the control. | Any [`Thickness`](#thickness) | `0` |
| `Padding` | The inner spacing inside the control. | Any [`Thickness`](#thickness) | `0` |
| `Clip`    | Whether the control should clip content to its arranged bounds. | `true` or `false` | `true` |

All alignable controls additionally support:

| Attribute             | Description                              | Values                                   | Default   |
|-----------------------|------------------------------------------|------------------------------------------|-----------|
| `HorizontalAlignment` | Horizontal placement within the parent.  | `Left`, `Center`, `Right`, `Stretch`     | `Stretch` |
| `VerticalAlignment`   | Vertical placement within the parent.    | `Top`, `Center`, `Bottom`, `Stretch`     | `Stretch` |

#### Creating your own control

To create your own, derive a class from the `Control` base class and override the `Render` method. Here is an example:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X39.Solutions.PdfTemplate.Controls;

namespace MyControls;
[Control("MyControls")] // The namespace of your control.
public class MyControl : Control
{
    public MyControl(ISomeDependency someDependency) // You can inject dependencies via the constructor.
    {
        // ...
    }
    
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        // The size your control wants to be, given the remaining space.
        return new Size(100, 100);
    }

    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        // The size your control actually is, given the remaining space.
        return new Size(100, 100);
    }


    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        // Render your control here.
        return Size.Zero;
    }
}
```

Later in your service registration, add the control:

```csharp
services.AddPdfTemplateService((builder) => builder.AddControl<MyControl>());
```

You can now use the control in your XML templates (note the namespace import at the top):

```xml 

<template xmlns:my="MyControls">
    <body>
        <my:MyControl/>
    </body>
</template>
```

**Warning:** The template has an implicit default namespace.
If you change the default namespace (`xmlns="MyControls"`) instead of defining your own prefix,
you must reference the default controls and the template layout through that namespace as well.

#### `text`

The `text` control renders text. It can be used as follows:

```xml

<template>
    <body>
        <text>Hello, world!</text>
    </body>
</template>
```

You may derive from the `TextBaseControl` class to create your own text-based controls.

The `text` control supports the following attributes:

| Attribute         | Description                                                          | Values                                                                            | Default                                                      |
|-------------------|----------------------------------------------------------------------|-----------------------------------------------------------------------------------|--------------------------------------------------------------|
| `Foreground`      | The foreground color of the text. See [`Color`](#color) for details. | Any color                                                                         | `#000000`                                                    |
| `FontSize`        | The font size of the text in points.                                 | A positive number                                                                 | `12`                                                         |
| `LineHeight`      | The line height of the text in points, relative to the font size.    | A positive number                                                                 | `1.0`                                                        |
| `Scale`           | The scale of the text.                                               | A positive number                                                                 | `1.0`                                                        |
| `Rotation`        | The rotation of the text in degrees.                                 | A number                                                                          | `0`                                                          |
| `StrokeThickness` | The thickness of the text stroke in points.                          | A positive number                                                                 | `1`                                                          |
| `LetterSpacing`   | The letter spacing/font width.                                       | A number                                                                          | `0`                                                          |
| `Weight`          | The weight of the font.                                              | Any positive number or the common names without a `-` (`thin`, `extraLight`, ...) | `0`                                                          |
| `Style`           | The style of the font.                                               | `normal`, `italic`, `oblique`, `upright`                                          | `normal`                                                     |
| `FontFamily`      | The font family of the text.                                         | A font family name or a comma-separated list of font family names                 | Windows: `Arial`, Any other system: OS-specific default font |
| `Text`            | The text to render. Also accepted as XML Content.                    | Any text                                                                          | `""`                                                         |

#### `border`

A border control can be used to draw a border around other controls
or to add a background color to a control.

The `border` control supports the following attributes:

| Attribute    | Description                                                             | Values                        | Default           |
|--------------|-------------------------------------------------------------------------|-------------------------------|-------------------|
| `Thickness`  | The thickness of the border. See [`Thickness`](#thickness) for details. | Any [`Thickness`](#thickness) | `0`               |
| `Background` | The background color of the border. See [`Color`](#color) for details.  | Any [`Color`](#color)         | `transparent`     |
| `Color`      | The color of the border. See [`Color`](#color) for details.             | Any [`Color`](#color)         | `transparent`     |

Usage:

```xml

<template>
    <body>
        <border thickness="1pt 1pt 1pt 1pt" background="#FF0000" color="#00FF00">
            <text>Hello, world!</text>
        </border>
    </body>
</template>
```

#### `image`

The `image` control renders images.

It supports the following attributes:

| Attribute | Description                                                                                                                         | Values                                                                       | Default                     |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------|-----------------------------|
| `Source`  | The source of the image. By default, the source is interpreted as Base64. Use a custom `IResourceResolver` to change this behavior. | Any URI, see [`IResourceResolver`](#iresourceresolver) for different formats | `""`                        |
| `Width`   | The width of the image in [`Length`](#length).                                                                                      | Any [`Length`](#length)                                                      | `auto`                      |
| `Height`  | The height of the image in [`Length`](#length).                                                                                     | Any [`Length`](#length)                                                      | `auto`                      |

Usage:

```xml

<template>
    <body>
        <image source="data:image/png;base64,..."/>
    </body>
</template>
```

#### `line`

The `line` control renders a simple line.

It supports the following attributes:

| Attribute     | Description                                                                 | Values                        | Default      |
|---------------|-----------------------------------------------------------------------------|-------------------------------|--------------|
| `Thickness`   | The thickness of the line. See [`Length`](#length) for details.             | Any [`Length`](#length)       | `auto`       |
| `Color`       | The color of the line. See [`Color`](#color) for details.                   | Any [`Color`](#color)         | `black`      |
| `Length`      | The length of the line in [`Length`](#length).                              | Any [`Length`](#length)       | `auto`       |
| `Orientation` | The orientation of the line. See [`Orientation`](#orientation) for details. | [`Orientation`](#orientation) | `Horizontal` |

Usage:

```xml

<template>
    <body>
        <line thickness="1pt" color="#FF0000" length="100%" orientation="Horizontal"/>
    </body>
</template>
```

#### `pageNumber`

The `pageNumber` control renders the current page number or the total number of pages or both.

It supports the following attributes:

In addition to these page-number-specific attributes, `pageNumber` supports the same text styling attributes as
[`text`](#text).

| Attribute   | Description                                                                               | Values                                             | Default   |
|-------------|-------------------------------------------------------------------------------------------|----------------------------------------------------|-----------|
| `Mode`      | The mode of the page number. Can be `Current`, `Total`, `CurrentTotal` or `TotalCurrent`. | `Current`, `Total`, `CurrentTotal`, `TotalCurrent` | `Current` |
| `Prefix`    | The prefix of the page number.                                                            | Any text                                           | `""`      |
| `Suffix`    | The suffix of the page number.                                                            | Any text                                           | `""`      |
| `Delimiter` | The delimiter between the current and total page number.                                  | Any text                                           | `""`      |

Usage:

```xml

<template>
    <body>
        <pageNumber mode="CurrentTotal" prefix="Page " delimiter=" of "/>
    </body>
</template>
```

#### `table`

The `table` control renders tables.
It is used in conjunction with the [`th`](#th), [`tr`](#tr) and [`td`](#td) controls.

It has no table-specific attributes.

Usage:

```xml

<template>
    <body>
        <table>
            <th>
                <td>Header 1</td>
                <td>Header 2</td>
            </th>
            <tr>
                <td>Cell 1</td>
                <td>Cell 2</td>
            </tr>
        </table>
    </body>
</template>
```

##### `th`

The `th` control is used to define the table headers.
A table header is repeated on every page if the table spans multiple pages.

It has no header-specific attributes.

See [`table`](#table) for usage.

##### `tr`

The `tr` control is used to define the table rows.
A table row cannot span multiple pages, but total rows will be broken across pages.

It has no row-specific attributes.

See [`table`](#table) for usage.

##### `td`

The `td` control is used to define the table cells.

It has the following attributes:

| Attribute    | Description                                                             | Values                                      | Default |
|--------------|-------------------------------------------------------------------------|---------------------------------------------|---------|
| `ColumnSpan` | The number of columns the cell spans.                                   | Any positive number                         | `1`     |
| `Width`      | The width of the cell in either [`Length`](#length) or parts (for example `1*`). | Any [`Length`](#length) or parts (for example `1*`) | `auto`  |

See [`table`](#table) for usage.

#### `chart`

The `chart` control is a container for chart visualizations.
It can contain one or more chart types: [`lineChart`](#linechart), [`barChart`](#barchart), or [`pieChart`](#piechart).

It has no chart-container-specific attributes.

Usage:

```xml
<template>
    <body>
        <chart>
            <lineChart height="300px" title="Sales Data">
                <data x="0" y="100" label="Jan" />
                <data x="1" y="150" label="Feb" />
                <data x="2" y="120" label="Mar" />
            </lineChart>
        </chart>
    </body>
</template>
```

##### `lineChart`

The `lineChart` control renders a line graph with connected data points.

It has the following attributes:

| Attribute         | Description                                | Values                  | Default         |
|-------------------|--------------------------------------------|-------------------------|-----------------|
| `width`           | The width of the chart                     | Any [`Length`](#length) | `100%`          |
| `height`          | The height of the chart                    | Any [`Length`](#length) | `300px`         |
| `title`           | The chart title                            | Any string              | empty           |
| `show-grid`       | Whether to show grid lines                 | `true` or `false`       | `true`          |
| `grid-color`      | Color of the grid lines                    | Any [`Color`](#color)   | `#CCCCCCFF`     |
| `axis-color`      | Color of the axes                          | Any [`Color`](#color)   | `#000000FF`     |
| `show-x-axis`     | Whether to show the X-axis                 | `true` or `false`       | `true`          |
| `show-y-axis`     | Whether to show the Y-axis                 | `true` or `false`       | `true`          |
| `x-axis-label`    | Label for the X-axis                       | Any string              | empty           |
| `y-axis-label`    | Label for the Y-axis                       | Any string              | empty           |
| `line-thickness`  | Thickness of the line                      | Any [`Length`](#length) | `2px`           |
| `line-color`      | Color of the line                          | Any [`Color`](#color)   | Default palette |
| `show-points`     | Whether to show point markers              | `true` or `false`       | `true`          |
| `point-size`      | Size of point markers                      | Any number              | `4`             |

See [`chart`](#chart) for usage.

##### `barChart`

The `barChart` control renders vertical or horizontal bar charts.

It has the following attributes:

| Attribute     | Description                                      | Values                           | Default         |
|---------------|--------------------------------------------------|----------------------------------|-----------------|
| `width`       | The width of the chart                           | Any [`Length`](#length)          | `100%`          |
| `height`      | The height of the chart                          | Any [`Length`](#length)          | `300px`         |
| `title`       | The chart title                                  | Any string                       | empty           |
| `show-grid`   | Whether to show grid lines                       | `true` or `false`                | `true`          |
| `grid-color`  | Color of the grid lines                          | Any [`Color`](#color)            | `#CCCCCCFF`     |
| `axis-color`  | Color of the axes                                | Any [`Color`](#color)            | `#000000FF`     |
| `show-x-axis` | Whether to show the X-axis                       | `true` or `false`                | `true`          |
| `show-y-axis` | Whether to show the Y-axis                       | `true` or `false`                | `true`          |
| `x-axis-label`| Label for the X-axis                             | Any string                       | empty           |
| `y-axis-label`| Label for the Y-axis                             | Any string                       | empty           |
| `orientation` | Orientation of the bars                          | `Vertical` or `Horizontal`       | `Vertical`      |
| `bar-width`   | Width of each bar (0 = auto-calculated)          | Any [`Length`](#length)          | `0px`           |
| `bar-spacing` | Spacing between bars                             | Any [`Length`](#length) (percent)| `10%`           |
| `bar-color`   | Color of the bars                                | Any [`Color`](#color)            | Default palette |

Usage:

```xml
<template>
    <body>
        <chart>
            <barChart height="300px" title="Revenue by Quarter">
                <data x="0" y="50" label="Q1" />
                <data x="1" y="75" label="Q2" />
                <data x="2" y="60" label="Q3" />
                <data x="3" y="90" label="Q4" />
            </barChart>
        </chart>
    </body>
</template>
```

##### `pieChart`

The `pieChart` control renders pie charts and donut charts. Only the `y` value is used for pie charts; the `x` value is ignored.

It has the following attributes:

| Attribute          | Description                                      | Values                  | Default         |
|--------------------|--------------------------------------------------|-------------------------|-----------------|
| `width`            | The width of the chart                           | Any [`Length`](#length) | `100%`          |
| `height`           | The height of the chart                          | Any [`Length`](#length) | `300px`         |
| `title`            | The chart title                                  | Any string              | empty           |
| `start-angle`      | Starting angle in degrees (0 = top)              | Any number              | `0`             |
| `inner-radius`     | Inner radius for donut charts (0 = pie)          | Any [`Length`](#length) | `0%`            |
| `show-percentages` | Whether to show percentage labels                | `true` or `false`       | `true`          |
| `show-labels`      | Whether to show data point labels                | `true` or `false`       | `true`          |

Usage:

```xml
<template>
    <body>
        <chart>
            <pieChart height="400px" title="Market Share">
                <data y="35" label="Product A" />
                <data y="25" label="Product B" />
                <data y="20" label="Product C" />
                <data y="20" label="Product D" />
            </pieChart>
        </chart>
    </body>
</template>
```

For a donut chart, set the `inner-radius` attribute:

```xml
<pieChart height="400px" title="Donut Chart" inner-radius="50%">
    <data y="40" label="Category 1" />
    <data y="30" label="Category 2" />
    <data y="30" label="Category 3" />
</pieChart>
```

##### `data`

The `data` control defines a single data point for a chart. It is used within chart controls like [`lineChart`](#linechart), [`barChart`](#barchart), or [`pieChart`](#piechart).

It has the following attributes:

| Attribute  | Description                                          | Values              | Default |
|------------|------------------------------------------------------|---------------------|---------|
| `x`        | The X-coordinate value                               | Any number          | `0`     |
| `y`        | The Y-coordinate value                               | Any number          | `0`     |
| `x-label`  | Label for the X-axis at this data point              | Any string          | empty   |
| `y-label`  | Label for the Y-axis at this data point              | Any string          | empty   |
| `label`    | Label for this specific data point                   | Any string          | empty   |
| `color`    | Color for this specific data point                   | Any [`Color`](#color)| Default palette |

See [`chart`](#chart) for usage.

### Transformers

Transformers are used to transform the XML template before it is rendered.
This expands a template and enriches it with data from C#.

#### Creating your own transformer

A transformer, at its core, is a class that implements the `ITransformer` interface.
It allows manipulating every node in its `{...}` block and hence is a very powerful tool regarding template
manipulation.

To create your own transformer, derive a class from the `ITransformer` interface and implement the `TransformAsync` method:

```csharp
public class MyTransformer : ITransformer
{
    public string Name => "MyTransformer"; // The name of your transformer.

    public async IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Return the transformed nodes.
    }
}
```

Afterwards, add the transformer to the service collection:

```csharp
services.AddPdfTemplateService((builder) => builder.AddTransformer<MyTransformer>());
```

##### Evaluating user data

While building your transformer, you may have to evaluate user data, for example to resolve a function call.
This can be done by utilizing the following function of the passed `ITemplateData` interface:

```csharp
// interface X39.Solutions.PdfTemplate.ITemplateData
ValueTask<object?> EvaluateAsync(
        CultureInfo cultureInfo,
        string expression,
        CancellationToken cancellationToken = default)
```

##### Introducing new variables or changing existing

A core feature of a transformer is dealing with variables. The library exposes
core functionality for variable interaction via the passed `ITemplateData`.

While you can modify all variables immediately, it is recommended that you create
a variable scope first:

```csharp
using var scope = templateData.Scope("scopeName");
```

This will ensure that your variable changes are only applied to the nodes returned
by the transformer.

To then set a variable, use `templateData.SetVariable(variable, value);`
Do note that while you can certainly receive variable values using `templateData.GetVariable(value);`,
chances are that you are more interested in [evaluating the user data](#evaluating-user-data) to also
accept functions.

Transformers may also share transformer-specific state through
`templateData.GetTransformerData<T>(name)` and `templateData.SetTransformerData(name, value)`.
The state is keyed by both type and name and lives for the lifetime of the `ITemplateData` instance.

#### `alternate`

The `alternate` transformer alternates between values, making it possible to create a table with
alternating row colors.
It can be used as follows:

```xml

<template>
    <body>
        <!-- Every time we call @alternate with just on and a list of values, the value list will be progressed by one and put into @value -->
        <!-- If the value list is exhausted, it will start over -->
        @alternate on value with ["one", "two"] {
        <!-- @value is "one" -->
        <text>@value</text>
        }
        @alternate repeat on value {
        <!-- @value is "one" -->
        <text>@value</text>
        }
        @alternate on value with ["one", "two"] {
        <!-- @value is "two" -->
        <text>@value</text>
        }
        @alternate on value {
        <!-- @value is "one" -->
        <text>@value</text>
        }
        @alternate on value {
        <!-- @value is "two" -->
        <text>@value</text>
        }
        <!-- When calling @alternate with a different list, the alternate will be reset -->
        @alternate on value with ["three"] {
        <!-- @value is "three" -->
        <text>@value</text>
        }
    </body>
</template>
```

#### `var`

The `var` transformer introduces new variables in the XML template to cache a result or
to simply make access to a certain, commonly used value more easy on the user.
It can be used as follows:

```xml

<template>
    <body>
        @var text = someFunc() {
        <text>@text</text>
        }
        @var text = someFunc(), text2 = moreFunc() {
        <text>@text</text>
        <text>@text2</text>
        }
    </body>
</template>
```

#### `if`

The `if` transformer conditionally includes parts of the template.
There is no `else` clause, but you can use `@if` multiple times to achieve the same effect.
It can be used as follows:

```xml

<template>
    <body>
        @if 1 == 1 {
        <text>Numerous operators are supported, including &gt;, &lt;, &gt;=, &lt;=, ==, !=, ===, !== and "in".</text>
        }
        @if false {
        <text>Never included</text>
        }
        @if true {
        <text>Always included</text>
        }
    </body>
</template>
```

#### `for`

The `for` transformer repeats parts of the template.

<!-- Regex: "\A\s*(?<variable>[a-zA-Z][a-zA-Z0-9_]*)\s+from\s+(?<from>.+?)\s+to\s+(?<to>.+?)(\s+step\s+(?<step>.+?))?\s*\z -->

```xml

<template>
    <body>
        @for i from 0 to 10 {
        <!-- @i is 0, 1, 2, ..., 9 -->
        <text>@i</text>
        }
        @for i from 0 to 10 step 2 {
        <!-- @i is 0, 2, 4, ..., 8 -->
        <text>@i</text>
        }
        @for i from 10 to 0 step -2 {
        <!-- @i is 10, 8, 6, ..., 2 -->
        <text>@i</text>
        }
    </body>
</template>
```

#### `foreach`

The `foreach` transformer repeats parts of the template for each element in a list.

```csharp
generator.TemplateData.SetVariable("MyList", new[] { "one", "two", "three" });
```

```xml

<template>
    <body>
        @foreach item in @MyList {
        <!-- @item is "one", "two", "three" -->
        <text>@item</text>
        }
        @foreach item in @MyList with index {
        <!-- @item is "one", "two", "three" -->
        <!-- @index is 0, 1, 2 -->
        <text>@item</text>
        }
    </body>
</template>
```

### Interfaces

This section contains the interfaces you may encounter when extending the library.
Not every public interface is intended to be implemented by consumers.
The usual extension points are custom controls, content controls, functions, transformers,
parameter converters and resource resolvers.

#### `IDrawableCanvas`

`IDrawableCanvas` is the low-level drawing abstraction used by controls while rendering.
It exists to isolate controls from the concrete SkiaSharp canvas operations.
External code should generally not implement this interface.
There is currently no supported alternate rendering backend, so a custom implementation would only be useful
for specialized tests or experimentation.

#### `IDeferredCanvas`

`IDeferredCanvas` is the canvas type passed to `IControl.Render`.
It records draw operations first and replays them later when the actual page canvas is available.
This is what makes page-dependent rendering possible: controls can call `Defer` when they need values such as the
current page number or total page count.
External code should use this interface inside custom controls, but should not implement it.
See also: [`IImmediateCanvas`](#iimmediatecanvas)

#### `IImmediateCanvas`

`IImmediateCanvas` represents the real render-time canvas exposed inside `IDeferredCanvas.Defer`.
It provides page-specific information such as `PageNumber` and `TotalPages`.
Use it only when a custom control cannot know the final value during normal deferred rendering.
Like the other canvas interfaces, it is an internal rendering abstraction and should not be implemented by consumers.

#### `IControl`

`IControl` is the base contract for renderable controls.
It exposes the measure, arrange and render phases used by the generator.
You can implement it directly, but most custom controls should derive from the `Control` base class instead.
The base class already handles common XML parameters such as `Margin`, `Padding`, `Clip`,
arrangement state and canvas state management.
Implement `IControl` directly only when you need full control over the layout lifecycle.

#### `IControlFactory`

`IControlFactory` creates control instances from template nodes.
The default implementation resolves the registered control type, constructs a fresh control instance and applies XML
parameters.
Most consumers should register controls with `AddPdfTemplateService((builder) => builder.AddControl<TControl>())`
instead of replacing the factory.
Replace it only when you need custom activation behavior such as diagnostics, pooling or another creation strategy.

#### `IContentControl`

`IContentControl` extends `IControl` for controls that can contain child controls.
It is relevant when creating container controls such as custom tables, panels or layout primitives.
Implement `CanAdd` to restrict which child control types are valid, and use the collection members to store children.
Most custom container controls should derive from `AlignableContentControl` instead of implementing this interface
from scratch.

#### `IFunction`

`IFunction` is the intended extension point for values that should be calculated from template expressions,
for example `@customerName()` or `@formatCurrency(@total)`.
Implement it when template authors need reusable logic or data access that should not be encoded directly in XML.
Functions are registered through dependency injection as `IFunction` services.
Use `IsVariadic` only when the function can accept more arguments than the fixed `Arguments` count.

#### `IInitializeControlAsync`

`IInitializeControlAsync` is an optional control hook.
Implement it when a control needs asynchronous setup after XML construction but before measure, arrange and render.
The default `image` control uses it to resolve and decode image resources.
This is the right place for request-scoped resource lookup, metadata loading or other preparation that should happen
once per generated document.
Keep heavy repeated work out of `Measure` and `Render`.

#### `IParameterConverter`

`IParameterConverter<T>` is an opt-in extension point for custom control parameter parsing.
Use it when a `[Parameter]` property cannot be parsed by the normal .NET type converter or `IParsable<T>` path.
Attach the converter through `ParameterAttribute.Converter`.
Converters are useful for compact XML syntaxes, formatted values or domain-specific value objects.

#### `ITemplateData`

`ITemplateData` is the document-generation data context.
It stores variables, registered functions, expression evaluation and transformer-specific shared data.
Custom functions and transformers use this interface heavily.
Application code usually interacts with it through `generator.TemplateData` to set variables or register functions.
You normally should not replace the implementation; use scopes when a transformer needs temporary variables.

#### `ITransformer`

`ITransformer` is the intended extension point for XML preprocessing blocks.
Built-in examples are `@if`, `@for`, `@foreach`, `@var` and `@alternate`.
Implement a transformer when you need to add, remove or repeat XML nodes before controls are constructed.
Transformers are powerful and operate on raw template nodes, so prefer functions for simple value substitution.
Use `ITemplateData.Scope` when introducing variables so changes stay limited to the transformed nodes.

#### `IPropertyAccessCache`

`IPropertyAccessCache` is infrastructure for expression evaluation.
It caches property-access delegates so repeated template expressions can read object properties efficiently.
It is registered by `AddPdfTemplateService` and is not a normal customization point.
Replace it only if you are changing expression-evaluation behavior at library-infrastructure level.

#### `ITextService`

`ITextService` is infrastructure for measuring and drawing text used by `TextBaseControl`.
Custom text controls should usually consume the existing service by deriving from `TextBaseControl`.
Replacing the service changes text layout and rendering behavior globally, so it is an advanced customization point
rather than a typical application extension.

#### `IResourceResolver`

The resource resolver is responsible for resolving resources when controls need them.
The default controls library only uses it for the `image` control.

This is the primary extension point for custom image loading.
The default implementation provided will treat all input as base64 encoded images.
Override it when images should come from a database, object storage, a trusted file system location,
authenticated HTTP requests or another application-specific source.
`DocumentOptions.Context` is passed unchanged into the resolver, allowing custom
resolvers to associate a resource lookup with the current print request.

```csharp
public sealed record PrintRequestContext(Guid RequestId);

public sealed class MyResourceResolver : IResourceResolver
{
    public ValueTask<byte[]> ResolveImageAsync(
        string source,
        object? context,
        CancellationToken cancellationToken = default)
    {
        var requestContext = context as PrintRequestContext;
        // Use requestContext?.RequestId to route or audit this lookup.
        return LoadImageAsync(source, requestContext, cancellationToken);
    }
}

await generator.GeneratePdfAsync(
    outputStream,
    reader,
    CultureInfo.InvariantCulture,
    new DocumentOptions
    {
        Context = new PrintRequestContext(Guid.NewGuid()),
    },
    cancellationToken);
```

## Building and Testing

This project uses GitHub Actions for continuous integration.
The pull-request workflow is defined in `.github/workflows/run-dotnet-tests.yml`.
The publish workflow is defined in `.github/workflows/main.yml` and restores dependencies,
builds, runs tests, packs the library and publishes the NuGet package.

To restore and build locally, use the following commands:

```shell
dotnet restore
dotnet build --no-restore
```

To run the tests locally, use the following command:

```shell
dotnet test --framework net8.0 --no-build --verbosity normal
```

To create a local package, use:

```shell
dotnet pack --configuration Release
```

## Documentation status

The public API is documented in code, and this README currently serves as the main user-facing guide.
Executable samples live in `test/X39.Solutions.PdfTemplate.Test/Samples`, including a README-style invoice sample.

A dedicated documentation site is still missing.
Contributions that improve end-user documentation, examples or a future documentation site are welcome.
If you want to add a larger documentation system such as JetBrains Writerside, please open a discussion or pull request
so the structure can be reviewed before it grows too large.

## Contributing

Contributions are welcome!
Please submit a pull request or create a discussion to discuss any changes you wish to make.

### Code of Conduct

Be excellent to each other.

### Contributors Agreement

First of all, thank you for your interest in contributing to this project!
Please add yourself to the list of contributors in the [CONTRIBUTORS](CONTRIBUTORS.md) file when submitting your
first pull request.
Also, please always add the following to your pull request:

```
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

This is necessary to ensure that this project can be licensed under the GNU Lesser General Public License v3.0 and
that a license change is possible in the future if necessary (e.g., to a more permissive license).
It also ensures that I can remove your contribution if necessary (e.g., because it violates third-party rights) and
that I can change your contribution if necessary (e.g., to fix a typo, change implementation details, or improve
performance).
It also shields me and every user of this project from any liability regarding your contribution by deflecting any
potential liability caused by your contribution to you (e.g., if your contribution violates the rights of your
employer).
Feel free to discuss this agreement in the discussions section of this repository. I am open to changes here as long as
they do not open me or any other user of this project to any liability due to a **malicious contribution**.

### Additional controls

If you have created an additional control which is not depending on any other library, feel free to submit a pull
request.
If your control depends on another library, please create a separate repository and create a pull request to add it to a
list in this README.md.
This way, the core library can stay as small as possible and users can decide which controls they want to use.
Feel free to ask for help regarding publishing your control as a separate NuGet package in the discussions section of
this repository.

## Semantic Versioning

This library follows the principles of [Semantic Versioning](https://semver.org/).
Version changes are intended to communicate compatibility:

| Change | Meaning |
|--------|---------|
| Patch | Backwards-compatible bug fixes or small internal changes. |
| Minor | Backwards-compatible features or additions. |
| Major | Breaking changes. |

## License

This project is licensed under the GNU Lesser General Public License v3.0. See the [LICENSE](LICENSE) file for details.
