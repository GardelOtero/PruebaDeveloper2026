namespace PruebaDeveloper2026.Infrastructure.Config;

public sealed class EnsuDataOptions
{
    // "Cb", "Cs", "Viv"
    public string Dataset { get; set; } = "Cb";
}

public static class EnsuDataOptionsExtensions
{
    public static PruebaDeveloper2026.Domain.Models.EnsuDataset ToEnum(this EnsuDataOptions opt)
        => (opt.Dataset ?? "Cb").Trim().ToUpperInvariant() switch
        {
            "CB" => PruebaDeveloper2026.Domain.Models.EnsuDataset.Cb,
            "CS" => PruebaDeveloper2026.Domain.Models.EnsuDataset.Cs,
            "VIV" => PruebaDeveloper2026.Domain.Models.EnsuDataset.Viv,
            _ => PruebaDeveloper2026.Domain.Models.EnsuDataset.Cb
        };
}
