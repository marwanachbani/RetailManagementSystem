using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateBackupSettings;

public sealed record UpdateBackupSettingsCommand(BackupSettingsModel Settings) : IRequest<Result>;

public sealed class UpdateBackupSettingsHandler : IRequestHandler<UpdateBackupSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;

    public UpdateBackupSettingsHandler(ISettingsWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public async Task<Result> Handle(UpdateBackupSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.BackupPairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateBackupSettingsValidator : AbstractValidator<UpdateBackupSettingsCommand>
{
    public UpdateBackupSettingsValidator()
    {
        RuleFor(x => x.Settings.Frequency)
            .Must(f => new[] { "Daily", "Weekly", "Monthly" }.Contains(f))
            .WithMessage("Backup frequency must be Daily, Weekly or Monthly.");
        RuleFor(x => x.Settings.Time)
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$").WithMessage("Backup time must be in HH:mm format.");
        RuleFor(x => x.Settings.MaximumCount)
            .InclusiveBetween(1, 999).WithMessage("Maximum backup count must be between 1 and 999.");
    }
}
