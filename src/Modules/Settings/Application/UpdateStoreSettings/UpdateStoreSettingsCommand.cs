using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateStoreSettings;

public sealed record UpdateStoreSettingsCommand(StoreSettingsModel Settings) : IRequest<Result>;

public sealed class UpdateStoreSettingsHandler : IRequestHandler<UpdateStoreSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;

    public UpdateStoreSettingsHandler(ISettingsWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public async Task<Result> Handle(UpdateStoreSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.StorePairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
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
