using System.Windows;
using RMS.Modules.Audit.Application.Contracts;

namespace RMS.WPF.ViewModels;

public sealed class AuditLogDetailsViewModel
{
    public AuditLogReadModel Entry { get; }

    public AuditLogDetailsViewModel(AuditLogReadModel entry)
    {
        Entry = entry;
    }
}
