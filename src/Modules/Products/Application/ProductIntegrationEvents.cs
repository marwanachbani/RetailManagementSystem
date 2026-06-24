namespace RMS.Modules.Products.Application;

public sealed record ProductCreatedIntegrationEvent(Guid ProductId, string ProductCode, string Name);
public sealed record ProductUpdatedIntegrationEvent(Guid ProductId, string ProductCode, string Name);
public sealed record ProductDeactivatedIntegrationEvent(Guid ProductId, string ProductCode);
