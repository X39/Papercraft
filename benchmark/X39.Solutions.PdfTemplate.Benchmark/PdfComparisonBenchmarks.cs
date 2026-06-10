using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;
using QuestPdfColors = QuestPDF.Helpers.Colors;
using QuestPdfPageSizes = QuestPDF.Helpers.PageSizes;

namespace X39.Solutions.PdfTemplate.Benchmark;

[BenchmarkCategory(BenchmarkCategories.Generation, BenchmarkCategories.Comparison)]
public class PdfComparisonBenchmarks
{
    private const int RowCount = 28;
    private const float PageWidth = 595.27563F;
    private const float PageHeight = 841.88977F;
    private const float Margin = 28.34646F;

    private ServiceProvider _serviceProvider = null!;
    private Generator _generator = null!;
    private IDocument _questPdfDocument = null!;
    private DocumentOptions _documentOptions;

    [GlobalSetup]
    public void GlobalSetup()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _serviceProvider = BenchmarkServices.CreateDefaultServiceProvider();
        _generator = BenchmarkServices.CreateDefaultGenerator(_serviceProvider);
        _questPdfDocument = new QuestPdfInvoiceDocument();
        _documentOptions = new DocumentOptions
        {
            DotsPerInch = 72,
            Margin = new Thickness(new Length(1, ELengthUnit.Centimeters)),
            Modified = new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc),
            Producer = "X39.Solutions.PdfTemplate.Benchmark",
        };

        GenerateDirectSkiaSharpInvoicePdf();
        GenerateQuestPdfInvoicePdf();
        GeneratePapercraftInvoicePdf().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _generator.Dispose();
        _serviceProvider.Dispose();
    }

    [Benchmark(Baseline = true, Description = "Direct SkiaSharp PDF/A")]
    public long GenerateDirectSkiaSharpInvoicePdf()
    {
        using var stream = new MemoryStream();
        using var document = SKDocument.CreatePdf(
            stream,
            new SKDocumentPdfMetadata
            {
                RasterDpi = 72,
                Producer = "X39.Solutions.PdfTemplate.Benchmark.DirectSkiaSharp",
                Modified = new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc),
                PdfA = true,
            });

        using var canvas = document.BeginPage(PageWidth, PageHeight);
        DrawDirectSkiaSharpInvoice(canvas);
        document.EndPage();
        document.Close();
        return stream.Length;
    }

    [Benchmark(Description = "QuestPDF PDF")]
    public long GenerateQuestPdfInvoicePdf()
    {
        using var stream = new MemoryStream();
        _questPdfDocument.GeneratePdf(stream);
        return stream.Length;
    }

    [Benchmark(Description = "Papercraft XML PDF/A")]
    public async Task<long> GeneratePapercraftInvoicePdf()
    {
        using var stream = new MemoryStream();
        using var reader = BenchmarkTemplates.CreateXmlReader(BenchmarkTemplates.RepresentativeGenerationTemplate);
        await _generator.GeneratePdfAsync(
                stream,
                reader,
                BenchmarkServices.Culture,
                _documentOptions,
                CancellationToken.None)
            .ConfigureAwait(false);
        return stream.Length;
    }

    private static void DrawDirectSkiaSharpInvoice(SKCanvas canvas)
    {
        canvas.Clear(SKColors.White);

        using var typeface = SKTypeface.FromFamilyName("Arial");
        using var titleFont = new SKFont(typeface, 18);
        using var bodyFont = new SKFont(typeface, 10);
        using var bodyBoldFont = new SKFont(typeface, 10);
        bodyBoldFont.Embolden = true;

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
        };
        using var alternatePaint = new SKPaint
        {
            Color = new SKColor(0xF0, 0xF0, 0xF0),
            Style = SKPaintStyle.Fill,
        };
        using var borderPaint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
            IsAntialias = false,
        };

        var contentWidth = PageWidth - 2 * Margin;
        var y = Margin + 18;
        canvas.DrawText("Benchmark invoice #2026-05", Margin, y, SKTextAlign.Left, titleFont, textPaint);
        y += 18;
        canvas.DrawText("Generated for deterministic benchmark data", Margin, y, SKTextAlign.Left, bodyFont, textPaint);
        y += 34;

        var columnWidths = new[]
        {
            contentWidth * 0.12F,
            contentWidth * 0.48F,
            contentWidth * 0.20F,
            contentWidth * 0.20F,
        };
        var columnX = new[]
        {
            Margin,
            Margin + columnWidths[0],
            Margin + columnWidths[0] + columnWidths[1],
            Margin + columnWidths[0] + columnWidths[1] + columnWidths[2],
        };

        const float headerHeight = 22;
        const float rowHeight = 18;
        DrawDirectCell(canvas, "#", columnX[0], y, columnWidths[0], headerHeight, bodyBoldFont, textPaint, borderPaint);
        DrawDirectCell(canvas, "Product", columnX[1], y, columnWidths[1], headerHeight, bodyBoldFont, textPaint, borderPaint);
        DrawDirectCell(canvas, "Quantity", columnX[2], y, columnWidths[2], headerHeight, bodyBoldFont, textPaint, borderPaint, SKTextAlign.Right);
        DrawDirectCell(canvas, "Total", columnX[3], y, columnWidths[3], headerHeight, bodyBoldFont, textPaint, borderPaint, SKTextAlign.Right);
        y += headerHeight;

        for (var i = 1; i <= RowCount; i++)
        {
            if (i % 2 == 0)
                canvas.DrawRect(Margin, y, contentWidth, rowHeight, alternatePaint);

            DrawDirectCell(canvas, i.ToString(BenchmarkServices.Culture), columnX[0], y, columnWidths[0], rowHeight, bodyFont, textPaint, borderPaint);
            DrawDirectCell(canvas, $"Benchmark item {i}", columnX[1], y, columnWidths[1], rowHeight, bodyFont, textPaint, borderPaint);
            DrawDirectCell(canvas, (1 + i % 4).ToString(BenchmarkServices.Culture), columnX[2], y, columnWidths[2], rowHeight, bodyFont, textPaint, borderPaint, SKTextAlign.Right);
            DrawDirectCell(canvas, $"{25 + i * 3}.00 EUR", columnX[3], y, columnWidths[3], rowHeight, bodyFont, textPaint, borderPaint, SKTextAlign.Right);
            y += rowHeight;
        }

        canvas.DrawText("Page footer", PageWidth - Margin, PageHeight - Margin, SKTextAlign.Right, bodyFont, textPaint);
    }

    private static void DrawDirectCell(
        SKCanvas canvas,
        string text,
        float x,
        float y,
        float width,
        float height,
        SKFont font,
        SKPaint textPaint,
        SKPaint borderPaint,
        SKTextAlign alignment = SKTextAlign.Left)
    {
        canvas.DrawRect(x, y, width, height, borderPaint);
        var textX = alignment is SKTextAlign.Right ? x + width - 4 : x + 4;
        canvas.DrawText(text, textX, y + height - 5, alignment, font, textPaint);
    }

    private sealed class QuestPdfInvoiceDocument : IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            container.Page(
                (page) =>
                {
                    page.Size(QuestPdfPageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle((style) => style.FontSize(10).FontFamily("Arial"));

                    page.Header()
                        .Column(
                            (column) =>
                            {
                                column.Item().Text("Benchmark invoice #2026-05").FontSize(18);
                                column.Item().Text("Generated for deterministic benchmark data");
                            });

                    page.Content()
                        .PaddingTop(16)
                        .Table(
                            (table) =>
                            {
                                table.ColumnsDefinition(
                                    (columns) =>
                                    {
                                        columns.RelativeColumn(12);
                                        columns.RelativeColumn(48);
                                        columns.RelativeColumn(20);
                                        columns.RelativeColumn(20);
                                    });

                                table.Header(
                                    (header) =>
                                    {
                                        HeaderCell(header.Cell(), "#");
                                        HeaderCell(header.Cell(), "Product");
                                        HeaderCell(header.Cell().AlignRight(), "Quantity");
                                        HeaderCell(header.Cell().AlignRight(), "Total");
                                    });

                                for (var i = 1; i <= RowCount; i++)
                                {
                                    var background = i % 2 == 0 ? QuestPdfColors.Grey.Lighten3 : QuestPdfColors.White;
                                    BodyCell(table.Cell(), i.ToString(BenchmarkServices.Culture), background);
                                    BodyCell(table.Cell(), $"Benchmark item {i}", background);
                                    BodyCell(table.Cell().AlignRight(), (1 + i % 4).ToString(BenchmarkServices.Culture), background);
                                    BodyCell(table.Cell().AlignRight(), $"{25 + i * 3}.00 EUR", background);
                                }
                            });

                    page.Footer()
                        .AlignRight()
                        .Text("Page footer");
                });
        }

        public DocumentMetadata GetMetadata()
            => DocumentMetadata.Default;

        private static void HeaderCell(IContainer container, string text)
        {
            container
                .BorderBottom(1)
                .BorderColor(QuestPdfColors.Black)
                .Padding(3)
                .Text(text)
                .SemiBold();
        }

        private static void BodyCell(IContainer container, string text, string background)
        {
            container
                .Background(background)
                .Border(1)
                .BorderColor(QuestPdfColors.Black)
                .Padding(3)
                .Text(text);
        }
    }
}
