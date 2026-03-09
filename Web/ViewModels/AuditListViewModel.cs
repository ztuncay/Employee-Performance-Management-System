namespace PerformansSitesi.Web.ViewModels;

public class AuditListViewModel
{
    public string? Arama { get; set; }
    public string? EventType { get; set; }
    public string? Role { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int Total { get; set; }

    public List<Row> Rows { get; set; } = new();

    public class Row
    {
        public long AuditLogId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string EventType { get; set; } = "";
        public string? UserName { get; set; }
        public int? UserId { get; set; }
        public string? UserRole { get; set; }

        public string Method { get; set; } = "";
        public string Path { get; set; } = "";

        public string? IpAddress { get; set; }
        public string? Note { get; set; }
    }
}
