using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.GetSupplierStatistics;

public sealed record GetSupplierStatisticsQuery(Guid SupplierId) : IRequest<Result<SupplierStatisticsReadModel>>;
