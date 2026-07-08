using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Application.GetDailySalesSummary;
using RMS.Modules.Sales.Application.GetSalesByDate;
using RMS.Modules.Sales.Application.RefundSale;
using RMS.WPF.Commands;
using RMS.WPF.Services;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class SalesHistoryViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogService;
    private DateTime _selectedDate = DateTime.Today;
    private string? _statusMessage;
    private DailySalesSummary? _summary;
    private SaleReadModel? _selectedSale;

    public SalesHistoryViewModel(IMediator mediator, IServiceProvider services, IDialogService dialogService)
    {
        _mediator = mediator;
        _services = services;
        _dialogService = dialogService;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
        EditCommand = new RelayCommand(o => _ = EditSaleAsync((SaleReadModel)o!));
        DeleteCommand = new RelayCommand(o => _ = DeleteSaleAsync((SaleReadModel)o!));
        _ = LoadAsync();
    }

    public ObservableCollection<SaleReadModel> Sales { get; } = new();

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            OnPropertyChanged();
            _ = LoadAsync();
        }
    }

    public SaleReadModel? SelectedSale
    {
        get => _selectedSale;
        set
        {
            _selectedSale = value;
            OnPropertyChanged();
        }
    }

    public DailySalesSummary? Summary
    {
        get => _summary;
        private set
        {
            _summary = value;
            OnPropertyChanged();
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

    public ICommand RefreshCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public async Task LoadAsync()
    {
        var salesResult = await _mediator.Send(new GetSalesByDateQuery(SelectedDate));
        if (salesResult.IsFailure)
        {
            StatusMessage = salesResult.Error;
            return;
        }

        Sales.Clear();
        foreach (var sale in salesResult.Value)
            Sales.Add(sale);

        var summaryResult = await _mediator.Send(new GetDailySalesSummaryQuery(SelectedDate));
        if (summaryResult.IsSuccess)
            Summary = summaryResult.Value;

        StatusMessage = $"{Sales.Count} sales for {SelectedDate:yyyy-MM-dd}";
    }

    private async Task EditSaleAsync(SaleReadModel sale)
    {
        if (sale.Status == "Pending")
        {
            // A pending sale hasn't been completed yet — resume it in the same
            // screen used to build a sale, instead of a read-only viewer.
            var window = (CreateSaleWindow)_services.GetService(typeof(CreateSaleWindow))!;
            window.ResumeSaleId = sale.Id;
            if (window.ShowDialog() == true)
                await LoadAsync();
            return;
        }

        // Completed/refunded sales are financial records and shouldn't be mutated —
        // show a read-only breakdown instead.
        var detailsWindow = (SaleDetailsWindow)_services.GetService(typeof(SaleDetailsWindow))!;
        var vm = (SaleDetailsViewModel)detailsWindow.DataContext;
        await vm.LoadAsync(sale.Id);
        detailsWindow.ShowDialog();
    }

    private async Task DeleteSaleAsync(SaleReadModel sale)
    {
        if (sale.Status == "Refunded")
        {
            _dialogService.ShowInfo("This sale has already been voided.");
            return;
        }

        if (!_dialogService.Confirm(
                $"Delete sale \"{sale.SaleNumber}\"?\n\nThis voids the sale and restores stock for its items. This cannot be undone.",
                "Delete Sale"))
            return;

        var result = await _mediator.Send(new RefundSaleCommand(sale.Id, "Deleted from Sales History"));
        if (result.IsFailure)
        {
            _dialogService.ShowError(result.Error ?? "Could not delete this sale.");
            return;
        }

        await LoadAsync();
    }
}
