using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PerformansSitesi.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("/Error/Handle")]
    public IActionResult Handle()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionFeature?.Error;

        if (exception != null)
        {
            _logger.LogError(exception,
                "GLOBAL ERROR HANDLER - Path: {Path}, User: {User}, IP: {IP}",
                exceptionFeature?.Path,
                User?.Identity?.Name ?? "Anonymous",
                HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        ViewBag.ErrorMessage = exception?.Message ?? "Bilinmeyen bir hata oluştu";
        ViewBag.ErrorDetails = exception?.ToString();
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        return View("Error");
    }

    [Route("/Error/StatusCode/{code}")]
    public IActionResult StatusCode(int code)
    {
        _logger.LogWarning("HTTP {StatusCode} error - Path: {Path}",
            code, HttpContext.Request.Path);

        ViewBag.StatusCode = code;
        return View();
    }
}