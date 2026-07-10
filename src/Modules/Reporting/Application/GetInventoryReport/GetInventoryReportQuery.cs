using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetInventoryReport;

public sealed record GetInventoryReportQuery(string? SearchTerm) : IRequest<Result<InventoryReportResult>>;
