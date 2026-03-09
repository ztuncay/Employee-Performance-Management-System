using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerformansSitesi.Application.Services;
using System.Security.Claims;

namespace PerformansSitesi.Controllers;

[Authorize]
public class KilavuzController : Controller
{
    private readonly KilavuzService _kilavuzService;

    public KilavuzController(KilavuzService kilavuzService)
    {
        _kilavuzService = kilavuzService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var kilavuzHtml = _kilavuzService.GetKilavuzHtmlByRole(userRole);
        
        ViewBag.UserRole = userRole;
        ViewBag.KilavuzContent = kilavuzHtml;
        
        return View();
    }
}
