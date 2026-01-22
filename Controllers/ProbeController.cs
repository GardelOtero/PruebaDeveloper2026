using Microsoft.AspNetCore.Mvc;
using PruebaDeveloper2026.Domain.Models;
using PruebaDeveloper2026.Infrastructure.Repositories;

namespace PruebaDeveloper2026.Controllers;

public class ProbeController : Controller
{
    private readonly IEnsuRepository _repo;

    public ProbeController(IEnsuRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("/probe")]
    public async Task<IActionResult> Index(string period = "Trimestre4", string dataset = "Cb")
    {
        if (!Enum.TryParse<EnsuDataset>(dataset, true, out var ds))
            ds = EnsuDataset.Cb;

        var rows = await _repo.GetAsync(period, ds);

        var headers = rows.FirstOrDefault()?.Fields.Keys.Take(80).ToList() ?? new List<string>();

        return Json(new
        {
            period,
            dataset = ds.ToString(),
            rows = rows.Count,
            headers
        });
    }
}
