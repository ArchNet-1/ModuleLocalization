// ArchNet · Localization Module — CSV
// Language tables to and from CSV, for translators and spreadsheets: a "key" column, then one
// column per language code. Quotes, commas and line breaks inside texts are kept (RFC 4180).
// Docs: https://archnet-plateform.fr/#ModuleLocalization

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArchNet.Module
{
    public static class LocalizationCsv
    {
        /// <summary>languages[code][key] = text → CSV text.</summary>
        public static string Write(IReadOnlyDictionary<string, Dictionary<string, string>> languages)
        {
            string[] codes = languages.Keys.OrderBy(c => c).ToArray();
            IEnumerable<string> keys = languages.Values.SelectMany(texts => texts.Keys).Distinct().OrderBy(k => k);
            var csv = new StringBuilder();
            csv.AppendLine(string.Join(",", new[] { "key" }.Concat(codes).Select(Quote)));
            foreach (string key in keys)
                csv.AppendLine(string.Join(",", new[] { key }.Concat(codes.Select(c => languages[c].TryGetValue(key, out string text) ? text : "")).Select(Quote)));
            return csv.ToString();
        }

        /// <summary>CSV text → languages[code][key] = text. The first row names the columns.</summary>
        public static Dictionary<string, Dictionary<string, string>> Read(string csv)
        {
            List<List<string>> rows = Parse(csv);
            if (rows.Count == 0 || rows[0].Count < 2 || rows[0][0].Trim() != "key")
                throw new FormatException("The first row must be: key, then one column per language code (en, fr…).");
            List<string> codes = rows[0].Skip(1).Select(c => c.Trim()).ToList();
            var languages = codes.ToDictionary(code => code, _ => new Dictionary<string, string>());
            foreach (List<string> row in rows.Skip(1))
            {
                if (row.Count == 0 || string.IsNullOrWhiteSpace(row[0])) continue;
                for (int i = 0; i < codes.Count; i++)
                    languages[codes[i]][row[0].Trim()] = i + 1 < row.Count ? row[i + 1] : "";
            }
            return languages;
        }

        static string Quote(string value) =>
            value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;

        static List<List<string>> Parse(string csv)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < csv.Length; i++)
            {
                char c = csv[i];
                if (quoted)
                {
                    if (c == '"' && i + 1 < csv.Length && csv[i + 1] == '"') { cell.Append('"'); i++; }
                    else if (c == '"') quoted = false;
                    else cell.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { row.Add(cell.ToString()); cell.Clear(); }
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                    row.Add(cell.ToString());
                    cell.Clear();
                    rows.Add(row);
                    row = new List<string>();
                }
                else cell.Append(c);
            }
            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row);
            }
            return rows;
        }
    }
}
