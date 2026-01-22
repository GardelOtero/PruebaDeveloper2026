using PruebaDeveloper2026.Domain.Models;

namespace PruebaDeveloper2026.Infrastructure.Data;

public interface IEnsuFileCatalog
{
    IReadOnlyList<EnsuPeriod> GetPeriods();
    string GetCsvPath(string periodKey, EnsuDataset dataset);
}
