***NOTE FOR [NuGet.org](https://www.nuget.org/packages/X39.Solutions.PdfTemplate):***
*This readme contains comments in the XML which is not rendered by the NuGet markdown parser.
Use [GitHub](https://github.com/X39/X39.Solutions.PdfTemplate) for best reading experience*

![A sample output for reference](https://raw.githubusercontent.com/X39/X39.Solutions.PdfTemplate/master/.github/media/sample.png)

<!-- TOC -->
* [X39.Solutions.PdfTemplate](#x39solutionspdftemplate)
  * [Semantic Versioning](#semantic-versioning)
  * [Getting Started](#getting-started)
  * [Template structure](#template-structure)
    * [About `areas`](#about-areas)
  * [Integration](#integration)
    * [Functions](#functions)
    * [Variables](#variables)
    * [End-User facing data types](#end-user-facing-data-types)
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
      * [`IContentControl`](#icontentcontrol)
      * [`IFunction`](#ifunction)
      * [`IInitializeControlAsync`](#iinitializecontrolasync)
      * [`IParameterConverter`](#iparameterconverter)
      * [`ITemplateData`](#itemplatedata)
      * [`ITransformer`](#itransformer)
      * [`IAddControls`](#iaddcontrols)
      * [`IAddTransformers`](#iaddtransformers)
      * [`IPropertyAccessCache`](#ipropertyaccesscache)
      * [`ITextService`](#itextservice)
      * [`IResourceResolver`](#iresourceresolver)
  * [Building and Testing](#building-and-testing)
  * [Proper documentation for End-Users](#proper-documentation-for-end-users)
  * [Contributing](#contributing)
    * [Code of Conduct](#code-of-conduct)
    * [Contributors Agreement](#contributors-agreement)
    * [Additional controls](#additional-controls)
  * [License](#license)
<!-- TOC -->

# X39.Solutions.PdfTemplate

This library provides a way to generate PDF documents (and images) from XML templates.
It uses SkiaSharp for rendering and supports a variety of controls for creating complex layouts.
You can easily integrate .NET objects into your templates by using so-called "variables" (`@myVariable`)
or pull data from a database as needed, by providing a custom function (`@myFunction()`).
You may even create your own controls by deriving from the `Control` base class!

## Semantic Versioning

This library follows the principles of [Semantic Versioning](https://semver.org/). This means that version numbers and
the way they change convey meaning about the underlying changes in the library. For example, if a minor version number
changes (e.g., 1.1 to 1.2), this indicates that new features have been added in a backwards-compatible manner.

## Getting Started

To get started, install the [NuGet package](https://www.nuget.org/packages/X39.Solutions.PdfTemplate/) into your
project:

```shell
dotnet add package X39.Solutions.PdfTemplate
```

The package targets .NET 8.0, is marked trim-compatible, and depends on SkiaSharp,
`Microsoft.Extensions.DependencyInjection.Abstractions` and `X39.Util`.
This README is included in the NuGet package.
Issues are tracked in the GitHub repository at <https://github.com/X39/X39.Solutions.PdfTemplate/issues>.

If you are running linux, you also will have to add
the [SkiaSharp linux assets](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux):

```shell
dotnet add package SkiaSharp.NativeAssets.Linux
```

Next, create an XML template. Here is a simple example:

```xml

<template>
    <body>
        <text>Hello, world!</text>
    </body>
</template>
```

After registering the library with your dependency injection container at startup:

```csharp
// ...
services.AddPdfTemplateServices();
// ...
```

You can use the following code to generate a PDF document from the template:

```csharp
// IServiceProvider serviceProvider
// Stream xmlTemplateStream
var paintCache             = serviceProvider.GetRequiredService<SkPaintCache>();
var controlExpressionCache = serviceProvider.GetRequiredService<ControlExpressionCache>();
var functions              = Enumerable.Empty<IFunction>();
await using var generator = new Generator(
    paintCache,
    controlExpressionCache,
    functions
);
generator.AddDefaults();
using var reader    = XmlReader.Create(xmlTemplateStream);
using var pdfStream = new MemoryStream();
await generator.GeneratePdfAsync(pdfStream, reader, CultureInfo.CurrentUICulture);
// pdfStream now contains the PDF
```

This will generate a PDF document with the text "Hello, world!".

`AddPdfTemplateServices` registers the supporting services used by `Generator`.
It does not register `Generator` itself and does not discover custom `IFunction`
implementations automatically. Pass any custom functions to the `Generator`
constructor, then call `generator.AddDefaults()` to register the built-in controls
and transformers.

## Template structure

A template is a "simple" XML file with some basic preprocessor.
It has four base sections:

```xml
<!-- The root node name is ignored and can be modified to your hearts desire -->
<template>
    <background>
        <!--
           Background is rendered every page and can be used to eg. add fold lines.
           All background contents are only rendering the first page
           (to clarify: the available space only accounts for the first page,
            it is rendered on all pages, but only ever the first page of
            the contents).
           Background also ignores page margin and padding configuration,
           working with the initial size.
        -->
    </background>
    <header>
        <!--
           Header Section is used to define a "header" that
           may have up to 25% (- page margin/padding) of the height.
           The header is repeated and rendered every page, always at top.
        -->
    </header>
    <body>
        <!--
           Body section contains the actual document contents.
           It is rendered across as many pages as required.
           Depending on the header/footer sections, the available size on the page
           may be 100% or 50% (- page margin/padding).
        -->
    </body>
    <footer>
        <!--
           Footer Section is used to define a "footer" that
           may have up to 25% (- page margin/padding) of the height.
           The footer is repeated and rendered every page, always at the bottom.
        -->
    </footer>
    <foreground>
      <!--
         Foreground is rendered every page and can be used to eg. add fold lines.
         All foreground contents are only rendering the first page
         (to clarify: the available space only accounts for the first page,
          it is rendered on all pages, but only ever the first page of
          the contents).
         Foreground also ignores page margin and padding configuration,
         working with the initial size.
      -->
    </foreground>
    <areas>
      <area left="10cm" right="10cm" height="10cm" top="10cm">
        <!-- See About areas -->
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

The `areas` section is a special section, rendering content at a designated area.
The area is identified by a position provided on a separate node and ignore margin rules.

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
Pass function instances to the `Generator` constructor when creating a generator. Here is an example:

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

### Variables

You can use variables in your templates to access .NET objects. To do this, you just need to add the variable to
the `Generator` instance:

```csharp
generator.TemplateData.SetVariable("MyVariable", "Hello World!");
```

### End-User facing data types

The library uses a variety of data types to represent values in the template.
The following list gives an overview of end-user facing data types and their meaning.

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

Later in your code, add the control to the `Generator` instance:

```csharp
generator.AddControl<MyControl>();
```

You can now use the control in your XML templates (note the namespace import at the top):

```xml 

<template xmlns:my="MyControls">
    <body>
        <my:MyControl/>
    </body>
</template>
```

**WARNING** The template has an implicit default namespace.
If you change the default namespace (`xmlns="MyControls"`) instead of defining your own prefix,
you will have to appropriately refer to default controls and the template layout itself via that namespace!

#### `text`

The `text` control allows to render text. It can be used as follows:

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

The `image` control allows to render images.

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

The `table` control allows to render tables.
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
| `Width`      | The width of the cell in either [`Length`](#length) or parts (eg. `1*). | Any [`Length`](#length) or parts (eg. `1*`) | `auto`  |

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
This allows to expand a template and enrich it with data from csharp.

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

afterwards, add the transformer to the `Generator` instance:

```csharp
generator.AddTransformer(new MyTransformer());
```

Note that the way transformers are added is subject to change in the future to allow for a better integration with
dependency injection.

##### Evaluating user data

While building your transformer, you may have to evaluate data of a user to eg. resolve a function call.
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

The `alternate` transformer allows to alternate between values, making it possible to eg. create a table with
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

The `var` transformer allows to introduce new variables in the XML template to eg. cache a result or
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

The `if` transformer allows to conditionally include parts of the template.
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

The `for` transformer allows to repeat parts of the template.

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

The `foreach` transformer allows to repeat parts of the template for each element in a list.

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
Functions are registered on `ITemplateData`; the `Generator` constructor registers the function instances it receives.
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

#### `IAddControls`

`IAddControls` is an internal registration abstraction used by the default-control helper methods.
It is not intended for external implementation.
Consumer code should call `generator.AddControl<TControl>()` for custom controls and `generator.AddDefaults()` for
the built-in control set.

#### `IAddTransformers`

`IAddTransformers` is an internal registration abstraction used by the default-transformer helper methods.
It is not intended for external implementation.
Consumer code should call `generator.AddTransformer(transformer)` for custom transformers and `generator.AddDefaults()`
for the built-in transformer set.

#### `IPropertyAccessCache`

`IPropertyAccessCache` is infrastructure for expression evaluation.
It caches property-access delegates so repeated template expressions can read object properties efficiently.
It is registered by `AddPdfTemplateServices` and is not a normal customization point.
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

## Proper documentation for End-Users

While the code is documented, a dedicated documentation site for end-users is still missing.
The test project contains executable samples in `test/X39.Solutions.PdfTemplate.Test/Samples`,
including a README-style invoice sample.
There is also a `samples/WebAppSampleApi` project in the repository, but it is currently only a generic ASP.NET sample
host and does not yet demonstrate the PDF template package.
Dedicated end-user documentation is planned tho given that this is a spare-time project,
it might take a while and does not have a high priority (on my list).
Feel free to contribute to this project by adding documentation for end-users (e.g. using JetBrains Writerside or
similar tools) and
submitting a pull request. I will gladly review it and provide the necessary web-hosting in this repository (including a
domain).

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
Feel free to discuss this agreement in the discussions section of this repository, i am open to changes here (as long as
they do not open me or any other user of this project to any liability due to a **malicious contribution**).

### Additional controls

If you have created an additional control which is not depending on any other library, feel free to submit a pull
request.
If your control depends on another library, please create a separate repository and create a pull request to add it to a
list in this README.md.
This way, the core library can stay as small as possible and users can decide which controls they want to use.
Feel free to ask for help regarding publishing your control as a separate NuGet package in the discussions section of
this repository.

## License

This project is licensed under the GNU Lesser General Public License v3.0. See the [LICENSE](LICENSE) file for details.
