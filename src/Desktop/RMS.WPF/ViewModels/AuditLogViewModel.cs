using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.GetAuditLogs;
using RMS.WPF.Commands;
using RMS.WPF.Services;
using RMS.WPF.Views;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace RMS.WPF.ViewModels;

public sealed class AuditLogViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private readonly IFolderBrowserService _folderBrowserService;

    private DateTime _fromDate = DateTime.Today.AddDays(-30);
    private DateTime _toDate = DateTime.Today;
    private string? _searchTerm;
    private string? _filterModule;
    private string? _filterAction;
    private string? _filterUserId;
    private string? _statusMessage;
    private AuditLogReadModel? _selectedEntry;

    public ObservableCollection<AuditLogReadModel> AuditLogs { get; } = new();

    public DateTime FromDate
    {
        get => _fromDate;
        set { _fromDate = value; OnPropertyChanged(); Refresh(); }
    }

    public DateTime ToDate
    {
        get => _toDate;
        set { _toDate = value; OnPropertyChanged(); Refresh(); }
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set { _searchTerm = value; OnPropertyChanged(); }
    }

    public string? FilterModule
    {
        get => _filterModule;
        set { _filterModule = value; OnPropertyChanged(); }
    }

    public string? FilterAction
    {
        get => _filterAction;
        set { _filterAction = value; OnPropertyChanged(); }
    }

    public string? FilterUserId
    {
        get => _filterUserId;
        set { _filterUserId = value; OnPropertyChanged(); }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public AuditLogReadModel? SelectedEntry
    {
        get => _selectedEntry;
        set { _selectedEntry = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand ExportPdfCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand PrintCommand { get; }
    public ICommand ShowDetailsCommand { get; }

    public AuditLogViewModel(IMediator mediator, IDialogService dialogService, IFolderBrowserService folderBrowserService)
    {
        _mediator = mediator;
        _dialogService = dialogService;
        _folderBrowserService = folderBrowserService;

        RefreshCommand = new RelayCommand(_ => Refresh());
        ExportPdfCommand = new RelayCommand(_ => _ = ExportPdfAsync());
        ExportCsvCommand = new RelayCommand(_ => _ = ExportCsvAsync());
        PrintCommand = new RelayCommand(_ => _ = PrintAsync());
        ShowDetailsCommand = new RelayCommand(_ => ShowDetails());

        Refresh();
    }

    private async void Refresh()
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(1, 100, FromDate, ToDate, FilterUserId, FilterModule, FilterAction, SearchTerm));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        AuditLogs.Clear();
        foreach (var entry in result.Value.Items)
            AuditLogs.Add(entry);

        StatusMessage = $"{result.Value.TotalCount} audit entries found";
    }

    private void ShowDetails()
    {
        if (SelectedEntry is null)
        {
            _dialogService.ShowWarning("Select an entry to view details.", "Audit Details");
            return;
        }

        var details = new AuditLogDetailsWindow
        {
            DataContext = new AuditLogDetailsViewModel(SelectedEntry)
        };
        details.ShowDialog();
    }

    private async Task ExportPdfAsync()
    {
        if (AuditLogs.Count == 0)
        {
            _dialogService.ShowWarning("No data to export.", "Export");
            return;
        }

        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.Header().Element(header => header.AlignCenter().Text("Audit Log").FontSize(20).Bold());
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(4).Text("Time").FontSize(9).Bold();
                            header.Cell().Border(1).Padding(4).Text("User").FontSize(9).Bold();
                            header.Cell().Border(1).Padding(4).Text("Module").FontSize(9).Bold();
                            header.Cell().Border(1).Padding(4).Text("Action").FontSize(9).Bold();
                            header.Cell().Border(1).Padding(4).Text("Entity").FontSize(9).Bold();
                            header.Cell().Border(1).Padding(4).Text("Entity Id").FontSize(9).Bold();
                        });

                        foreach (var item in AuditLogs)
                        {
                            table.Cell().Border(1).Padding(4).Text(item.Timestamp.ToString("yyyy-MM-dd HH:mm")).FontSize(8);
                            table.Cell().Border(1).Padding(4).Text(item.UserName).FontSize(8);
                            table.Cell().Border(1).Padding(4).Text(item.Module).FontSize(8);
                            table.Cell().Border(1).Padding(4).Text(item.Action).FontSize(8);
                            table.Cell().Border(1).Padding(4).Text(item.Entity).FontSize(8);
                            table.Cell().Border(1).Padding(4).Text(item.EntityId ?? string.Empty).FontSize(8);
                        }
                    });
                });
            });

            var tempPath = Path.Combine(Path.GetTempPath(), $"audit_{Guid.NewGuid():N}.pdf");
            document.GeneratePdf(tempPath);
            var bytes = await File.ReadAllBytesAsync(tempPath);
            File.Delete(tempPath);

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"AuditLog_{DateTime.Now:yyyyMMdd}.pdf",
                DefaultExt = ".pdf",
                Filter = "PDF files (*.pdf)|*.pdf"
            };
            if (saveDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
                _dialogService.ShowInfo($"Exported {AuditLogs.Count} rows as PDF.", "Export");
                StatusMessage = $"Exported {AuditLogs.Count} rows as PDF.";
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Export failed: {ex.Message}", "Export Error");
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }

    private async Task ExportCsvAsync()
    {
        if (AuditLogs.Count == 0)
        {
            _dialogService.ShowWarning("No data to export.", "Export");
            return;
        }

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Audit Log");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            var properties = typeof(AuditLogReadModel).GetProperties();
            sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            foreach (var item in AuditLogs)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item);
                    var str = value?.ToString() ?? string.Empty;
                    if (str.Contains(',') || str.Contains('"') || str.Contains('\n'))
                        str = $"\"{str.Replace("\"", "\"\"")}\"";
                    return str;
                });
                sb.AppendLine(string.Join(",", values));
            }

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"AuditLog_{DateTime.Now:yyyyMMdd}.csv",
                DefaultExt = ".csv",
                Filter = "CSV files (*.csv)|*.csv"
            };
            if (saveDialog.ShowDialog() == true)
            {
                await File.WriteAllTextAsync(saveDialog.FileName, sb.ToString(), Encoding.UTF8);
                _dialogService.ShowInfo($"Exported {AuditLogs.Count} rows as CSV.", "Export");
                StatusMessage = $"Exported {AuditLogs.Count} rows as CSV.";
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Export failed: {ex.Message}", "Export Error");
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }

    private async Task PrintAsync()
    {
        if (AuditLogs.Count == 0)
        {
            _dialogService.ShowWarning("No data to print.", "Print");
            return;
        }

        try
        {
            var printDialog = new System.Windows.Controls.PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(20);
                        page.Size(PageSizes.A4);
                        page.Header().Element(header => header.AlignCenter().Text("Audit Log").FontSize(20).Bold());
                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Border(1).Padding(4).Text("Time").FontSize(9).Bold();
                                header.Cell().Border(1).Padding(4).Text("User").FontSize(9).Bold();
                                header.Cell().Border(1).Padding(4).Text("Module").FontSize(9).Bold();
                                header.Cell().Border(1).Padding(4).Text("Action").FontSize(9).Bold();
                                header.Cell().Border(1).Padding(4).Text("Entity").FontSize(9).Bold();
                                header.Cell().Border(1).Padding(4).Text("Entity Id").FontSize(9).Bold();
                            });

                            foreach (var item in AuditLogs)
                            {
                                table.Cell().Border(1).Padding(4).Text(item.Timestamp.ToString("yyyy-MM-dd HH:mm")).FontSize(8);
                                table.Cell().Border(1).Padding(4).Text(item.UserName).FontSize(8);
                                table.Cell().Border(1).Padding(4).Text(item.Module).FontSize(8);
                                table.Cell().Border(1).Padding(4).Text(item.Action).FontSize(8);
                                table.Cell().Border(1).Padding(4).Text(item.Entity).FontSize(8);
                                table.Cell().Border(1).Padding(4).Text(item.EntityId ?? string.Empty).FontSize(8);
                            }
                        });
                    });
                });

                var tempPath = Path.Combine(Path.GetTempPath(), $"audit_print_{Guid.NewGuid():N}.pdf");
                document.GeneratePdf(tempPath);

                try
                {
                    Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                    _dialogService.ShowInfo("Print request sent. Check your default PDF viewer.", "Print");
                    StatusMessage = "Print requested.";
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Print failed: {ex.Message}", "Print Error");
                    StatusMessage = $"Print failed: {ex.Message}";
                }
                finally
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Print failed: {ex.Message}", "Print Error");
            StatusMessage = $"Print failed: {ex.Message}";
        }
    }
}
