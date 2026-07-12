using System.Collections.ObjectModel;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.Modules.Reporting.Application.IntegrationEvents;
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

    public CustomerReportViewModel(IReportingReadStore readStore, IDialogService dialogService, IEventBus eventBus)
        : base(readStore, dialogService, eventBus)
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

            await EventBus.PublishAsync(new ReportGeneratedIntegrationEvent("Customer", "View"), default);
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
