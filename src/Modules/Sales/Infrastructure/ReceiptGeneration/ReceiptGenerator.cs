using System.Text;
using RMS.Modules.Sales.Domain.Entities;

namespace RMS.Modules.Sales.Infrastructure.ReceiptGeneration;

public sealed class ReceiptGenerator : IReceiptGenerator
{
    public async Task<string> GenerateReceiptAsync(
        Sale sale,
        string storeName,
        string? cashierName,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDirectory);

        var fileName = $"receipt-{sale.SaleNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
        var path = Path.Combine(outputDirectory, fileName);

        var sb = new StringBuilder();
        sb.AppendLine(storeName);
        sb.AppendLine(new string('-', 40));
        sb.AppendLine($"Sale Number: {sale.SaleNumber}");
        sb.AppendLine($"Date: {sale.SaleDate:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Cashier: {cashierName ?? "N/A"}");
        sb.AppendLine($"Status: {sale.Status}");
        sb.AppendLine(new string('-', 40));

        foreach (var item in sale.Items)
        {
            sb.AppendLine($"{item.ProductName} x{item.Quantity} @ {item.UnitPrice:0.00} = {item.TotalPrice:0.00}");
        }

        sb.AppendLine(new string('-', 40));
        sb.AppendLine($"Subtotal: {sale.SubTotal:0.00}");
        sb.AppendLine($"Discount: {sale.DiscountAmount:0.00}");
        sb.AppendLine($"Tax: {sale.TaxAmount:0.00}");
        sb.AppendLine($"Total: {sale.TotalAmount:0.00}");

        await File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
        return path;
    }
}
