using System.Windows.Input;
using MediatR;
using RMS.Modules.Products.Application.GetProductById;
using RMS.Modules.Products.Application.UpdateProduct;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class EditProductViewModel : ProductFormViewModelBase
{
    private Guid _productId;

    public EditProductViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService)
    {
        SaveCommand = new RelayCommand(_ => _ = SaveAsync());
    }

    public ICommand SaveCommand { get; }
    public event EventHandler? Saved;

    public async Task LoadProductAsync(Guid productId)
    {
        _productId = productId;
        await LoadCategoriesAsync();

        var result = await Mediator.Send(new GetProductByIdQuery(productId));
        if (result.IsFailure)
        {
            ErrorMessage = result.Error;
            return;
        }

        Name = result.Value.Name;
        Description = result.Value.Description;
        Barcode = result.Value.Barcode;
        CostPrice = result.Value.CostPrice;
        SalePrice = result.Value.SalePrice;
        SelectedCategory = Categories.FirstOrDefault(x => x.Id == result.Value.CategoryId);
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(Barcode));
        OnPropertyChanged(nameof(CostPrice));
        OnPropertyChanged(nameof(SalePrice));
    }

    private async Task SaveAsync()
    {
        if (!ValidateRequiredFields()) return;

        var result = await Mediator.Send(new UpdateProductCommand(_productId, Name, Description, Barcode, SelectedCategory!.Id, SalePrice, CostPrice));
        if (result.IsFailure)
        {
            Fail(result.Error ?? "Could not update the product.");
            return;
        }

        Saved?.Invoke(this, EventArgs.Empty);
    }
}
