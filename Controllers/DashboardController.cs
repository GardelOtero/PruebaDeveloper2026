using Microsoft.AspNetCore.Mvc;
using PruebaDeveloper2026.Application.Services;

namespace PruebaDeveloper2026.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("/")]
    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index(string estado = "NUEVO LEÓN", string period = "all")
    {
        string? periodKey = period.Equals("all", StringComparison.OrdinalIgnoreCase) ? null : period;

        var vm = await _service.GetAsync(estado, periodKey);
        return View(vm); // vm.Periods ya viene lleno
    }

    [HttpGet("/dashboard/data")]
    public async Task<IActionResult> Data(string estado = "NUEVO LEÓN", string period = "all")
    {
        string? periodKey = period.Equals("all", StringComparison.OrdinalIgnoreCase) ? null : period;

        var vm = await _service.GetAsync(estado, periodKey);
        return Json(vm);
    }
}
