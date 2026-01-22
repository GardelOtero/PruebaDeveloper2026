using Microsoft.Extensions.Logging;
using PruebaDeveloper2026.Domain.Models;
using PruebaDeveloper2026.Infrastructure.Csv;
using PruebaDeveloper2026.Infrastructure.Data;

namespace PruebaDeveloper2026.Infrastructure.Repositories;

public sealed class EnsuRepository : IEnsuRepository
{
    private readonly IEnsuFileCatalog _catalog;
    private readonly IEnsuCsvReader _reader;
    private readonly ILogger<EnsuRepository> _logger;

    private readonly Dictionary<(string periodKey, EnsuDataset dataset), IReadOnlyList<EnsuRow>> _cache = new();

    public EnsuRepository(IEnsuFileCatalog catalog, IEnsuCsvReader reader, ILogger<EnsuRepository> logger)
    {
        _catalog = catalog;
        _reader = reader;
        _logger = logger;
    }

    public IReadOnlyList<string> GetAvailablePeriods()
        => _catalog.GetPeriods().Select(p => p.Key).OrderBy(x => x).ToList();

    public async Task<IReadOnlyList<EnsuRow>> GetAsync(string periodKey, EnsuDataset dataset, CancellationToken ct = default)
    {
        var key = (periodKey, dataset);

        if (_cache.TryGetValue(key, out var hit))
            return hit;

        var path = _catalog.GetCsvPath(periodKey, dataset);
        var rows = await _reader.ReadAsync(path, ct);

        _cache[key] = rows;

        _logger.LogInformation("ENSU cargado: {Period} {Dataset} filas={Rows}", periodKey, dataset, rows.Count);

        return rows;
    }
}
