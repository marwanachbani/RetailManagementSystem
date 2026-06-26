using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Sales.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new BusinessRuleValidationException("Money.AmountLessThanZero", "Amount cannot be less than zero.");
        
        Amount = amount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "MAD" : currency.Trim().ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency = "MAD")
    {
        return new Money(amount, currency);
    }

    public static Money Zero(string currency = "MAD") => new(0, currency);

    public Money Add(Money other)
    {
        if (other.Currency != Currency)
            throw new BusinessRuleValidationException("Money.CurrencyMismatch", "Cannot add amounts with different currencies.");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (other.Currency != Currency)
            throw new BusinessRuleValidationException("Money.CurrencyMismatch", "Cannot subtract amounts with different currencies.");
        if (other.Amount > Amount)
            throw new BusinessRuleValidationException("Money.Insufficient", "Subtraction would result in a negative amount.");
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new BusinessRuleValidationException("Money.NegativeFactor", "Cannot multiply by a negative factor.");
        return new Money(Amount * factor, Currency);
    }

    public Money ApplyDiscount(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new BusinessRuleValidationException("Money.InvalidDiscount", "Discount percentage must be between 0 and 100.");
        return new Money(Amount * (1 - discountPercentage / 100), Currency);
    }

    public Money AddTax(decimal taxPercentage)
    {
        if (taxPercentage < 0)
            throw new BusinessRuleValidationException("Money.InvalidTax", "Tax percentage cannot be negative.");
        return new Money(Amount * (1 + taxPercentage / 100), Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
