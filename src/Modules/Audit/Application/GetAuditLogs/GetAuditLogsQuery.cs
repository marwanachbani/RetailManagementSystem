using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Audit.Application.Contracts;

namespace RMS.Modules.Audit.Application.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    int PageNumber,
    int PageSize,
    DateTime? FromDate,
    DateTime? ToDate,
    string? UserId,
    string? Module,
    string? Action,
    string? SearchTerm) : IRequest<Result<PagedResult<AuditLogReadModel>>>;

public sealed class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogReadModel>>>
{
    private readonly IAuditReadStore _readStore;

    public GetAuditLogsQueryHandler(IAuditReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<PagedResult<AuditLogReadModel>>>
        Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.FromDate,
            request.ToDate,
            request.UserId,
            request.Module,
            request.Action,
            request.SearchTerm,
            cancellationToken);

        return Result.Success(result);
    }
}
