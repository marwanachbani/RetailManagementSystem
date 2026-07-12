using System.Collections.ObjectModel;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.Modules.Reporting.Application.IntegrationEvents;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class FinancialReportViewModel : ReportViewModelBase
{
    private readonly ObservableCollection<FinancialReportItem> _items = new();
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private string _periodType = "Monthly";

    public override ObservableCollection<object> Items => new(_items);

    public ObservableCollection<FinancialReportItem> TypedItems => _items;

    public DateTime? FromDate
    {
        get => _fromDate;
        set { _fromDate = value; OnPropertyChanged(); }
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set { _toDate = value; OnPropertyChanged(); }
    }

    public string PeriodType
    {
        get => _periodType;
        set { _periodType = value; OnPropertyChanged(); }
    }

    public FinancialReportViewModel(IReportingReadStore readStore, IDialogService dialogService, IEventBus eventBus)
        : base(readStore, dialogService, eventBus)
    {
    }

    public override async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading financial report...";

        try
        {
            var result = await ReadStore.GetFinancialReportAsync(new DateRangeFilter(FromDate, ToDate), PeriodType);
            _items.Clear();
            foreach (var item in result.Items)
                _items.Add(item);
            StatusMessage = $"Loaded {result.TotalPeriods} periods. Revenue: {result.TotalRevenue:C}";

            await EventBus.PublishAsync(new ReportGeneratedIntegrationEvent("Financial", "View"), default);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading report: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(Items));
        }
    }
}
