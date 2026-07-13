using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdatePrinterSettings;

public sealed record UpdatePrinterSettingsCommand(PrinterSettingsModel Settings) : IRequest<Result>;

public sealed class UpdatePrinterSettingsHandler : IRequestHandler<UpdatePrinterSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdatePrinterSettingsHandler(ISettingsWriteStore writeStore, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdatePrinterSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.PrinterPairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        await _eventBus.PublishAsync(new SettingChangedIntegrationEvent("Printer", null, request.Settings.DefaultPrinter), cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdatePrinterSettingsValidator : AbstractValidator<UpdatePrinterSettingsCommand>
{
    public UpdatePrinterSettingsValidator()
    {
        RuleFor(x => x.Settings.Copies)
            .InclusiveBetween(1, 99)
            .WithMessage("Copies must be between 1 and 99.");
        RuleFor(x => x.Settings.PaperWidth)
            .InclusiveBetween(20, 210)
            .WithMessage("Paper width must be between 20 and 210 mm.");
        RuleFor(x => x.Settings.MarginMm)
            .InclusiveBetween(0, 50)
            .WithMessage("Margins must be between 0 and 50 mm.");
        RuleFor(x => x.Settings.Orientation)
            .Must(o => o is "Portrait" or "Landscape")
            .WithMessage("Orientation must be Portrait or Landscape.");
    }
}
