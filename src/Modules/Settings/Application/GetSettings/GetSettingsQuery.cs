using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.Models;
using RMS.Modules.Settings.Application.Services;

namespace RMS.Modules.Settings.Application.GetSettings;

public sealed record GetSettingsQuery : IRequest<Result<SettingsModel>>;

public sealed class GetSettingsHandler : IRequestHandler<GetSettingsQuery, Result<SettingsModel>>
{
    private readonly ISettingsReadStore _readStore;
    private readonly IFolderResolver _resolver;

    public GetSettingsHandler(ISettingsReadStore readStore, IFolderResolver resolver)
    {
        _readStore = readStore;
        _resolver = resolver;
    }

    public async Task<Result<SettingsModel>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var values = await _readStore.GetAllValuesAsync(cancellationToken);
        var model = SettingsModelMapper.ToModel(values, _resolver);
        return Result.Success(model);
    }
}
