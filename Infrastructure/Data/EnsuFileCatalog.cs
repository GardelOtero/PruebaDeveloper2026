using Microsoft.AspNetCore.Hosting;
using PruebaDeveloper2026.Domain.Models;

namespace PruebaDeveloper2026.Infrastructure.Data;

public sealed class EnsuFileCatalog : IEnsuFileCatalog
{
    private readonly IWebHostEnvironment _env;

    private static readonly IReadOnlyList<EnsuPeriod> _periods = new List<EnsuPeriod>
    {
        new("Trimestre1", "0324", "Marzo 2024"),
        new("Trimestre2", "0624", "Junio 2024"),
        new("Trimestre3", "0924", "Septiembre 2024"),
        new("Trimestre4", "1224", "Diciembre 2024"),
    };

    public EnsuFileCatalog(IWebHostEnvironment env)
    {
        _env = env;
    }

    public IReadOnlyList<EnsuPeriod> GetPeriods() => _periods;

    public string GetCsvPath(string periodKey, EnsuDataset dataset)
    {
        var p = _periods.FirstOrDefault(x => x.Key.Equals(periodKey, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Periodo inválido: {periodKey}");

        var folder = Path.Combine(_env.ContentRootPath, "AppData", p.Key);

        var fileName = dataset switch
        {
            EnsuDataset.Cb => $"conjunto_de_datos_ensu_cb_{p.Code}.csv",
            EnsuDataset.Cs => $"conjunto_de_datos_ensu_cs_{p.Code}.csv",
            EnsuDataset.Viv => $"conjunto_de_datos_ensu_viv_{p.Code}.csv",
            EnsuDataset.Indice => $"0_indice_tablas_ensu{p.Code}.csv",
            _ => throw new ArgumentOutOfRangeException(nameof(dataset))
        };

        var full = Path.Combine(folder, fileName);

        if (!File.Exists(full))
            throw new FileNotFoundException($"No encontré el CSV {dataset} del periodo {p.Key}. Esperado: {full}");

        return full;
    }
}
