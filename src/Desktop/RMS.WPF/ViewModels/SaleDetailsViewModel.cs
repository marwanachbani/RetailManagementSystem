using System.Collections.ObjectModel;
using MediatR;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Application.GetSaleById;

namespace RMS.WPF.ViewModels;

public sealed class SaleDetailsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private SaleReadModel? _sale;
    private string? _statusMessage;

    public SaleDetailsViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public ObservableCollection<SaleItemReadModel> Items { get; } = new();

    public SaleReadModel? Sale
    {
        get => _sale;
        private set
        {
            _sale = value;
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

    public async Task LoadAsync(Guid saleId)
    {
        var result = await _mediator.Send(new GetSaleByIdQuery(saleId));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        Sale = result.Value;
        Items.Clear();
        foreach (var item in result.Value.Items)
            Items.Add(item);
    }
}
