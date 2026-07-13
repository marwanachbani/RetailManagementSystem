namespace RMS.Modules.Notifications.Domain;

public enum NotificationSeverity
{
    Information = 0,
    Success = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

public enum NotificationCategory
{
    Inventory = 0,
    Sales = 1,
    Purchasing = 2,
    Customers = 3,
    Suppliers = 4,
    Reports = 5,
    Backup = 6,
    Audit = 7,
    Settings = 8,
    System = 9
}

public sealed class Notification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationCategory Category { get; set; }
    public NotificationSeverity Severity { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ReadOn { get; set; }
    public bool IsRead { get; set; }
    public Guid? UserId { get; set; }
    public string RelatedModule { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }

    public Notification() { }

    public Notification(
        Guid id,
        string title,
        string message,
        NotificationCategory category,
        NotificationSeverity severity,
        string relatedModule,
        Guid? relatedEntityId = null,
        Guid? userId = null)
    {
        Id = id;
        Title = title;
        Message = message;
        Category = category;
        Severity = severity;
        CreatedOn = DateTime.UtcNow;
        IsRead = false;
        RelatedModule = relatedModule;
        RelatedEntityId = relatedEntityId;
        UserId = userId;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadOn = DateTime.UtcNow;
        }
    }
}
