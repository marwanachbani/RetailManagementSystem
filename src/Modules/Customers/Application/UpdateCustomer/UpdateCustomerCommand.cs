using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Customers.Application.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string? Email,
    string? Street,
    string? City,
    string? PostalCode,
    string? Country) : IRequest<Result>;
