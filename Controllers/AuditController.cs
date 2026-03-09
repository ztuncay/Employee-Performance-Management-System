using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.ViewModels;

namespace PerformansSitesi.Controllers;

[Authorize(Roles = "SistemAdmin")]
public class AuditController : Controller
{
    private readonly PerformansDbContext _db;

    public AuditController(PerformansDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? arama, string? eventType, string? role, int page = 1)
    {
        if (page < 1) page = 1;

        var q = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(eventType))
            q = q.Where(x => x.EventType == eventType);

        if (!string.IsNullOrWhiteSpace(role))
            q = q.Where(x => x.UserRole == role);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();
            q = q.Where(x =>
                (x.UserName ?? "").Contains(arama) ||
                (x.Path ?? "").Contains(arama) ||
                (x.Note ?? "").Contains(arama) ||
                (x.IpAddress ?? "").Contains(arama));
        }

        const int pageSize = 50;
        var total = await q.CountAsync();

        var rows = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditListViewModel.Row
            {
                AuditLogId = x.AuditLogId,
                CreatedAt = x.CreatedAt,
                EventType = x.EventType,
                UserId = x.UserId,
                UserName = x.UserName,
                UserRole = x.UserRole,
                Method = x.Method,
                Path = x.Path,
                IpAddress = x.IpAddress,
                Note = x.Note
            })
            .ToListAsync();

        var vm = new AuditListViewModel
        {
            Arama = arama,
            EventType = eventType,
            Role = role,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Rows = rows
        };

        return View(vm);
    }
}
