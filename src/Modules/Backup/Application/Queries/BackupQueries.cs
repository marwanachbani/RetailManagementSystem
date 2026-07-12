using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Application.Models;

namespace RMS.Modules.Backup.Application.Queries;

public sealed record GetBackupDashboardQuery : IRequest<Result<BackupDashboard>>;

public sealed class GetBackupDashboardHandler : IRequestHandler<GetBackupDashboardQuery, Result<BackupDashboard>>
{
    private readonly IBackupService _service;
    public GetBackupDashboardHandler(IBackupService service) => _service = service;

    public async Task<Result<BackupDashboard>> Handle(GetBackupDashboardQuery request, CancellationToken cancellationToken)
        => Result.Success(await _service.GetDashboardAsync(cancellationToken));
}

public sealed record GetBackupHistoryQuery : IRequest<Result<IReadOnlyList<BackupHistoryEntry>>>;

public sealed class GetBackupHistoryHandler : IRequestHandler<GetBackupHistoryQuery, Result<IReadOnlyList<BackupHistoryEntry>>>
{
    private readonly IBackupService _service;
    public GetBackupHistoryHandler(IBackupService service) => _service = service;

    public async Task<Result<IReadOnlyList<BackupHistoryEntry>>> Handle(GetBackupHistoryQuery request, CancellationToken cancellationToken)
        => Result.Success(await _service.GetHistoryAsync(cancellationToken));
}

public sealed record GetBackupDetailsQuery(string BackupPath) : IRequest<Result<BackupMetadata?>>;

public sealed class GetBackupDetailsHandler : IRequestHandler<GetBackupDetailsQuery, Result<BackupMetadata?>>
{
    private readonly IBackupService _service;
    public GetBackupDetailsHandler(IBackupService service) => _service = service;

    public async Task<Result<BackupMetadata?>> Handle(GetBackupDetailsQuery request, CancellationToken cancellationToken)
        => Result.Success(await _service.GetBackupDetailsAsync(request.BackupPath, cancellationToken));
}
