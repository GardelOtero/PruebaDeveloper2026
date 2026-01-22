namespace PruebaDeveloper2026.Domain.Models;

public sealed class EnsuRow
{
    public Dictionary<string, string?> Fields { get; }

    public EnsuRow(Dictionary<string, string?> fields)
    {
        Fields = fields;
    }

    public string? Get(string col)
        => Fields.TryGetValue(col, out var v) ? v : null;

    public string GetOrEmpty(string col)
        => Get(col) ?? "";
}
