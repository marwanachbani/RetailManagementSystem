using System.Collections.ObjectModel;
using MediatR;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Application.GetCategories;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public abstract class ProductFormViewModelBase : ViewModelBase
{
    protected readonly IMediator Mediator;
    protected readonly IDialogService DialogService;
    private CategoryReadModel? _selectedCategory;
    private string? _errorMessage;

    protected ProductFormViewModelBase(IMediator mediator, IDialogService dialogService)
    {
        Mediator = mediator;
        DialogService = dialogService;
    }

    public ObservableCollection<CategoryReadModel> Categories { get; } = new();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }

    public CategoryReadModel? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            OnPropertyChanged();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        protected set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Validate the common required fields, showing a popup for the first
    /// problem found. Returns true when the form is valid.</summary>
    protected bool ValidateRequiredFields()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Fail("Product name is required.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(Barcode))
        {
            Fail("Barcode is required.");
            return false;
        }
        if (SelectedCategory is null)
        {
            Fail("Choose a category.");
            return false;
        }
        if (SalePrice <= 0)
        {
            Fail("Sale price must be greater than zero.");
            return false;
        }
        if (CostPrice < 0)
        {
            Fail("Cost price cannot be negative.");
            return false;
        }
        return true;
    }

    protected void Fail(string message)
    {
        ErrorMessage = message;
        DialogService.ShowWarning(message);
    }

    public async Task LoadCategoriesAsync()
    {
        var result = await Mediator.Send(new GetCategoriesQuery());
        if (result.IsFailure)
        {
            ErrorMessage = result.Error;
            return;
        }

        Categories.Clear();
        foreach (var category in result.Value)
            Categories.Add(category);

        SelectedCategory ??= Categories.FirstOrDefault();
    }
}
