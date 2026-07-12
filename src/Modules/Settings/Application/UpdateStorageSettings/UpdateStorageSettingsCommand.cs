using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Settings.Application.Models;
using RMS.Modules.Settings.Application.Services;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateStorageSettings;

public sealed record UpdateStorageSettingsCommand(IReadOnlyList<FolderSettingModel> Folders) : IRequest<Result>;

public sealed class UpdateStorageSettingsHandler : IRequestHandler<UpdateStorageSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;
    private readonly IFolderResolver _resolver;
    private readonly IEventBus _eventBus;

    public UpdateStorageSettingsHandler(ISettingsWriteStore writeStore, IFolderResolver resolver, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _resolver = resolver;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateStorageSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = request.Folders
            .Where(f => !string.IsNullOrWhiteSpace(f.Key))
            .ToDictionary(f => f.Key, f => (string?)f.Path);

        await _writeStore.UpsertManyAsync(pairs, cancellationToken);

        foreach (var folder in request.Folders)
        {
            if (!string.IsNullOrWhiteSpace(folder.Path))
            {
                var oldPath = _resolver.GetDefaultPath(folder.Key);
                _resolver.EnsureExists(folder.Path);
                await _eventBus.PublishAsync(new FolderChangedIntegrationEvent(folder.Key, oldPath, folder.Path), cancellationToken);
            }
        }

        return Result.Success();
    }
}

public sealed class UpdateStorageSettingsValidator : AbstractValidator<UpdateStorageSettingsCommand>
{
    public UpdateStorageSettingsValidator()
    {
        RuleForEach(x => x.Folders).ChildRules(folder =>
        {
            folder.RuleFor(f => f.Key).NotEmpty().WithMessage("Folder key is required.");
            folder.RuleFor(f => f.Path)
                .NotEmpty().WithMessage("Folder path is required.")
                .Must(BeValidPath).WithMessage("Folder path is not valid.");
        });
    }

    private static bool BeValidPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        // Characters that are never valid inside a Windows path component.
        if (path.IndexOfAny(new[] { '<', '>', '"', '|', '?', '*' }) >= 0) return false;
        try
        {
            Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
