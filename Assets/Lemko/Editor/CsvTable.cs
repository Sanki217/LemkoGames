using System;
using System.Collections.Generic;
using System.Text;
namespace Lemko.Editor
{
    public static class CsvTable
    {
        public static List<string[]> Parse(string input)
        {
            input = input.TrimStart('\uFEFF');
            // Excel may use the locale list separator or a sep= directive.
            char separator = ',';
            if (input.StartsWith("sep=", StringComparison.OrdinalIgnoreCase))
            { if (input.Length < 5) throw new FormatException("Missing Excel separator."); separator = input[4]; int end = input.IndexOf('\n'); input = end < 0 ? "" : input.Substring(end + 1); }
            else { var header = input.Split('\n')[0]; if (!header.Contains(",") && header.Contains(";")) separator = ';'; }
            var rows = new List<string[]>(); var row = new List<string>(); var value = new StringBuilder();
            bool quoted = false, closed = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (quoted)
                {
                    if (c == '"') { if (i + 1 < input.Length && input[i + 1] == '"') { value.Append('"'); i++; } else { quoted = false; closed = true; } }
                    else value.Append(c);
                }
                else if (c == separator) { row.Add(value.ToString()); value.Clear(); closed = false; }
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < input.Length && input[i + 1] == '\n') i++;
                    row.Add(value.ToString()); if (row.Count > 1 || row[0].Length > 0) rows.Add(row.ToArray());
                    row.Clear(); value.Clear(); closed = false;
                }
                else if (c == '"' && value.Length == 0 && !closed) quoted = true;
                else { if (closed || c == '"') throw new FormatException("Unexpected character after quoted field."); value.Append(c); }
            }
            if (quoted) throw new FormatException("Unclosed CSV quote.");
            if (row.Count > 0 || value.Length > 0 || closed) { row.Add(value.ToString()); rows.Add(row.ToArray()); }
            return rows;
        }
        public static List<Dictionary<string,string>> Records(string text, params string[] required)
        {
            var rows = Parse(text); if (rows.Count == 0) throw new FormatException("Empty CSV.");
            var headers = new HashSet<string>(rows[0]);
            if (headers.Count != rows[0].Length) throw new FormatException("Duplicate column heading.");
            foreach (var h in required) if (!headers.Contains(h)) throw new FormatException("Missing column: " + h);
            var records = new List<Dictionary<string,string>>();
            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i].Length != rows[0].Length) throw new FormatException("Column count mismatch at row " + (i + 1));
                var record = new Dictionary<string,string>();
                for (int j = 0; j < rows[0].Length; j++) record.Add(rows[0][j], rows[i][j]);
                records.Add(record);
            }
            return records;
        }
    }
}
