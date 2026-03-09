using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PerformansSitesi.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
