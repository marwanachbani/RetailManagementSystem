using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateInventorySettings;

public sealed record UpdateInventorySettingsCommand(InventorySettingsModel Settings) : IRequest<Result>;

public sealed class UpdateInventorySettingsHandler : IRequestHandler<UpdateInventorySettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdateInventorySettingsHandler(ISettingsWriteStore writeStore, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateInventorySettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.InventoryPairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        await _eventBus.PublishAsync(new SettingChangedIntegrationEvent("Inventory", null, request.Settings.DefaultLowStockThreshold.ToString()), cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateInventorySettingsValidator : AbstractValidator<UpdateInventorySettingsCommand>
{
    public UpdateInventorySettingsValidator()
    {
        RuleFor(x => x.Settings.DefaultLowStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Low stock threshold cannot be negative.");
        RuleFor(x => x.Settings.DefaultWarehouse).NotEmpty().WithMessage("Default warehouse is required.");
        RuleFor(x => x.Settings.DefaultStockAdjustmentReason)
            .NotEmpty().WithMessage("Default stock adjustment reason is required.");
    }
}
