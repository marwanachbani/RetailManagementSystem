using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateStoreSettings;

public sealed record UpdateStoreSettingsCommand(StoreSettingsModel Settings) : IRequest<Result>;

public sealed class UpdateStoreSettingsHandler : IRequestHandler<UpdateStoreSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdateStoreSettingsHandler(ISettingsWriteStore writeStore, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateStoreSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.StorePairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        await _eventBus.PublishAsync(new SettingChangedIntegrationEvent("Store", null, "Updated"), cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateStoreSettingsValidator : AbstractValidator<UpdateStoreSettingsCommand>
{
    public UpdateStoreSettingsValidator()
    {
        RuleFor(x => x.Settings.CompanyAddress)
            .MaximumLength(500).WithMessage("Company address is too long.");
    }
}
