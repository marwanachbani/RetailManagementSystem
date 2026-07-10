using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetProductReport;

public sealed record GetProductReportQuery(string? SearchTerm, string? SortColumn, bool SortDescending) : IRequest<Result<ProductReportResult>>;
