using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Application.GetDailySalesSummary;
using RMS.Modules.Sales.Application.GetSalesByDate;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class SalesHistoryViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private DateTime _selectedDate = DateTime.Today;
    private string? _statusMessage;
    private DailySalesSummary? _summary;

    public SalesHistoryViewModel(IMediator mediator)
    {
        _mediator = mediator;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
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
}
