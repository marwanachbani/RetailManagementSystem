using System.Collections.ObjectModel;
using MediatR;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Application.GetCategories;

namespace RMS.WPF.ViewModels;

public abstract class ProductFormViewModelBase : ViewModelBase
{
    protected readonly IMediator Mediator;
    private CategoryReadModel? _selectedCategory;
    private string? _errorMessage;

    protected ProductFormViewModelBase(IMediator mediator)
    {
        Mediator = mediator;
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
