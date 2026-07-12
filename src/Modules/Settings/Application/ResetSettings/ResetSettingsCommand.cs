using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Settings.Application.Models;
using RMS.Modules.Settings.Application.Services;
using RMS.Modules.Settings.Domain;

namespace RMS.Modules.Settings.Application.ResetSettings;

/// <summary>
/// Restores every setting to its catalog default value.
/// </summary>
public sealed record ResetSettingsCommand : IRequest<Result<SettingsModel>>;

public sealed class ResetSettingsHandler : IRequestHandler<ResetSettingsCommand, Result<SettingsModel>>
{
    private readonly ISettingsWriteStore _writeStore;
    private readonly ISettingsReadStore _readStore;
    private readonly IFolderResolver _resolver;
    private readonly IEventBus _eventBus;

    public ResetSettingsHandler(
        ISettingsWriteStore writeStore,
        ISettingsReadStore readStore,
        IFolderResolver resolver,
        IEventBus eventBus)
    {
        _writeStore = writeStore;
        _readStore = readStore;
        _resolver = resolver;
        _eventBus = eventBus;
    }

    public async Task<Result<SettingsModel>> Handle(ResetSettingsCommand request, CancellationToken cancellationToken)
    {
        await _writeStore.ResetToDefaultsAsync(cancellationToken);

        foreach (var folder in SettingCatalog.FolderDefinitions)
            _resolver.EnsureExists(_resolver.GetDefaultPath(folder.FolderSubPath!));

        await _eventBus.PublishAsync(new SettingsResetIntegrationEvent(), cancellationToken);

        var values = await _readStore.GetAllValuesAsync(cancellationToken);
        var model = SettingsModelMapper.ToModel(values, _resolver);
        return Result.Success(model);
    }
}
