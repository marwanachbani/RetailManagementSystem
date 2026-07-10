using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetCustomerReport;

public sealed record GetCustomerReportQuery(string? SearchTerm, bool IncludeInactive) : IRequest<Result<CustomerReportResult>>;
