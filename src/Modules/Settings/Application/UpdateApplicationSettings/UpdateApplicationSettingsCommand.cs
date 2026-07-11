using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateApplicationSettings;

public sealed record UpdateApplicationSettingsCommand(ApplicationSettingsModel Settings) : IRequest<Result>;

public sealed class UpdateApplicationSettingsHandler : IRequestHandler<UpdateApplicationSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;

    public UpdateApplicationSettingsHandler(ISettingsWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public async Task<Result> Handle(UpdateApplicationSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.ApplicationPairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateApplicationSettingsValidator : AbstractValidator<UpdateApplicationSettingsCommand>
{
    public UpdateApplicationSettingsValidator()
    {
        RuleFor(x => x.Settings.Theme)
            .Must(t => new[] { "Light", "Dark" }.Contains(t))
            .WithMessage("Theme must be Light or Dark.");
        RuleFor(x => x.Settings.StartupPage).NotEmpty().WithMessage("Startup page is required.");
        RuleFor(x => x.Settings.SessionTimeout)
            .InclusiveBetween(1, 1440).WithMessage("Session timeout must be between 1 and 1440 minutes.");
    }
}
