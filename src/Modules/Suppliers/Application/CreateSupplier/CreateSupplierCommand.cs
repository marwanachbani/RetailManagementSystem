using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Suppliers.Application.CreateSupplier;

public sealed record CreateSupplierCommand(
    string CompanyName,
    string PhoneNumber,
    string? ContactPerson = null,
    string? Email = null,
    string? VatNumber = null,
    string? Street = null,
    string? City = null,
    string? PostalCode = null,
    string? Country = null) : IRequest<Result<Guid>>;
