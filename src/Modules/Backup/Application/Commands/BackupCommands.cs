using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Application.Models;

namespace RMS.Modules.Backup.Application.Commands;

public sealed record CreateBackupCommand(string? Notes = null) : IRequest<Result<BackupResult>>;

public sealed class CreateBackupHandler : IRequestHandler<CreateBackupCommand, Result<BackupResult>>
{
    private readonly IBackupService _service;
    public CreateBackupHandler(IBackupService service) => _service = service;

    public async Task<Result<BackupResult>> Handle(CreateBackupCommand request, CancellationToken cancellationToken)
        => Result.Success(await _service.CreateBackupAsync(request.Notes, null, cancellationToken));
}

public sealed record DeleteBackupCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteBackupHandler : IRequestHandler<DeleteBackupCommand, Result>
{
    private readonly IBackupService _service;
    public DeleteBackupHandler(IBackupService service) => _service = service;

    public async Task<Result> Handle(DeleteBackupCommand request, CancellationToken cancellationToken)
        => await _service.DeleteBackupAsync(request.Id, cancellationToken);
}

public sealed record VerifyBackupCommand(string BackupPath) : IRequest<Result<BackupVerificationResult>>;

public sealed class VerifyBackupHandler : IRequestHandler<VerifyBackupCommand, Result<BackupVerificationResult>>
{
    private readonly IBackupService _service;
    public VerifyBackupHandler(IBackupService service) => _service = service;

    public async Task<Result<BackupVerificationResult>> Handle(VerifyBackupCommand request, CancellationToken cancellationToken)
        => Result.Success(await _service.VerifyBackupAsync(request.BackupPath, cancellationToken));
}

public sealed record RestoreBackupCommand(RestoreRequest Request) : IRequest<Result<RestoreResult>>;

public sealed class RestoreBackupHandler : IRequestHandler<RestoreBackupCommand, Result<RestoreResult>>
{
    private readonly IBackupService _service;
    public RestoreBackupHandler(IBackupService service) => _service = service;

    public async Task<Result<RestoreResult>> Handle(RestoreBackupCommand request, CancellationToken cancellationToken)
        => Result.Success(await _service.RestoreAsync(request.Request, null, cancellationToken));
}
