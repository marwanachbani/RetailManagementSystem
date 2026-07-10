using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetPurchaseReport;

public sealed record GetPurchaseReportQuery(DateRangeFilter? DateRange, Guid? SupplierId, string? SearchTerm, string? SortColumn, bool SortDescending) : IRequest<Result<PurchaseReportResult>>;
