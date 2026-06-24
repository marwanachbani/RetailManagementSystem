using System.Windows.Input;
using MediatR;
using RMS.Modules.Products.Application.CreateProduct;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class CreateProductViewModel : ProductFormViewModelBase
{
    public CreateProductViewModel(IMediator mediator) : base(mediator)
    {
        SaveCommand = new RelayCommand(_ => _ = SaveAsync());
    }

    public ICommand SaveCommand { get; }
    public event EventHandler? Saved;

    private async Task SaveAsync()
    {
        if (SelectedCategory is null)
        {
            ErrorMessage = "Choose a category.";
            return;
        }

        var result = await Mediator.Send(new CreateProductCommand(Name, Description, Barcode, SelectedCategory.Id, SalePrice, CostPrice));
        if (result.IsFailure)
        {
            ErrorMessage = result.Error;
            return;
        }

        Saved?.Invoke(this, EventArgs.Empty);
    }
}
