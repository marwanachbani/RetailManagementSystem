using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.Modules.Suppliers.Application.DeactivateSupplier;
using RMS.Modules.Suppliers.Application.GetSuppliersPaged;
using RMS.Modules.Suppliers.Application.ReactivateSupplier;
using RMS.WPF.Commands;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class SupplierListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _services;
    private string? _statusMessage;
    private SupplierReadModel? _selectedSupplier;
    private string? _searchTerm;
    private bool _includeInactive;
    private int _pageNumber = 1;
    private int _totalPages = 1;
    private bool _isLoading;

    public SupplierListViewModel(IMediator mediator, IServiceProvider services)
    {
        _mediator = mediator;
        _services = services;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
        NewSupplierCommand = new RelayCommand(_ => _ = OpenCreateSupplierDialog());
        EditSupplierCommand = new RelayCommand(_ => _ = OpenEditSupplierDialog(), _ => SelectedSupplier is not null);
        ViewDetailsCommand = new RelayCommand(_ => _ = OpenDetailsDialog(), _ => SelectedSupplier is not null);
        DeactivateCommand = new RelayCommand(_ => _ = DeactivateAsync(), _ => SelectedSupplier?.Status == "Active");
        ReactivateCommand = new RelayCommand(_ => _ = ReactivateAsync(), _ => SelectedSupplier?.Status == "Inactive");
        NextPageCommand = new RelayCommand(_ => _ = NextPageAsync(), _ => PageNumber < TotalPages);
        PreviousPageCommand = new RelayCommand(_ => _ = PreviousPageAsync(), _ => PageNumber > 1);
        _ = LoadAsync();
    }

    public ObservableCollection<SupplierReadModel> Suppliers { get; } = new();
    public int PageSize { get; } = 25;

    public SupplierReadModel? SelectedSupplier
    {
        get => _selectedSupplier;
        set
        {
            _selectedSupplier = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set
        {
            _searchTerm = value;
            OnPropertyChanged();
        }
    }

    public bool IncludeInactive
    {
        get => _includeInactive;
        set
        {
            _includeInactive = value;
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
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand NewSupplierCommand { get; }
    public ICommand EditSupplierCommand { get; }
    public ICommand ViewDetailsCommand { get; }
    public ICommand DeactivateCommand { get; }
    public ICommand ReactivateCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        var result = await _mediator.Send(new GetSuppliersPagedQuery(PageNumber, PageSize, SearchTerm, IncludeInactive));
        IsLoading = false;

        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        Suppliers.Clear();
        foreach (var supplier in result.Value.Items)
            Suppliers.Add(supplier);

        TotalPages = Math.Max(1, result.Value.TotalPages);
        StatusMessage = $"{result.Value.TotalCount} suppliers";
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

    private async Task OpenCreateSupplierDialog()
    {
        var window = (CreateSupplierWindow)_services.GetService(typeof(CreateSupplierWindow))!;
        if (window.ShowDialog() == true)
            await LoadAsync();
    }

    private async Task OpenEditSupplierDialog()
    {
        if (SelectedSupplier is null) return;
        var window = (EditSupplierWindow)_services.GetService(typeof(EditSupplierWindow))!;
        var viewModel = (EditSupplierViewModel)window.DataContext;
        viewModel.LoadSupplier(SelectedSupplier);
        if (window.ShowDialog() == true)
            await LoadAsync();
    }

    private async Task OpenDetailsDialog()
    {
        if (SelectedSupplier is null) return;
        MessageBox.Show(
            $"Supplier: {SelectedSupplier.CompanyName}\nCode: {SelectedSupplier.SupplierCode}\nContact: {SelectedSupplier.ContactPerson ?? "N/A"}\nPhone: {SelectedSupplier.PhoneNumber}\nEmail: {SelectedSupplier.Email ?? "N/A"}\nVAT: {SelectedSupplier.VatNumber ?? "N/A"}\nAddress: {SelectedSupplier.Street ?? "N/A"}, {SelectedSupplier.City ?? "N/A"}\nStatus: {SelectedSupplier.Status}",
            "Supplier Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        await Task.CompletedTask;
    }

    private async Task DeactivateAsync()
    {
        if (SelectedSupplier is null) return;
        var result = MessageBox.Show(
            $"Are you sure you want to deactivate {SelectedSupplier.CompanyName}?",
            "Confirm Deactivation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        var commandResult = await _mediator.Send(new DeactivateSupplierCommand(SelectedSupplier.Id));
        if (commandResult.IsSuccess)
            await LoadAsync();
        else
            StatusMessage = commandResult.Error;
    }

    private async Task ReactivateAsync()
    {
        if (SelectedSupplier is null) return;
        var result = MessageBox.Show(
            $"Are you sure you want to reactivate {SelectedSupplier.CompanyName}?",
            "Confirm Reactivation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        var commandResult = await _mediator.Send(new ReactivateSupplierCommand(SelectedSupplier.Id));
        if (commandResult.IsSuccess)
            await LoadAsync();
        else
            StatusMessage = commandResult.Error;
    }
}
