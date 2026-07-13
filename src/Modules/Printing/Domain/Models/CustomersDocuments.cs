using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Domain.Models;

public sealed record StatementLine(
    DateTime Date,
    string Reference,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal Balance);

public sealed record CustomerStatementDocument(
    string StatementNumber,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    DocumentParty Customer,
    decimal OpeningBalance,
    IReadOnlyList<StatementLine> Lines,
    decimal ClosingBalance,
    string? CurrencyCode = null);

public sealed record PurchaseHistoryLine(
    DateTime Date,
    string Reference,
    int ItemCount,
    decimal Total);

public sealed record CustomerPurchaseHistoryDocument(
    DocumentParty Customer,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    IReadOnlyList<PurchaseHistoryLine> Orders,
    decimal TotalSpent,
    int OrderCount,
    DateTime? LastPurchaseDate = null,
    string? CurrencyCode = null);

public sealed record SupplierStatementDocument(
    string StatementNumber,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    DocumentParty Supplier,
    decimal OpeningBalance,
    IReadOnlyList<StatementLine> Lines,
    decimal ClosingBalance,
    string? CurrencyCode = null);

public sealed record SupplierPurchaseHistoryDocument(
    DocumentParty Supplier,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    IReadOnlyList<PurchaseHistoryLine> Orders,
    decimal TotalSpent,
    int OrderCount,
    DateTime? LastPurchaseDate = null,
    string? CurrencyCode = null);
