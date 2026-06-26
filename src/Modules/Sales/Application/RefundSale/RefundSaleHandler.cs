using MediatR;
using RMS.BuildingBlocks.Exceptions;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Domain.Entities;
using RMS.Modules.Sales.Application;

namespace RMS.Modules.Sales.Application.RefundSale;

public sealed class RefundSaleHandler : IRequestHandler<RefundSaleCommand, Result>
{
    private readonly ISaleReadStore _readStore;
    private readonly ISaleWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public RefundSaleHandler(ISaleReadStore readStore, ISaleWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(RefundSaleCommand request, CancellationToken cancellationToken)
    {
        var saleModel = await _readStore.GetByIdAsync(request.SaleId, cancellationToken);
        if (saleModel is null)
            return Result.Failure("Sale not found.", "Sales.NotFound");

        var sale = Sale.Rehydrate(
            saleModel.Id, saleModel.SaleNumber, saleModel.CashierId, saleModel.SaleDate,
            Enum.Parse<SaleStatus>(saleModel.Status), saleModel.SubTotal, saleModel.DiscountAmount,
            saleModel.TaxAmount, saleModel.TotalAmount, saleModel.DiscountPercentage, saleModel.TaxPercentage,
            saleModel.CompletedAt, saleModel.RefundedAt, saleModel.CreatedAt, saleModel.Notes);

        sale.RehydrateItems(saleModel.Items.Select(i => SaleItem.Create(i.Id, sale.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)));

        try
        {
            sale.Refund(request.Reason);

            await _writeStore.UpdateAsync(sale, cancellationToken);

            foreach (var item in saleModel.Items)
            {
                await _eventBus.PublishAsync(
                    new StockRestorationRequestedEvent(
                        sale.Id, item.ProductId, item.ProductName, item.Quantity, $"Refund for sale {sale.SaleNumber}"),
                    cancellationToken);
            }

            await _eventBus.PublishAsync(
                new SaleRefundedIntegrationEvent(sale.Id, sale.SaleNumber, sale.TotalAmount),
                cancellationToken);

            sale.ClearDomainEvents();
            return Result.Success();
        }
        catch (BusinessRuleValidationException ex)
        {
            return Result.Failure(ex.Message, ex.RuleName);
        }
    }
}
