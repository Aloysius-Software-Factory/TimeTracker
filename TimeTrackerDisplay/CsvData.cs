using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TimeTrackerDisplay
{
    public class AppLogRecord
    {
        public DateTime LogTimestamp { get; set; }
        public string ApplicationName { get; set; }
        public DateTime StartTimestamp { get; set; }
        public DateTime EndTimestamp { get; set; }
        public int DurationInSeconds { get; set; }
    }

    public class IdleLogRecord
    {
        public string Status { get; set; }
        public DateTime StartTimestamp { get; set; }
        public DateTime EndTimestamp { get; set; }
        public double DurationMinutes { get; set; }
    }

    public class DayData
    {
        public DateTime Date { get; set; }
        public List<AppLogRecord> AppRecords { get; set; }
        public List<IdleLogRecord> IdleRecords { get; set; }
    }

    public static class CsvParser
    {
        private static readonly Regex FileNameRegex = new Regex(@"^(\d{4}-\d{1,2}-\d{1,2})-(app|idle)\.csv$", RegexOptions.IgnoreCase);

        public static DateTime? ParseDateFromFileName(string fileName)
        {
            var match = FileNameRegex.Match(Path.GetFileName(fileName));
            if (!match.Success) return null;
            return DateTime.ParseExact(match.Groups[1].Value, "yyyy-M-d", CultureInfo.InvariantCulture);
        }

        public static string GetFileType(string fileName)
        {
            var match = FileNameRegex.Match(Path.GetFileName(fileName));
            return match.Success ? match.Groups[2].Value.ToLower() : null;
        }

        public static List<AppLogRecord> ParseAppLog(string filePath)
        {
            var records = new List<AppLogRecord>();
            var lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var parts = SplitCsvLine(lines[i]);
                if (parts.Length < 5) continue;

                try
                {
                    records.Add(new AppLogRecord
                    {
                        LogTimestamp = DateTime.Parse(parts[0]),
                        ApplicationName = parts[1],
                        StartTimestamp = DateTime.Parse(parts[2]),
                        EndTimestamp = DateTime.Parse(parts[3]),
                        DurationInSeconds = int.Parse(parts[4])
                    });
                }
                catch { }
            }
            return records;
        }

        public static List<IdleLogRecord> ParseIdleLog(string filePath)
        {
            var records = new List<IdleLogRecord>();
            var lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var parts = SplitCsvLine(lines[i]);
                if (parts.Length < 4) continue;

                try
                {
                    string minutesStr = parts[3];
                    if (parts.Length >= 5)
                        minutesStr = parts[3] + "." + parts[4];

                    double minutes = double.Parse(minutesStr, CultureInfo.InvariantCulture);

                    records.Add(new IdleLogRecord
                    {
                        Status = parts[0],
                        StartTimestamp = DateTime.Parse(parts[1]),
                        EndTimestamp = DateTime.Parse(parts[2]),
                        DurationMinutes = minutes
                    });
                }
                catch { }
            }
            return records;
        }

        private static string[] SplitCsvLine(string line)
        {
            return line.Split(',');
        }

        public static string[] DiscoverPairedFiles(string[] selectedFiles)
        {
            var fileSet = new HashSet<string>(selectedFiles, StringComparer.OrdinalIgnoreCase);
            var result = new HashSet<string>(selectedFiles, StringComparer.OrdinalIgnoreCase);

            foreach (var file in selectedFiles)
            {
                var date = ParseDateFromFileName(file);
                if (date == null) continue;

                var type = GetFileType(file);
                var dir = Path.GetDirectoryName(file);
                string paired;
                if (type == "app")
                    paired = Path.Combine(dir, date.Value.ToString("yyyy-M-d") + "-idle.csv");
                else
                    paired = Path.Combine(dir, date.Value.ToString("yyyy-M-d") + "-app.csv");

                if (File.Exists(paired) && !fileSet.Contains(paired))
                    result.Add(paired);
            }

            return result.ToArray();
        }
    }
}
