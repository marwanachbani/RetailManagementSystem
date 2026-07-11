using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.Models;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateReceiptSettings;

public sealed record UpdateReceiptSettingsCommand(ReceiptSettingsModel Settings) : IRequest<Result>;

public sealed class UpdateReceiptSettingsHandler : IRequestHandler<UpdateReceiptSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;

    public UpdateReceiptSettingsHandler(ISettingsWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public async Task<Result> Handle(UpdateReceiptSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.ReceiptPairs(request.Settings);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateReceiptSettingsValidator : AbstractValidator<UpdateReceiptSettingsCommand>
{
    public UpdateReceiptSettingsValidator()
    {
        RuleFor(x => x.Settings.PaperWidth)
            .InclusiveBetween(20, 200)
            .WithMessage("Paper width must be between 20 and 200 mm.");
        RuleFor(x => x.Settings.Header).MaximumLength(200).WithMessage("Receipt header is too long.");
        RuleFor(x => x.Settings.Footer).MaximumLength(200).WithMessage("Receipt footer is too long.");
    }
}
