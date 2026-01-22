using PruebaDeveloper2026.Domain.Models;
namespace PruebaDeveloper2026.Infrastructure.Repositories;
public interface IEnsuRepository
{
    Task<IReadOnlyList<EnsuRow>> GetAsync(string periodKey, EnsuDataset dataset, CancellationToken ct = default);
    IReadOnlyList<string> GetAvailablePeriods();
}
