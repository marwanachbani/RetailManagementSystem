using System.Collections.ObjectModel;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class CustomerReportViewModel : ReportViewModelBase
{
    private readonly ObservableCollection<CustomerReportItem> _items = new();
    private string? _searchTerm;
    private bool _includeInactive;

    public override ObservableCollection<object> Items => new(_items);

    public ObservableCollection<CustomerReportItem> TypedItems => _items;

    public string? SearchTerm
    {
        get => _searchTerm;
        set { _searchTerm = value; OnPropertyChanged(); }
    }

    public bool IncludeInactive
    {
        get => _includeInactive;
        set { _includeInactive = value; OnPropertyChanged(); }
    }

    public CustomerReportViewModel(IReportingReadStore readStore, IDialogService dialogService)
        : base(readStore, dialogService)
    {
    }

    public override async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading customer report...";

        try
        {
            var result = await ReadStore.GetCustomerReportAsync(SearchTerm, IncludeInactive);
            _items.Clear();
            foreach (var item in result.Items)
                _items.Add(item);
            StatusMessage = $"Loaded {result.TotalCount} customers";
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
