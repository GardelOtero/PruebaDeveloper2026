using PruebaDeveloper2026.Application.ViewModels;

namespace PruebaDeveloper2026.Application.Services;

public interface IDashboardService
{
    Task<DashboardVm> GetAsync(string estado, string? periodKey = null, CancellationToken ct = default);
}
