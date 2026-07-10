using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetFinancialReport;

public sealed record GetFinancialReportQuery(DateRangeFilter? DateRange, string PeriodType) : IRequest<Result<FinancialReportResult>>;
