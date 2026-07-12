using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;

namespace RMS.Modules.Reporting.Application.IntegrationEvents;

public sealed record ReportGeneratedIntegrationEvent(string ReportType, string Format) : DomainEvent, IIntegrationEvent;
public sealed record ReportExportedIntegrationEvent(string ReportType, string Format) : DomainEvent, IIntegrationEvent;
public sealed record ReportPrintedIntegrationEvent(string ReportType) : DomainEvent, IIntegrationEvent;
