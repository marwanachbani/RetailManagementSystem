using System.Collections.ObjectModel;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class ProductReportViewModel : ReportViewModelBase
{
    private readonly ObservableCollection<ProductReportItem> _items = new();
    private string? _searchTerm;
    private string? _selectedSortColumn = nameof(ProductReportItem.TotalRevenue);
    private bool _sortDescending = true;

    public override ObservableCollection<object> Items => new(_items);

    public ObservableCollection<ProductReportItem> TypedItems => _items;

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

    public ProductReportViewModel(IReportingReadStore readStore, IDialogService dialogService)
        : base(readStore, dialogService)
    {
    }

    public override async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading product report...";

        try
        {
            var result = await ReadStore.GetProductReportAsync(SearchTerm, SelectedSortColumn, SortDescending);
            _items.Clear();
            foreach (var item in result.Items)
                _items.Add(item);
            StatusMessage = $"Loaded {result.TotalCount} products";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading report: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
