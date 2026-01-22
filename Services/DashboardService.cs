using Microsoft.Extensions.Options;
using PruebaDeveloper2026.Application.ViewModels;
using PruebaDeveloper2026.Domain.Models;
using PruebaDeveloper2026.Infrastructure.Config;
using PruebaDeveloper2026.Infrastructure.Options;
using PruebaDeveloper2026.Infrastructure.Repositories;
using System.Globalization;
using System.Text;

namespace PruebaDeveloper2026.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IEnsuRepository _repo;
    private readonly EnsuColumnOptions _opt;
    private readonly EnsuDisplayOptions _displayOpt;
    private readonly EnsuDataOptions _dataOpt;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IEnsuRepository repo,
        IOptions<EnsuColumnOptions> opt,
        IOptions<EnsuDisplayOptions> displayOpt,
        IOptions<EnsuDataOptions> dataOpt,
        ILogger<DashboardService> logger)
    {
        _repo = repo;
        _opt = opt.Value;
        _displayOpt = displayOpt.Value;
        _dataOpt = dataOpt.Value;
        _logger = logger;
    }

    public async Task<DashboardVm> GetAsync(string estado, string? periodKey = null, CancellationToken ct = default)
    {
        // dataset para tus KPIs = Cb
        var dataset = _dataOpt.ToEnum();

        // dropdown
        var availablePeriods = _repo.GetAvailablePeriods(); // Trimestre1..4
        if (availablePeriods.Count == 0)
            throw new InvalidOperationException("No hay periodos disponibles. Revisa AppData/Trimestre*.");

        // si periodKey null => "all"
        var selectedPeriod = periodKey is null ? "all" : periodKey;

        // qué periodos se usan en la serie
        var periodsForSerie = periodKey is null ? availablePeriods : new List<string> { periodKey };

        // 1) Serie temporal
        var serie = new List<TimePointVm>();
        foreach (var p in periodsForSerie)
        {
            var rows = await _repo.GetAsync(p, dataset, ct);
            var cols = ResolveColumns(p, rows);

            var st = FilterEstado(rows, cols.EstadoCol, estado);
            var pct = CalcPctInseguro(st, cols.PercepcionCol);

            serie.Add(new TimePointVm(p, pct));
        }

        // periodo base real (para rankings/lugar/género)
        var basePeriod = periodKey ?? periodsForSerie.Last();

        var baseRows = await _repo.GetAsync(basePeriod, dataset, ct);
        var baseCols = ResolveColumns(basePeriod, baseRows);
        var baseSt = FilterEstado(baseRows, baseCols.EstadoCol, estado);

        // 2) Municipios top/bottom
        var muniRank = RankBy(baseSt, baseCols.MunicipioCol, baseCols.PercepcionCol);
        var masInseguros = muniRank.OrderByDescending(x => x.PorcentajeInseguro).Take(5).ToList();
        var masSeguros = muniRank.OrderBy(x => x.PorcentajeInseguro).Take(5).ToList();

        // 3) Por tipo de lugar
        var lugares = CalcPorLugar(baseSt, baseCols.LugarCols);

        // 4) Género
        var genero = RankBy(baseSt, baseCols.SexoCol, baseCols.PercepcionCol)
            .Select(x => new GeneroItemVm(NormalizaSexo(x.Nombre), x.PorcentajeInseguro))
            .ToList();

        // KPIs (periodo base)
        var kpiInseg = CalcPctInseguro(baseSt, baseCols.PercepcionCol);

        var kpiMuniMasInseguro = masInseguros.FirstOrDefault()?.Nombre ?? "—";
        var kpiMuniMasSeguro = masSeguros.FirstOrDefault()?.Nombre ?? "—";

        var pctMuj = genero.FirstOrDefault(g => g.Genero == "Mujeres")?.PorcentajeInseguro ?? 0m;
        var pctHom = genero.FirstOrDefault(g => g.Genero == "Hombres")?.PorcentajeInseguro ?? 0m;
        var brecha = Math.Round(pctMuj - pctHom, 2);

        return new DashboardVm(
            Estado: estado,
            SelectedPeriod: selectedPeriod,
            Periods: availablePeriods,              // para el dropdown
            BasePeriod: basePeriod,
            KpiInseguridad: kpiInseg,
            KpiMunicipioMasInseguro: kpiMuniMasInseguro,
            KpiMunicipioMasSeguro: kpiMuniMasSeguro,
            KpiBrechaGenero: brecha,
            InseguridadTiempo: serie,
            MunicipiosMasInseguros: masInseguros,
            MunicipiosMasSeguros: masSeguros,
            InseguridadPorLugar: lugares,
            InseguridadPorGenero: genero
        );
    }

    // ----------------- Helpers -----------------

    private static IEnumerable<EnsuRow> FilterEstado(IEnumerable<EnsuRow> rows, string estadoCol, string estado)
    {
        static string norm(string s)
        {
            s = (s ?? "").Trim().ToUpperInvariant();
            var normalized = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        var target = norm(estado);
        return rows.Where(r => norm(r.GetOrEmpty(estadoCol)) == target);
    }

    private static decimal CalcPctInseguro(IEnumerable<EnsuRow> rows, string percepCol)
    {
        // ENSU Cb típico: BP1_1 = 1(seguro), 2(inseguro), 9(NS/NR)
        var vals = rows.Select(r => (r.Get(percepCol) ?? "").Trim())
                       .Where(v => v.Length > 0)
                       .ToList();

        if (vals.Count == 0) return 0m;

        // ignorar NS/NR y códigos raros
        var valid = vals.Where(v => v == "1" || v == "2").ToList();
        if (valid.Count == 0) return 0m;

        var inseg = valid.Count(v => v == "2");
        return Math.Round((decimal)inseg * 100m / valid.Count, 2);
    }

    private static List<RankedItemVm> RankBy(IEnumerable<EnsuRow> rows, string groupCol, string percepCol)
    {
        var groups = rows
            .GroupBy(r => (r.Get(groupCol) ?? "").Trim())
            .Where(g => !string.IsNullOrWhiteSpace(g.Key));

        var result = new List<RankedItemVm>();
        foreach (var g in groups)
        {
            var pct = CalcPctInseguro(g, percepCol);
            result.Add(new RankedItemVm(g.Key, pct));
        }
        return result;
    }

    private List<LugarItemVm> CalcPorLugar(IEnumerable<EnsuRow> rows, IReadOnlyList<string> lugarCols)
    {
        var list = new List<LugarItemVm>();

        foreach (var col in lugarCols)
        {
            var vals = rows.Select(r => (r.Get(col) ?? "").Trim())
                           .Where(v => v == "1" || v == "2")
                           .ToList();

            if (vals.Count == 0)
            {
                list.Add(new LugarItemVm(GetLugarLabel(col), 0m));
                continue;
            }

            var inseg = vals.Count(v => v == "2");
            var pct = Math.Round((decimal)inseg * 100m / vals.Count, 2);

            list.Add(new LugarItemVm(GetLugarLabel(col), pct));
        }

        return list.OrderByDescending(x => x.PorcentajeInseguro).ToList();
    }

    private string GetLugarLabel(string col)
        => _displayOpt?.LugarLabels != null && _displayOpt.LugarLabels.TryGetValue(col, out var label)
            ? label
            : col;

    private static string NormalizaSexo(string raw)
    {
        var u = (raw ?? "").Trim().ToUpperInvariant();
        if (u == "1" || u.Contains("HOM")) return "Hombres";
        if (u == "2" || u.Contains("MUJ")) return "Mujeres";
        return raw;
    }

    // ----------------- Column resolution (Cb) -----------------

    private (string EstadoCol, string MunicipioCol, string SexoCol, string PercepcionCol, IReadOnlyList<string> LugarCols)
        ResolveColumns(string period, IReadOnlyList<EnsuRow> rows)
    {
        var headers = rows.FirstOrDefault()?.Fields.Keys.ToList() ?? new List<string>();
        if (headers.Count == 0) throw new InvalidOperationException("No hay headers en el CSV.");

        _logger.LogInformation("Headers ({Period}) sample: {Headers}", period, string.Join(", ", headers.Take(120)));

        string Pick(string? configured, params string[] candidates)
        {
            if (!string.IsNullOrWhiteSpace(configured))
            {
                var exact = headers.FirstOrDefault(h => h.Equals(configured, StringComparison.OrdinalIgnoreCase));
                if (exact != null) return exact;
            }

            foreach (var c in candidates)
            {
                var hit = headers.FirstOrDefault(h => h.Equals(c, StringComparison.OrdinalIgnoreCase));
                if (hit != null) return hit;
            }

            foreach (var c in candidates)
            {
                var hit = headers.FirstOrDefault(h => h.Contains(c, StringComparison.OrdinalIgnoreCase));
                if (hit != null) return hit;
            }

            throw new InvalidOperationException($"No pude resolver columna. Candidatos: {string.Join(", ", candidates)}");
        }

        var estadoCol = Pick(_opt.EstadoNombreCol, "NOM_ENT");
        var municipioCol = Pick(_opt.MunicipioNombreCol, "NOM_MUN");
        var sexoCol = Pick(_opt.SexoCol, "SEXO");

        // Cb: percepción
        var percepCol = Pick(_opt.PercepcionInseguridadCol, "BP1_1");

        // Cb: lugares típicos
        var lugarCols = new List<string>();
        if (_opt.LugarColumns is { Count: > 0 })
        {
            foreach (var c in _opt.LugarColumns)
            {
                var hit = headers.FirstOrDefault(h => h.Equals(c, StringComparison.OrdinalIgnoreCase));
                if (hit != null) lugarCols.Add(hit);
            }
        }

        // fallback si config no matchea por alguna razón
        if (lugarCols.Count == 0)
        {
            lugarCols = headers
                .Where(h => h.StartsWith("BP1_2_", StringComparison.OrdinalIgnoreCase))
                .OrderBy(h => h)
                .ToList();
        }

        _logger.LogInformation(
            "Columnas resueltas: Estado={Estado} Mun={Mun} Sexo={Sexo} Percep={Percep} Lugares={LugaresCount}",
            estadoCol, municipioCol, sexoCol, percepCol, lugarCols.Count);

        return (estadoCol, municipioCol, sexoCol, percepCol, lugarCols);
    }
}
