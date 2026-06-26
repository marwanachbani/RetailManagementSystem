using MediatR;
using RMS.BuildingBlocks.Exceptions;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Domain.Entities;

namespace RMS.Modules.Sales.Application.RemoveSaleItem;

public sealed class RemoveSaleItemHandler : IRequestHandler<RemoveSaleItemCommand, Result>
{
    private readonly ISaleReadStore _readStore;
    private readonly ISaleWriteStore _writeStore;

    public RemoveSaleItemHandler(ISaleReadStore readStore, ISaleWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async Task<Result> Handle(RemoveSaleItemCommand request, CancellationToken cancellationToken)
    {
        var saleModel = await _readStore.GetByIdAsync(request.SaleId, cancellationToken);
        if (saleModel is null)
            return Result.Failure("Sale not found.", "Sales.NotFound");

        var sale = Sale.Rehydrate(
            saleModel.Id, saleModel.SaleNumber, saleModel.CashierId, saleModel.SaleDate,
            Enum.Parse<SaleStatus>(saleModel.Status), saleModel.SubTotal, saleModel.DiscountAmount,
            saleModel.TaxAmount, saleModel.TotalAmount, saleModel.DiscountPercentage, saleModel.TaxPercentage,
            saleModel.CompletedAt, saleModel.RefundedAt, saleModel.CreatedAt, saleModel.Notes);

        var existingItems = saleModel.Items.Select(i => SaleItem.Create(
            i.Id, saleModel.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList();
        sale.RehydrateItems(existingItems);

        try
        {
            sale.RemoveItem(request.SaleItemId);

            await _writeStore.UpdateAsync(sale, cancellationToken);
            sale.ClearDomainEvents();
            return Result.Success();
        }
        catch (BusinessRuleValidationException ex)
        {
            return Result.Failure(ex.Message, ex.RuleName);
        }
    }
}
