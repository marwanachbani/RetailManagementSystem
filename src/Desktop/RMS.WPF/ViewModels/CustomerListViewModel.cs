using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Customers.Application.Contracts;
using RMS.Modules.Customers.Application.DeactivateCustomer;
using RMS.Modules.Customers.Application.GetCustomersPaged;
using RMS.Modules.Customers.Application.ReactivateCustomer;
using RMS.WPF.Commands;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class CustomerListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _services;
    private string? _statusMessage;
    private CustomerReadModel? _selectedCustomer;
    private string? _searchTerm;
    private bool _includeInactive;
    private int _pageNumber = 1;
    private int _totalPages = 1;
    private bool _isLoading;

    public CustomerListViewModel(IMediator mediator, IServiceProvider services)
    {
        _mediator = mediator;
        _services = services;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
        NewCustomerCommand = new RelayCommand(_ => _ = OpenCreateCustomerDialog());
        EditCustomerCommand = new RelayCommand(_ => _ = OpenEditCustomerDialog(), _ => SelectedCustomer is not null);
        ViewDetailsCommand = new RelayCommand(_ => _ = OpenDetailsDialog(), _ => SelectedCustomer is not null);
        DeactivateCommand = new RelayCommand(_ => _ = DeactivateAsync(), _ => SelectedCustomer?.Status == "Active");
        ReactivateCommand = new RelayCommand(_ => _ = ReactivateAsync(), _ => SelectedCustomer?.Status == "Inactive");
        NextPageCommand = new RelayCommand(_ => _ = NextPageAsync(), _ => PageNumber < TotalPages);
        PreviousPageCommand = new RelayCommand(_ => _ = PreviousPageAsync(), _ => PageNumber > 1);
        _ = LoadAsync();
    }

    public ObservableCollection<CustomerReadModel> Customers { get; } = new();
    public int PageSize { get; } = 25;

    public CustomerReadModel? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            _selectedCustomer = value;
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
    public ICommand NewCustomerCommand { get; }
    public ICommand EditCustomerCommand { get; }
    public ICommand ViewDetailsCommand { get; }
    public ICommand DeactivateCommand { get; }
    public ICommand ReactivateCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        var result = await _mediator.Send(new GetCustomersPagedQuery(PageNumber, PageSize, SearchTerm, IncludeInactive));
        IsLoading = false;

        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        Customers.Clear();
        foreach (var customer in result.Value.Items)
            Customers.Add(customer);

        TotalPages = Math.Max(1, result.Value.TotalPages);
        StatusMessage = $"{result.Value.TotalCount} customers";
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

    private async Task OpenCreateCustomerDialog()
    {
        var window = (CreateCustomerWindow)_services.GetService(typeof(CreateCustomerWindow))!;
        if (window.ShowDialog() == true)
            await LoadAsync();
    }

    private async Task OpenEditCustomerDialog()
    {
        if (SelectedCustomer is null) return;
        var window = (EditCustomerWindow)_services.GetService(typeof(EditCustomerWindow))!;
        var viewModel = (EditCustomerViewModel)window.DataContext;
        viewModel.LoadCustomer(SelectedCustomer);
        if (window.ShowDialog() == true)
            await LoadAsync();
    }

    private async Task OpenDetailsDialog()
    {
        if (SelectedCustomer is null) return;
        MessageBox.Show(
            $"Customer: {SelectedCustomer.FullName}\nCode: {SelectedCustomer.CustomerCode}\nPhone: {SelectedCustomer.PhoneNumber}\nEmail: {SelectedCustomer.Email ?? "N/A"}\nAddress: {SelectedCustomer.Street ?? "N/A"}, {SelectedCustomer.City ?? "N/A"}\nStatus: {SelectedCustomer.Status}",
            "Customer Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        await Task.CompletedTask;
    }

    private async Task DeactivateAsync()
    {
        if (SelectedCustomer is null) return;
        var result = MessageBox.Show(
            $"Are you sure you want to deactivate {SelectedCustomer.FullName}?",
            "Confirm Deactivation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        var commandResult = await _mediator.Send(new DeactivateCustomerCommand(SelectedCustomer.Id));
        if (commandResult.IsSuccess)
            await LoadAsync();
        else
            StatusMessage = commandResult.Error;
    }

    private async Task ReactivateAsync()
    {
        if (SelectedCustomer is null) return;
        var result = MessageBox.Show(
            $"Are you sure you want to reactivate {SelectedCustomer.FullName}?",
            "Confirm Reactivation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        var commandResult = await _mediator.Send(new ReactivateCustomerCommand(SelectedCustomer.Id));
        if (commandResult.IsSuccess)
            await LoadAsync();
        else
            StatusMessage = commandResult.Error;
    }
}
