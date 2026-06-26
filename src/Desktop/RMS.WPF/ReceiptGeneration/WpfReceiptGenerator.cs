using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RMS.Modules.Sales.Domain.Entities;
using RMS.Modules.Sales.Infrastructure.ReceiptGeneration;

namespace RMS.WPF.ReceiptGeneration;

public sealed class WpfReceiptGenerator : IReceiptGenerator
{
    public Task<string> GenerateReceiptAsync(Sale sale, string storeName, string? cashierName, string outputDirectory, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDirectory);
        var fileName = $"Receipt_{sale.SaleNumber}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine(outputDirectory, fileName);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Text(storeName).Bold().FontSize(20);
                    header.Item().AlignCenter().Text("Retail Management System").FontSize(10);
                    header.Item().AlignCenter().Text($"Sale: {sale.SaleNumber}").FontSize(10);
                    header.Item().AlignCenter().Text($"Date: {sale.SaleDate:yyyy-MM-dd HH:mm}").FontSize(10);
                    if (!string.IsNullOrWhiteSpace(cashierName))
                        header.Item().AlignCenter().Text($"Cashier: {cashierName}").FontSize(10);
                });

                page.Content().Column(content =>
                {
                    content.Item().Height(10);
                    content.Item().LineHorizontal(1);
                    content.Item().Height(10);

                    // Items table
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Item").Bold();
                            header.Cell().Text("Qty").Bold();
                            header.Cell().Text("Unit").Bold();
                            header.Cell().Text("Total").Bold();
                        });

                        foreach (var item in sale.Items)
                        {
                            table.Cell().Text(item.ProductName);
                            table.Cell().Text(item.Quantity.ToString());
                            table.Cell().Text(item.UnitPrice.ToString("F2"));
                            table.Cell().Text(item.TotalPrice.ToString("F2"));
                        }
                    });

                    content.Item().Height(10);
                    content.Item().LineHorizontal(1);
                    content.Item().Height(10);

                    // Totals
                    content.Item().AlignRight().Text($"SubTotal: {sale.SubTotal:F2}");
                    if (sale.DiscountAmount > 0)
                        content.Item().AlignRight().Text($"Discount ({sale.DiscountPercentage:F2}%): -{sale.DiscountAmount:F2}");
                    if (sale.TaxAmount > 0)
                        content.Item().AlignRight().Text($"Tax ({sale.TaxPercentage:F2}%): +{sale.TaxAmount:F2}");
                    content.Item().AlignRight().Text($"Total: {sale.TotalAmount:F2}").Bold().FontSize(14);
                });

                page.Footer().AlignCenter().Text("Thank you for your purchase!").FontSize(9);
            });
        });

        document.GeneratePdf(filePath);
        return Task.FromResult(filePath);
    }
}
