using System.Globalization;
using PruebaDeveloper2026.Domain.Models;

namespace PruebaDeveloper2026.Infrastructure.Csv;

public sealed class EnsuCsvReader : IEnsuCsvReader
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EnsuCsvReader> _logger;

    public EnsuCsvReader(IWebHostEnvironment env, ILogger<EnsuCsvReader> logger)
    {
        _env = env;
        _logger = logger;
    }

    public IReadOnlyList<string> GetAvailablePeriods()
    {
        // Busca: {ContentRoot}/AppData/Trimestre1..n
        var appData = Path.Combine(_env.ContentRootPath, "AppData");
        if (!Directory.Exists(appData))
        {
            _logger.LogWarning("No existe AppData en: {Path}", appData);
            return Array.Empty<string>();
        }

        var periods = Directory.GetDirectories(appData, "Trimestre*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => ExtractNumber(n!))
            .ToList()!;

        _logger.LogInformation("Periodos detectados: {Periods}", string.Join(", ", periods));
        return periods;

        static int ExtractNumber(string name)
        {
            var digits = new string(name.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var n) ? n : 999;
        }
    }

    public async Task<IReadOnlyList<EnsuRow>> ReadAsync(string path, CancellationToken ct = default)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"No existe el CSV: {path}", path);

        using var fs = File.OpenRead(path);
        using var sr = new StreamReader(fs);

        string? headerLine = await sr.ReadLineAsync();
        if (headerLine is null)
            return Array.Empty<EnsuRow>();

        // CSVs ENSU normalmente vienen con comas; si tus archivos vienen con otro separador, cámbialo aquí
        var headers = SplitCsvLine(headerLine);

        var rows = new List<EnsuRow>(capacity: 1024);

        while (!sr.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();

            var line = await sr.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = SplitCsvLine(line);

            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                var key = headers[i];
                var val = i < values.Count ? values[i] : null;
                dict[key] = val;
            }

            rows.Add(new EnsuRow(dict));
        }

        _logger.LogInformation("CSV leído: {File} filas={Count}", Path.GetFileName(path), rows.Count);
        return rows;
    }

    // ✅ CSV splitter “suficiente” para ENSU (maneja comillas)
    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var cur = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // "" dentro de quotes => "
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    cur.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(cur.ToString().Trim());
                cur.Clear();
                continue;
            }

            cur.Append(c);
        }

        result.Add(cur.ToString().Trim());
        return result;
    }
}
