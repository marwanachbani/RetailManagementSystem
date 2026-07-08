using System.Windows.Input;
using RMS.WPF.Commands;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class SalesViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly SalesHistoryViewModel _historyViewModel;

    public SalesViewModel(IServiceProvider services, SalesHistoryViewModel historyViewModel)
    {
        _services = services;
        _historyViewModel = historyViewModel;
        RefreshCommand = new RelayCommand(_ => _ = HistoryViewModel.LoadAsync());
        NewSaleCommand = new RelayCommand(_ => _ = OpenCreateSaleDialog());
    }

    public SalesHistoryViewModel HistoryViewModel => _historyViewModel;

    public ICommand RefreshCommand { get; }
    public ICommand NewSaleCommand { get; }

    private async Task OpenCreateSaleDialog()
    {
        var window = (CreateSaleWindow)_services.GetService(typeof(CreateSaleWindow))!;
        if (window.ShowDialog() == true)
            await HistoryViewModel.LoadAsync();
    }
}
