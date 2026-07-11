using System.Collections.ObjectModel;
using System.Windows.Input;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class SalesReportViewModel : ReportViewModelBase
{
    private readonly ObservableCollection<SalesReportItem> _items = new();
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private string? _searchTerm;
    private string? _selectedSortColumn = nameof(SalesReportItem.SaleDate);
    private bool _sortDescending = true;

    public override ObservableCollection<object> Items => new(_items);

    public ObservableCollection<SalesReportItem> TypedItems => _items;

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

    public string? SearchTerm
    {
        get => _searchTerm;
        set { _searchTerm = value; OnPropertyChanged(); }
    }

    public string? SelectedSortColumn
    {
        get => _selectedSortColumn;
        set { _selectedSortColumn = value; OnPropertyChanged(); }
    }

    public bool SortDescending
    {
        get => _sortDescending;
        set { _sortDescending = value; OnPropertyChanged(); }
    }

    public SalesReportViewModel(IReportingReadStore readStore, IDialogService dialogService)
        : base(readStore, dialogService)
    {
    }

    public override async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading sales report...";

        try
        {
            var dateRange = new DateRangeFilter(FromDate, ToDate);
            var result = await ReadStore.GetSalesReportAsync(dateRange, SearchTerm, SelectedSortColumn, SortDescending);

            _items.Clear();
            foreach (var item in result.Items)
                _items.Add(item);

            StatusMessage = $"Loaded {result.TotalCount} sales. Revenue: {result.GrandTotalRevenue:C}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading sales report: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(Items));
        }
    }
}
