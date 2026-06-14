using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TimeTrackerDisplay
{
    public partial class MainForm : Form
    {
        private List<DayData> _allDays = new List<DayData>();

        public MainForm()
        {
            InitializeComponent();
            dtpTimeFrom.Value = DateTime.Today.AddHours(8);
            dtpTimeTo.Value = DateTime.Today.AddHours(17);
        }

        private void BtnOpenFiles_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.Title = "Select time tracker CSV files";

                if (dialog.ShowDialog() == DialogResult.OK)
                    LoadFiles(dialog.FileNames);
            }
        }

        private class FilePair
        {
            public string AppFile;
            public string IdleFile;
        }

        private void LoadFiles(string[] selectedFiles)
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

            TimelinePanel.ResetAppColors();
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

            ApplyFilter();
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var timeFrom = dtpTimeFrom.Value.TimeOfDay;
            var timeTo = dtpTimeTo.Value.TimeOfDay;
            bool hasTimeFilter = timeFrom != timeTo;

            var filtered = _allDays.OrderBy(d => d.Date).ToList();

            tabControl.TabPages.Clear();

            foreach (var day in filtered)
            {
                var tab = new TabPage(day.Date.ToString("yyyy-M-d"));

                var sc = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Horizontal,
                    SplitterWidth = 4
                };
                sc.SplitterDistance = sc.ClientSize.Height / 3;

                sc.Panel1.Controls.Add(new TimelinePanel
                {
                    Dock = DockStyle.Fill,
                    Records = day.IdleRecords,
                    AppRecords = day.AppRecords,
                    FilterStart = hasTimeFilter ? timeFrom : (TimeSpan?)null,
                    FilterEnd = hasTimeFilter ? timeTo : (TimeSpan?)null
                });

                sc.Panel2.Controls.Add(BuildPieChart(day, hasTimeFilter, timeFrom, timeTo));

                tab.Controls.Add(sc);
                tabControl.TabPages.Add(tab);
            }

            if (filtered.Count == 0)
            {
                var empty = new TabPage("No data");
                var lbl = new Label
                {
                    Text = "No data matches the current filters.\nOpen CSV files using the button above.",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Gray
                };
                empty.Controls.Add(lbl);
                tabControl.TabPages.Add(empty);
            }
        }

        private Chart BuildPieChart(DayData day, bool hasTimeFilter, TimeSpan timeFrom, TimeSpan timeTo)
        {
            var chart = new Chart { Dock = DockStyle.Fill };
            var area = new ChartArea();
            area.Area3DStyle.Enable3D = false;
            chart.ChartAreas.Add(area);

            var series = new Series("Duration")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = false,
                Font = new Font("Segoe UI", 8),
                BorderWidth = 1,
                BorderColor = Color.White
            };

            var appRecords = day.AppRecords;
            if (hasTimeFilter)
            {
                appRecords = appRecords
                    .Where(r => r.StartTimestamp.TimeOfDay < timeTo && r.EndTimestamp.TimeOfDay > timeFrom)
                    .ToList();
            }

            var appGroups = appRecords
                .GroupBy(r => r.ApplicationName)
                .Select(g => new { Name = g.Key, TotalSeconds = g.Sum(r => r.DurationInSeconds) })
                .Where(x => x.TotalSeconds > 0)
                .OrderByDescending(x => x.TotalSeconds)
                .ToList();

            if (appGroups.Count == 0)
            {
                var noData = new Label
                {
                    Text = "No application data for this period",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Gray
                };
                chart.Controls.Add(noData);
                return chart;
            }

            for(int i = 0; i < appGroups.Count; i++)
            {
                var g = appGroups[i];
                var point = series.Points.AddXY(g.Name, g.TotalSeconds);
                var ts = TimeSpan.FromSeconds(g.TotalSeconds);
                series.Points[i].Label = $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
                series.Points[i].LegendText = g.Name;
            }

            chart.Series.Add(series);

            var legend = new Legend("Legend")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                LegendStyle = LegendStyle.Table
            };
            chart.Legends.Add(legend);

            return chart;
        }
    }
}
