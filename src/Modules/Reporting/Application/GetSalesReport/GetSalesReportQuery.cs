using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetSalesReport;

public sealed record GetSalesReportQuery(DateRangeFilter? DateRange, string? SearchTerm, string? SortColumn, bool SortDescending) : IRequest<Result<SalesReportResult>>;
