using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Application.CreateSale;
using RMS.Modules.Sales.Application.GetSalesPaged;
using RMS.WPF.Commands;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class SalesViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _services;
    private string? _statusMessage;
    private SaleReadModel? _selectedSale;
    private int _pageNumber = 1;
    private int _totalPages = 1;

    public SalesViewModel(IMediator mediator, IServiceProvider services)
    {
        _mediator = mediator;
        _services = services;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
        NewSaleCommand = new RelayCommand(_ => _ = OpenCreateSaleDialog());
        ViewHistoryCommand = new RelayCommand(_ => _ = OpenHistoryDialog());
        NextPageCommand = new RelayCommand(_ => _ = NextPageAsync(), _ => PageNumber < TotalPages);
        PreviousPageCommand = new RelayCommand(_ => _ = PreviousPageAsync(), _ => PageNumber > 1);
        _ = LoadAsync();
    }

    public ObservableCollection<SaleReadModel> Sales { get; } = new();
    public int PageSize { get; } = 25;

    public SaleReadModel? SelectedSale
    {
        get => _selectedSale;
        set
        {
            _selectedSale = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public int PageNumber
    {
        get => _pageNumber;
        private set
        {
            _pageNumber = value;
            OnPropertyChanged();
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            _totalPages = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand NewSaleCommand { get; }
    public ICommand ViewHistoryCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    public async Task LoadAsync()
    {
        var result = await _mediator.Send(new GetSalesPagedQuery(PageNumber, PageSize));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        Sales.Clear();
        foreach (var sale in result.Value.Items)
            Sales.Add(sale);

        TotalPages = Math.Max(1, result.Value.TotalPages);
        StatusMessage = $"{result.Value.TotalCount} sales";
    }

    private async Task NextPageAsync()
    {
        PageNumber++;
        await LoadAsync();
    }

    private async Task PreviousPageAsync()
    {
        PageNumber--;
        await LoadAsync();
    }

    private async Task OpenCreateSaleDialog()
    {
        var window = (CreateSaleWindow)_services.GetService(typeof(CreateSaleWindow))!;
        if (window.ShowDialog() == true)
            await LoadAsync();
    }

    private async Task OpenHistoryDialog()
    {
        var window = (SalesHistoryWindow)_services.GetService(typeof(SalesHistoryWindow))!;
        if (window.ShowDialog() == true)
            await LoadAsync();
    }
}
