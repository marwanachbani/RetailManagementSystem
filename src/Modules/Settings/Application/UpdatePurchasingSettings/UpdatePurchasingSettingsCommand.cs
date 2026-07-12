using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdatePurchasingSettings;

public sealed record UpdatePurchasingSettingsCommand(PurchasingSettingsModel Settings) : IRequest<Result>;

public sealed class UpdatePurchasingSettingsHandler : IRequestHandler<UpdatePurchasingSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdatePurchasingSettingsHandler(ISettingsWriteStore writeStore, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdatePurchasingSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.PurchasingPairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        await _eventBus.PublishAsync(new SettingChangedIntegrationEvent("Purchasing", null, request.Settings.PurchaseNumberPrefix), cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdatePurchasingSettingsValidator : AbstractValidator<UpdatePurchasingSettingsCommand>
{
    public UpdatePurchasingSettingsValidator()
    {
        RuleFor(x => x.Settings.PurchaseNumberPrefix)
            .NotEmpty().WithMessage("Purchase number prefix is required.")
            .MaximumLength(10).WithMessage("Purchase number prefix is too long.");
        RuleFor(x => x.Settings.DefaultPaymentTerms).NotEmpty().WithMessage("Default payment terms are required.");
    }
}
