using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateGeneralSettings;

public sealed record UpdateGeneralSettingsCommand(GeneralSettingsModel Settings) : IRequest<Result>;

public sealed class UpdateGeneralSettingsHandler : IRequestHandler<UpdateGeneralSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdateGeneralSettingsHandler(ISettingsWriteStore writeStore, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateGeneralSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.GeneralPairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        await _eventBus.PublishAsync(new SettingChangedIntegrationEvent("General", null, request.Settings.StoreName), cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateGeneralSettingsValidator : AbstractValidator<UpdateGeneralSettingsCommand>
{
    public UpdateGeneralSettingsValidator()
    {
        RuleFor(x => x.Settings.StoreName).NotEmpty().WithMessage("Store name is required.");
        RuleFor(x => x.Settings.Currency).NotEmpty().WithMessage("Currency is required.");
        RuleFor(x => x.Settings.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Settings.Email))
            .WithMessage("Email format is invalid.");
        RuleFor(x => x.Settings.Language).NotEmpty().WithMessage("Language is required.");
        RuleFor(x => x.Settings.DateFormat).NotEmpty().WithMessage("Date format is required.");
        RuleFor(x => x.Settings.TimeFormat).NotEmpty().WithMessage("Time format is required.");
    }
}
