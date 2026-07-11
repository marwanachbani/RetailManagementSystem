using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.Models;
using RMS.Modules.Settings.Application.Services;
using FluentValidation;

namespace RMS.Modules.Settings.Application.UpdateReportSettings;

public sealed record UpdateReportSettingsCommand(ReportSettingsModel Settings) : IRequest<Result>;

public sealed class UpdateReportSettingsHandler : IRequestHandler<UpdateReportSettingsCommand, Result>
{
    private readonly ISettingsWriteStore _writeStore;
    private readonly IFolderResolver _resolver;

    public UpdateReportSettingsHandler(ISettingsWriteStore writeStore, IFolderResolver resolver)
    {
        _writeStore = writeStore;
        _resolver = resolver;
    }

    public async Task<Result> Handle(UpdateReportSettingsCommand request, CancellationToken cancellationToken)
    {
        var pairs = SettingsModelMapper.ReportPairs(request.Settings, _resolver);
        await _writeStore.UpsertManyAsync(pairs, cancellationToken);
        _resolver.EnsureExists(request.Settings.DefaultReportFolder);
        return Result.Success();
    }
}

public sealed class UpdateReportSettingsValidator : AbstractValidator<UpdateReportSettingsCommand>
{
    private static readonly string[] Orientations = { "Portrait", "Landscape" };
    private static readonly string[] Qualities = { "Draft", "Standard", "High" };
    private static readonly string[] Formats = { "Xlsx", "Xls", "Csv" };

    public UpdateReportSettingsValidator()
    {
        RuleFor(x => x.Settings.DefaultReportFolder)
            .NotEmpty().WithMessage("Default report folder is required.")
            .Must(BeRootedPath).WithMessage("Default report folder must be a valid absolute path.");
        RuleFor(x => x.Settings.FileNamePattern).NotEmpty().WithMessage("Report file name pattern is required.");
        RuleFor(x => x.Settings.CsvDelimiter).NotEmpty().WithMessage("CSV delimiter is required.");
        RuleFor(x => x.Settings.PrintOrientation)
            .Must(o => Orientations.Contains(o)).WithMessage("Print orientation must be Portrait or Landscape.");
        RuleFor(x => x.Settings.PdfQuality)
            .Must(q => Qualities.Contains(q)).WithMessage("PDF quality must be Draft, Standard or High.");
        RuleFor(x => x.Settings.ExcelExportFormat)
            .Must(f => Formats.Contains(f)).WithMessage("Excel export format must be Xlsx, Xls or Csv.");
    }

    private static bool BeRootedPath(string path) =>
        !string.IsNullOrWhiteSpace(path) && Path.IsPathRooted(path);
}
