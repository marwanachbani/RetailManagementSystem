namespace RMS.Modules.Printing.Domain.Models;

/// <summary>Company branding loaded from the Settings module and applied to every document.</summary>
public sealed record BrandingInfo(
    string StoreName,
    string Address,
    string Phone,
    string TaxNumber,
    string Email,
    string Website,
    string LogoPath,
    string ReceiptHeader,
    string ReceiptFooter,
    string CurrencyCode)
{
    public static readonly BrandingInfo Empty = new(
        "My Retail Store", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
        string.Empty, string.Empty, string.Empty, "USD");

    public bool HasLogo => !string.IsNullOrWhiteSpace(LogoPath) && File.Exists(LogoPath);
}

/// <summary>A single line on an invoice, receipt, order, statement, etc.</summary>
public sealed record DocumentLineItem(
    string Name,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? Sku = null,
    string? Description = null,
    decimal Discount = 0,
    decimal Tax = 0,
    string? Unit = null);

/// <summary>Monetary summary block shared by transactional documents.</summary>
public sealed record DocumentTotals(
    decimal SubTotal,
    decimal DiscountTotal = 0,
    decimal TaxTotal = 0,
    decimal GrandTotal = 0,
    decimal? PaidAmount = null,
    decimal? Change = null,
    string? PaymentMethod = null);

/// <summary>A customer or supplier participating in a document.</summary>
public sealed record DocumentParty(
    string Name,
    string? Address = null,
    string? Phone = null,
    string? TaxNumber = null,
    string? Email = null,
    string? AccountNumber = null,
    decimal Balance = 0);

public sealed record DocumentNote(string Text);
