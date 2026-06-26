using RMS.Modules.Sales.Domain.Entities;

namespace RMS.Modules.Sales.Infrastructure.ReceiptGeneration;

public interface IReceiptGenerator
{
    Task<string> GenerateReceiptAsync(Sale sale, string storeName, string? cashierName, string outputDirectory, CancellationToken cancellationToken = default);
}
