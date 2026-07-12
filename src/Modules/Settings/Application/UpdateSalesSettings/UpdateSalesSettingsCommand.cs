using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateSalesSettings;

public sealed record UpdateSalesSettingsCommand(SalesSettingsModel Settings) : IRequest<Result>;

public sealed class UpdateSalesSettingsHandler : IRequestHandler<UpdateSalesSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdateSalesSettingsHandler(ISettingsWriteStore writeStore, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateSalesSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.SalesPairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        await _eventBus.PublishAsync(new SettingChangedIntegrationEvent("Sales", null, request.Settings.DefaultTaxRate.ToString()), cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateSalesSettingsValidator : AbstractValidator<UpdateSalesSettingsCommand>
{
    public UpdateSalesSettingsValidator()
    {
        RuleFor(x => x.Settings.DefaultTaxRate)
            .InclusiveBetween(0, 100).WithMessage("Default tax rate must be between 0 and 100%.");
        RuleFor(x => x.Settings.DefaultDiscount)
            .InclusiveBetween(0, 100).WithMessage("Default discount must be between 0 and 100%.");
        RuleFor(x => x.Settings.MaximumDiscount)
            .InclusiveBetween(0, 100).WithMessage("Maximum discount must be between 0 and 100%.");
        RuleFor(x => x.Settings.DefaultPaymentMethod).NotEmpty().WithMessage("Default payment method is required.");
    }
}
