using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetSupplierReport;

public sealed record GetSupplierReportQuery(string? SearchTerm, bool IncludeInactive) : IRequest<Result<SupplierReportResult>>;
