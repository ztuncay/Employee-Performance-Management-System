using PerformansSitesi.Domain.Entities;

namespace PerformansSitesi.Web.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int TotalEvaluations { get; set; }
    public int TotalAuditLogs { get; set; }
    public int ActivePeriods { get; set; }
    public List<AuditLog> RecentLogs { get; set; } = new();
    public Dictionary<string, int> UsersByRole { get; set; } = new();
}

public class AdminLogsViewModel
{
    public List<AuditLog> Logs { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalLogs { get; set; }
}

public class AdminDatabaseViewModel
{
    public int Users { get; set; }
    public int Personnel { get; set; }
    public int Evaluations { get; set; }
    public int AuditLogs { get; set; }
    public int Periods { get; set; }
    public int Questions { get; set; }
    public int EvaluationDetails { get; set; }
}

public class AdminConfigViewModel
{
    public string DatabaseServer { get; set; }
    public string Environment { get; set; }
    public string AspNetCoreVersion { get; set; }
    public DateTime SystemTime { get; set; }
}

public class AdminHealthCheckViewModel
{
    public string DatabaseStatus { get; set; }
    public string ApplicationVersion { get; set; }
    public DateTime ServerTime { get; set; }
    public bool IsProduction { get; set; }
    public int ActiveUserSessions { get; set; }
}

public class AdminPerformanceViewModel
{
    public int TotalUsers { get; set; }
    public int TotalEvaluations { get; set; }
    public double AverageDegerlendirmeFillPercentage { get; set; }
    public List<dynamic> MostActiveUsers { get; set; } = new();
}
