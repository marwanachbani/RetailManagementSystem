using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Sales.Domain.Entities;

public sealed class Receipt : Entity<Guid>
{
    public Guid SaleId { get; private set; }
    public string ReceiptNumber { get; private set; } = string.Empty;
    public string? PdfPath { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public string? StoreName { get; private set; }
    public string? CashierName { get; private set; }
    public decimal TotalAmount { get; private set; }

    private Receipt() { }

    public static Receipt Create(
        Guid id,
        Guid saleId,
        string receiptNumber,
        string? storeName,
        string? cashierName,
        decimal totalAmount)
    {
        return new Receipt
        {
            Id = id,
            SaleId = saleId,
            ReceiptNumber = receiptNumber,
            StoreName = storeName,
            CashierName = cashierName,
            TotalAmount = totalAmount,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public void SetPdfPath(string pdfPath)
    {
        PdfPath = pdfPath;
    }
}
