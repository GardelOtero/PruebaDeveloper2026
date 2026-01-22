using PruebaDeveloper2026.Domain.Models;

namespace PruebaDeveloper2026.Infrastructure.Csv;

public interface IEnsuCsvReader
{
    Task<IReadOnlyList<EnsuRow>> ReadAsync(string path, CancellationToken ct = default);
    IReadOnlyList<string> GetAvailablePeriods();
}
