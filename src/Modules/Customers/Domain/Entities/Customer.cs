using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Customers.Domain.ValueObjects;

namespace RMS.Modules.Customers.Domain.Entities;

public enum CustomerStatus
{
    Active = 0,
    Inactive = 1
}

public sealed class Customer : AggregateRoot<Guid>
{
    public string CustomerCode { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public Email? Email { get; private set; }
    public Address? Address { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Customer() { }

    public static Customer Rehydrate(
        Guid id,
        string customerCode,
        string firstName,
        string lastName,
        PhoneNumber phoneNumber,
        Email? email,
        Address? address,
        CustomerStatus status,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new Customer
        {
            Id = id,
            CustomerCode = customerCode,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Email = email,
            Address = address,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public static Customer Create(
        Guid id,
        string firstName,
        string lastName,
        PhoneNumber phoneNumber,
        Email? email = null,
        Address? address = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessRuleValidationException("Customer.FirstNameEmpty", "First name is required.");

        if (firstName.Length > 100)
            throw new BusinessRuleValidationException("Customer.FirstNameTooLong", "First name must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new BusinessRuleValidationException("Customer.LastNameEmpty", "Last name is required.");

        if (lastName.Length > 100)
            throw new BusinessRuleValidationException("Customer.LastNameTooLong", "Last name must not exceed 100 characters.");

        var customer = new Customer
        {
            Id = id,
            CustomerCode = GenerateCustomerCode(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = phoneNumber,
            Email = email,
            Address = address,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        customer.Raise(new CustomerCreatedEvent(customer.Id, customer.CustomerCode, customer.FullName, customer.PhoneNumber.Value, customer.Email?.Value));
        return customer;
    }

    public void Update(
        string firstName,
        string lastName,
        PhoneNumber phoneNumber,
        Email? email,
        Address? address)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessRuleValidationException("Customer.FirstNameEmpty", "First name is required.");

        if (firstName.Length > 100)
            throw new BusinessRuleValidationException("Customer.FirstNameTooLong", "First name must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new BusinessRuleValidationException("Customer.LastNameEmpty", "Last name is required.");

        if (lastName.Length > 100)
            throw new BusinessRuleValidationException("Customer.LastNameTooLong", "Last name must not exceed 100 characters.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber;
        Email = email;
        Address = address;
        UpdatedAt = DateTime.UtcNow;

        Raise(new CustomerUpdatedEvent(Id, CustomerCode, FullName, PhoneNumber.Value, Email?.Value));
    }

    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
            throw new BusinessRuleValidationException("Customer.AlreadyInactive", "Customer is already inactive.");

        Status = CustomerStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        Raise(new CustomerDeactivatedEvent(Id, CustomerCode, FullName));
    }

    public void Reactivate()
    {
        if (Status == CustomerStatus.Active)
            throw new BusinessRuleValidationException("Customer.AlreadyActive", "Customer is already active.");

        Status = CustomerStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        Raise(new CustomerReactivatedEvent(Id, CustomerCode, FullName));
    }

    public void EnsureActiveForNewSale()
    {
        if (Status == CustomerStatus.Inactive)
            throw new BusinessRuleValidationException("Customer.InactiveForSale", "Inactive customers cannot be assigned to new sales.");
    }

    private static string GenerateCustomerCode()
    {
        return $"CUST-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
    }
}
