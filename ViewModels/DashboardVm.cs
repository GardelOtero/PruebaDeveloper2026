namespace PruebaDeveloper2026.Application.ViewModels;

public sealed record DashboardVm(
    string Estado,
    string SelectedPeriod,                 // "Trimestre1..4" o "all"
    IReadOnlyList<string> Periods,         // para el dropdown
    string BasePeriod,                     // periodo base real (cuando SelectedPeriod="all" usa el último)
    decimal KpiInseguridad,
    string KpiMunicipioMasInseguro,
    string KpiMunicipioMasSeguro,
    decimal KpiBrechaGenero,
    List<TimePointVm> InseguridadTiempo,
    List<RankedItemVm> MunicipiosMasInseguros,
    List<RankedItemVm> MunicipiosMasSeguros,
    List<LugarItemVm> InseguridadPorLugar,
    List<GeneroItemVm> InseguridadPorGenero
);

public sealed record TimePointVm(string Periodo, decimal PorcentajeInseguro);
public sealed record RankedItemVm(string Nombre, decimal PorcentajeInseguro);
public sealed record LugarItemVm(string Lugar, decimal PorcentajeInseguro);
public sealed record GeneroItemVm(string Genero, decimal PorcentajeInseguro);
