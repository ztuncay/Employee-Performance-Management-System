using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerformansSitesi.Application.Services;
using PerformansSitesi.Domain.Entities;

namespace PerformansSitesi.Controllers;

// Tema yönetimi kapatıldı - Sadece direkt URL ile erişim için bırakıldı
[Authorize(Roles = "SistemAdmin")]
[Route("admin/theme-disabled")]
public class ThemeController : Controller
{
    private readonly ThemeService _themeService;

    public ThemeController(ThemeService themeService)
    {
        _themeService = themeService;
    }

    public IActionResult Index()
    {
        // Tema yönetimi geçici olarak devre dışı
        TempData["Warning"] = "Tema yönetimi şu anda kullanıma kapatılmıştır.";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Create()
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Save(SiteTema tema)
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Activate(int id)
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Preview(int id)
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult DownloadCss(int id)
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateDefault()
    {
        return RedirectToAction(nameof(Index));
    }
}