namespace PruebaDeveloper2026.Infrastructure.Config;

public sealed class EnsuColumnOptions
{
    // Puedes dejar null para que se auto-detecte
    public string? EstadoNombreCol { get; set; }      // ej: "NOM_ENT"
    public string? MunicipioNombreCol { get; set; }   // ej: "NOM_MUN"
    public string? SexoCol { get; set; }              // ej: "SEXO"
    public List<string>? LugarColumns { get; set; }


    // Percepción global (ciudad insegura/segura): ej "P1_1"
    public string? PercepcionInseguridadCol { get; set; }

    // Columnas por tipo de lugar: ej P2_1..P2_? (depende dataset)
    // Si no lo configuras, se auto-detectan por prefijo P2_
    public string? LugarPrefix { get; set; } = "P2_";
}
