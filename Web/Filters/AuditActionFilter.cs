using Microsoft.AspNetCore.Mvc.Filters;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Infrastructure.Data;
using System.Security.Claims;

namespace PerformansSitesi.Web.Filters;

public class AuditActionFilter : IAsyncActionFilter
{
    private readonly PerformansDbContext _db;
    
    // GÜVENLİK: Hassas endpoint'ler loglanmaz (QueryString veri sızıntısı önleme)
    private static readonly HashSet<string> SensitiveEndpoints = new(StringComparer.OrdinalIgnoreCase)
    {
        "/account/login",
        "/account/changepassword",
        "/kullanici/changepassword",
        "/account/resetpassword",
        "/kvkk/deletemydata"
    };

    public AuditActionFilter(PerformansDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        try
        {
            var http = context.HttpContext;
            var pathRaw = http.Request.Path.ToString();
            var path = string.IsNullOrWhiteSpace(pathRaw) ? "/" : pathRaw;
            var method = string.IsNullOrWhiteSpace(http.Request.Method) ? "UNKNOWN" : http.Request.Method.ToUpperInvariant();

            // Statik dosyaları atla
            if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/lib") || path.StartsWith("/favicon"))
                return;
            
            // GÜVENLİK: Hassas endpoint'leri loglamayı atla
            if (SensitiveEndpoints.Contains(path))
                return;

            var user = http.User;

            int? userId = null;
            var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idStr, out var idVal))
                userId = idVal;
            else if (user?.Identity?.IsAuthenticated != true)
                userId = 0; // zorunlu alan ihtiyacı için anonim 0

            var role = user.FindFirstValue(ClaimTypes.Role) ?? (user?.Identity?.IsAuthenticated == true ? null : "Anonymous");
            var name = user.Identity?.IsAuthenticated == true ? user.Identity?.Name : "Anonymous";

            var log = new AuditLog
            {
                EventType = executed.Exception != null ? "Error" : "Action",
                UserId = userId,
                UserName = name,
                UserRole = role,
                Method = method,
                Path = path,
                // GÜVENLİK: QueryString sadece GET isteklerinde loglanır; zorunlu alan için boş string
                QueryString = method == "GET" && http.Request.QueryString.HasValue
                    ? http.Request.QueryString.Value
                    : string.Empty,
                IpAddress = string.IsNullOrWhiteSpace(http.Connection.RemoteIpAddress?.ToString())
                    ? "Unknown"
                    : http.Connection.RemoteIpAddress!.ToString(),
                UserAgent = string.IsNullOrWhiteSpace(http.Request.Headers.UserAgent)
                    ? "Unknown"
                    : http.Request.Headers.UserAgent.ToString(),
                Note = executed.Exception != null
                    ? $"EXCEPTION: {executed.Exception.GetType().Name}"
                    : null,
                CreatedAt = DateTime.UtcNow
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Audit log hatası uygulamayı durdurmamalı
        }
    }
}
