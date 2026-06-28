using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Customers.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string? PostalCode { get; }
    public string? Country { get; }

    private Address(string street, string city, string? postalCode, string? country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string street, string city, string? postalCode = null, string? country = null)
    {
        return new Address(street.Trim(), city.Trim(), postalCode?.Trim(), country?.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }
}
