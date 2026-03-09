using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using PerformansSitesi.Application.Services;

namespace PerformansSitesi.Web.ViewComponents;

public class DynamicThemeViewComponent : ViewComponent
{
    private readonly ThemeService _themeService;

    public DynamicThemeViewComponent(ThemeService themeService)
    {
        _themeService = themeService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var aktifTema = await _themeService.GetAktifTemaAsync();
        
        if (aktifTema == null)
        {
            ViewBag.ThemeCss = string.Empty;
        }
        else
        {
            ViewBag.ThemeCss = _themeService.GenerateCssVariables(aktifTema);
        }
        
        return View();
    }
}
