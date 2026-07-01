using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Suppliers.Domain.Events;
using RMS.Modules.Suppliers.Domain.ValueObjects;

namespace RMS.Modules.Suppliers.Domain.Entities;

public enum SupplierStatus
{
    Active = 0,
    Inactive = 1
}

public sealed class Supplier : AggregateRoot<Guid>
{
    public string SupplierCode { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string? ContactPerson { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public Email? Email { get; private set; }
    public string? VatNumber { get; private set; }
    public Address? Address { get; private set; }
    public SupplierStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Supplier() { }

    public static Supplier Rehydrate(
        Guid id,
        string supplierCode,
        string companyName,
        string? contactPerson,
        PhoneNumber phoneNumber,
        Email? email,
        string? vatNumber,
        Address? address,
        SupplierStatus status,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new Supplier
        {
            Id = id,
            SupplierCode = supplierCode,
            CompanyName = companyName,
            ContactPerson = contactPerson,
            PhoneNumber = phoneNumber,
            Email = email,
            VatNumber = vatNumber,
            Address = address,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public static Supplier Create(
        Guid id,
        string companyName,
        PhoneNumber phoneNumber,
        string? contactPerson = null,
        Email? email = null,
        string? vatNumber = null,
        Address? address = null)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new BusinessRuleValidationException("Supplier.CompanyNameEmpty", "Company name is required.");

        if (companyName.Length > 200)
            throw new BusinessRuleValidationException("Supplier.CompanyNameTooLong", "Company name must not exceed 200 characters.");

        var supplier = new Supplier
        {
            Id = id,
            SupplierCode = GenerateSupplierCode(),
            CompanyName = companyName.Trim(),
            ContactPerson = contactPerson?.Trim(),
            PhoneNumber = phoneNumber,
            Email = email,
            VatNumber = vatNumber?.Trim(),
            Address = address,
            Status = SupplierStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        supplier.Raise(new SupplierCreatedEvent(
            supplier.Id,
            supplier.SupplierCode,
            supplier.CompanyName,
            supplier.PhoneNumber.Value,
            supplier.Email?.Value));

        return supplier;
    }

    public void Update(
        string companyName,
        PhoneNumber phoneNumber,
        string? contactPerson = null,
        Email? email = null,
        string? vatNumber = null,
        Address? address = null)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new BusinessRuleValidationException("Supplier.CompanyNameEmpty", "Company name is required.");

        if (companyName.Length > 200)
            throw new BusinessRuleValidationException("Supplier.CompanyNameTooLong", "Company name must not exceed 200 characters.");

        CompanyName = companyName.Trim();
        ContactPerson = contactPerson?.Trim();
        PhoneNumber = phoneNumber;
        Email = email;
        VatNumber = vatNumber?.Trim();
        Address = address;
        UpdatedAt = DateTime.UtcNow;

        Raise(new SupplierUpdatedEvent(Id, SupplierCode, CompanyName, PhoneNumber.Value));
    }

    public void Deactivate()
    {
        if (Status == SupplierStatus.Inactive)
            throw new BusinessRuleValidationException("Supplier.AlreadyInactive", "Supplier is already inactive.");

        Status = SupplierStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        Raise(new SupplierDeactivatedEvent(Id, SupplierCode));
    }

    public void Reactivate()
    {
        if (Status == SupplierStatus.Active)
            throw new BusinessRuleValidationException("Supplier.AlreadyActive", "Supplier is already active.");

        Status = SupplierStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        Raise(new SupplierReactivatedEvent(Id, SupplierCode));
    }

    public void EnsureActiveForPurchaseOrder()
    {
        if (Status == SupplierStatus.Inactive)
            throw new BusinessRuleValidationException("Supplier.InactiveForPurchase", "Inactive suppliers cannot receive new purchase orders.");
    }

    private static string GenerateSupplierCode()
    {
        return $"SUP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
    }
}
