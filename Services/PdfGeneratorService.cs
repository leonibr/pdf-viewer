using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PdfViewer.Services;

public class PdfGeneratorService
{
    private static readonly string[] Products =
    [
        "Wireless Keyboard", "Mechanical Mouse", "4K Monitor", "USB-C Hub",
        "Laptop Stand", "Webcam HD", "Noise-Cancelling Headset", "SSD 1TB",
        "RAM 32GB DDR5", "Graphics Card RTX", "Ergonomic Chair", "Desk Lamp"
    ];

    private static readonly string[] Regions = ["North", "South", "East", "West", "Central"];

    public byte[] Generate()
    {
        var rng = new Random();
        var requestId = Guid.NewGuid();
        var generatedAt = DateTime.Now;

        // Random KPI values
        var revenue = rng.Next(40_000, 120_000);
        var orders = rng.Next(500, 3_000);
        var newUsers = rng.Next(100, 800);
        var conversionRate = Math.Round(rng.NextDouble() * 8 + 1, 2);

        // Random sales table rows
        var rowCount = rng.Next(6, 10);
        var tableRows = Enumerable.Range(0, rowCount).Select(_ =>
        {
            var product = Products[rng.Next(Products.Length)];
            var region = Regions[rng.Next(Regions.Length)];
            var qty = rng.Next(1, 50);
            var unitPrice = Math.Round(rng.NextDouble() * 490 + 10, 2);
            return (product, region, qty, unitPrice, total: Math.Round(qty * unitPrice, 2));
        }).ToList();

        var grandTotal = tableRows.Sum(r => r.total);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeContent(content, requestId, generatedAt, revenue, orders, newUsers, conversionRate, tableRows, grandTotal));
                page.Footer().Element(footer => ComposeFooter(footer, generatedAt));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container)
    {
        container
            .Background("#1e40af")
            .Padding(20)
            .Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Sales Dashboard Report")
                        .FontSize(22).FontColor(Colors.White).Bold();
                    col.Item().Text("Quarterly Performance Overview")
                        .FontSize(11).FontColor("#93c5fd");
                });

                row.ConstantItem(120).AlignRight().AlignMiddle().Column(col =>
                {
                    col.Item().Text("LIVE DATA")
                        .FontSize(10).FontColor("#fbbf24").Bold().AlignRight();
                    col.Item().Text("Auto-Generated")
                        .FontSize(9).FontColor("#93c5fd").AlignRight();
                });
            });
    }

    private static void ComposeContent(
        IContainer container,
        Guid requestId,
        DateTime generatedAt,
        int revenue, int orders, int newUsers, double conversionRate,
        List<(string product, string region, int qty, double unitPrice, double total)> rows,
        double grandTotal)
    {
        container.PaddingTop(16).Column(col =>
        {
            // Request metadata bar
            col.Item()
                .Background("#f1f5f9")
                .Border(1).BorderColor("#cbd5e1")
                .Padding(10)
                .Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Request ID").FontSize(8).FontColor("#64748b");
                        c.Item().Text(requestId.ToString()).FontSize(9).FontColor("#1e293b").Bold();
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Generated At").FontSize(8).FontColor("#64748b");
                        c.Item().Text(generatedAt.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                            .FontSize(9).FontColor("#dc2626").Bold();
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Status").FontSize(8).FontColor("#64748b");
                        c.Item().Text("FRESH — Not Cached").FontSize(9).FontColor("#16a34a").Bold();
                    });
                });

            col.Item().PaddingTop(14);

            // KPI Cards row
            col.Item().Text("Key Performance Indicators").FontSize(12).Bold().FontColor("#1e293b");
            col.Item().PaddingTop(1);

            col.Item().Row(row =>
            {
                KpiCard(row.RelativeItem(), "Total Revenue", $"${revenue:N0}", "#dcfce7", "#16a34a", "#14532d");
                row.ConstantItem(10);
                KpiCard(row.RelativeItem(), "Total Orders", $"{orders:N0}", "#dbeafe", "#2563eb", "#1e3a8a");
                row.ConstantItem(10);
                KpiCard(row.RelativeItem(), "New Users", $"{newUsers:N0}", "#fef3c7", "#d97706", "#78350f");
                row.ConstantItem(10);
                KpiCard(row.RelativeItem(), "Conversion Rate", $"{conversionRate}%", "#fce7f3", "#db2777", "#831843");
            });

            col.Item().PaddingTop(16);

            // Section header
            col.Item().Text("Product Sales Breakdown").FontSize(12).Bold().FontColor("#1e293b");
            col.Item().PaddingTop(1);

            // Sales table
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(4);   // Product
                    cols.RelativeColumn(2);   // Region
                    cols.RelativeColumn(1);   // Qty
                    cols.RelativeColumn(2.5f); // Unit Price
                    cols.RelativeColumn(2.5f); // Total
                });

                // Header row
                static void HeaderCell(IContainer c, string text, bool alignRight = false)
                {
                    var td = c.Background("#1e40af").Padding(6)
                               .Text(text).FontColor(Colors.White).Bold().FontSize(9).FontFamily("JetBrains Mono");
                    if (alignRight) td.AlignRight();
                }

                table.Header(header =>
                {
                    header.Cell().Element(c => HeaderCell(c, "Product"));
                    header.Cell().Element(c => HeaderCell(c, "Region"));
                    header.Cell().Element(c => HeaderCell(c, "Qty", alignRight: true));
                    header.Cell().Element(c => HeaderCell(c, "Unit Price", alignRight: true));
                    header.Cell().Element(c => HeaderCell(c, "Total", alignRight: true));
                });

                // Data rows
                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];
                    var bg = i % 2 == 0 ? "#ffffff" : "#f8fafc";

                    static IContainer DataCell(IContainer c, string bg) =>
                        c.Background(bg).BorderBottom(1).BorderColor("#e2e8f0").Padding(1);

                    table.Cell().Element(c => DataCell(c, bg).Text(r.product).FontSize(9).FontFamily("JetBrains Mono"));
                    table.Cell().Element(c => DataCell(c, bg).Text(r.region).FontSize(9).FontFamily("JetBrains Mono"));
                    table.Cell().Element(c => DataCell(c, bg).Text(r.qty.ToString()).FontSize(9).FontFamily("JetBrains Mono").AlignRight());
                    table.Cell().Element(c => DataCell(c, bg).Text($"{r.unitPrice:C2}").FontSize(9).FontFamily("JetBrains Mono").AlignRight());
                    table.Cell().Element(c => DataCell(c, bg).Text($"{r.total:C2}").FontSize(9).FontFamily("JetBrains Mono").Bold().AlignRight());
                }
            });

            // Grand total
            col.Item()
                .Background("#1e293b")
                .Padding(8)
                .AlignRight()
                .Text($"Grand Total:   ${grandTotal:N2}")
                .FontColor(Colors.White).Bold().FontSize(11);

            col.Item().PaddingTop(14);

            // Note about caching
            col.Item()
                .Background("#fffbeb")
                .Border(1).BorderColor("#fcd34d")
                .Padding(10)
                .Text(t =>
                {
                    t.Span("Note: ").Bold().FontColor("#92400e");
                    t.Span("This PDF is generated fresh on every request. The Request ID and timestamp above are unique per call, proving the document is never served from cache.")
                        .FontColor("#78350f").FontSize(9);
                });
        });
    }

    private static void KpiCard(IContainer container, string label, string value, string bg, string accent, string dark)
    {
        container
            .Background(bg)
            .Border(1).BorderColor(accent)
            .Padding(12)
            .Column(col =>
            {
                col.Item().Text(label).FontSize(8).FontColor(dark).Bold();
                col.Item().PaddingTop(4).Text(value).FontSize(18).FontColor(dark).Bold();
            });
    }

    private static void ComposeFooter(IContainer container, DateTime generatedAt)
    {
        container
            .BorderTop(1).BorderColor("#e2e8f0")
            .PaddingTop(8)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text($"Generated by PdfViewer Demo  •  {generatedAt:ddd, dd MMM yyyy HH:mm:ss}")
                    .FontSize(8).FontColor("#94a3b8");

                row.ConstantItem(60)
                    .AlignRight()
                    .Text(t =>
                    {
                        t.Span("Page ").FontSize(8).FontColor("#94a3b8");
                        t.CurrentPageNumber().FontSize(8).FontColor("#94a3b8");
                        t.Span(" of ").FontSize(8).FontColor("#94a3b8");
                        t.TotalPages().FontSize(8).FontColor("#94a3b8");
                    });
            });
    }
}
