using System.Windows.Input;
using MediatR;
using RMS.Modules.Products.Application.CreateProduct;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class CreateProductViewModel : ProductFormViewModelBase
{
    public CreateProductViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService)
    {
        SaveCommand = new RelayCommand(_ => _ = SaveAsync());
        _ = LoadCategoriesAsync();
    }

    public ICommand SaveCommand { get; }
    public event EventHandler? Saved;

    private async Task SaveAsync()
    {
        if (!ValidateRequiredFields()) return;

        var result = await Mediator.Send(new CreateProductCommand(Name, Description, Barcode, SelectedCategory!.Id, SalePrice, CostPrice));
        if (result.IsFailure)
        {
            Fail(result.Error ?? "Could not create the product.");
            return;
        }

        Saved?.Invoke(this, EventArgs.Empty);
    }
}
