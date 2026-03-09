namespace PerformansSitesi.Domain.Entities;

public class AuditLog
{
    public int AuditLogId { get; set; }

    public string EventType { get; set; } = "";   // Export, Import, Login, Update...
    public int? UserId { get; set; }              // nullable
    public string? UserName { get; set; }
    public string? UserRole { get; set; }

    public string? Method { get; set; }           // GET/POST
    public string? Path { get; set; }
    public string? QueryString { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
