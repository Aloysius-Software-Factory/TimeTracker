using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTrackerDisplay
{
    public class FilePair
    {
        public string AppFile;
        public string IdleFile;
    }

    public class AppUsageSummary
    {
        public string Name { get; set; }
        public double TotalSeconds { get; set; }
    }

    public class StatusUsageSummary
    {
        public string Status { get; set; }
        public double TotalSeconds { get; set; }
    }

    public class MainFormDataService
    {
        private List<DayData> _allDays = new List<DayData>();
        public IList<DayData> AllDays => _allDays;

        public void LoadFiles(string[] selectedFiles)
        {
            var allFiles = CsvParser.DiscoverPairedFiles(selectedFiles);

            var fileGroups = new SortedDictionary<DateTime, FilePair>();
            foreach (var file in allFiles)
            {
                var date = CsvParser.ParseDateFromFileName(file);
                if (date == null) continue;
                var type = CsvParser.GetFileType(file);
                if (!fileGroups.ContainsKey(date.Value))
                    fileGroups[date.Value] = new FilePair();
                var g = fileGroups[date.Value];
                if (type == "app")
                    g.AppFile = file;
                else
                    g.IdleFile = file;
            }

            _allDays.Clear();
            foreach (var kvp in fileGroups)
            {
                _allDays.Add(new DayData
                {
                    Date = kvp.Key,
                    AppRecords = kvp.Value.AppFile != null ? CsvParser.ParseAppLog(kvp.Value.AppFile) : new List<AppLogRecord>(),
                    IdleRecords = kvp.Value.IdleFile != null ? CsvParser.ParseIdleLog(kvp.Value.IdleFile) : new List<IdleLogRecord>()
                });
            }
        }

        public IEnumerable<AppUsageSummary> GetAppUsageSummaries(DayData day, bool hasTimeFilter, TimeSpan timeFrom, TimeSpan timeTo)
        {
            var appRecords = day.AppRecords;
            if (hasTimeFilter)
            {
                appRecords = appRecords
                    .Where(r => r.StartTimestamp.TimeOfDay < timeTo && r.EndTimestamp.TimeOfDay > timeFrom)
                    .ToList();
            }

            return appRecords
                .GroupBy(r => r.ApplicationName)
                .Select(g => new AppUsageSummary { Name = g.Key, TotalSeconds = g.Sum(r => r.DurationInSeconds) })
                .Where(x => x.TotalSeconds > 0)
                .OrderByDescending(x => x.TotalSeconds);
        }

        public IEnumerable<StatusUsageSummary> GetStatusUsageSummaries(DayData day, bool hasTimeFilter, TimeSpan timeFrom, TimeSpan timeTo)
        {
            var idleRecords = day.IdleRecords;
            if (hasTimeFilter)
            {
                idleRecords = idleRecords
                    .Where(r => r.StartTimestamp.TimeOfDay < timeTo && r.EndTimestamp.TimeOfDay > timeFrom)
                    .ToList();
            }

            return idleRecords
                .GroupBy(r => r.Status.ToLower())
                .Select(g => new StatusUsageSummary { Status = g.Key, TotalSeconds = g.Sum(r => r.DurationMinutes * 60) })
                .Where(x => x.TotalSeconds > 0)
                .OrderByDescending(x => x.TotalSeconds);
        }
    }
}
