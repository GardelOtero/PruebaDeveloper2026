using Microsoft.AspNetCore.Mvc;
using PruebaDeveloper2026.Domain.Models;
using PruebaDeveloper2026.Infrastructure.Repositories;

namespace PruebaDeveloper2026.Controllers;

public class DebugController : Controller
{
    private readonly IEnsuRepository _repo;

    public DebugController(IEnsuRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("/debug/cb-headers")]
    public async Task<IActionResult> CbHeaders(string period = "Trimestre4")
    {
        var rows = await _repo.GetAsync(period, EnsuDataset.Cb);
        var headers = rows.FirstOrDefault()?.Fields.Keys.ToList() ?? new List<string>();
        return Json(headers);
    }
}
