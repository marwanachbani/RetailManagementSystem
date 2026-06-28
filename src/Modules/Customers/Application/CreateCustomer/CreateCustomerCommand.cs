using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Customers.Application.CreateCustomer;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string? Email,
    string? Street,
    string? City,
    string? PostalCode,
    string? Country) : IRequest<Result<Guid>>;
