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
        private MainFormDataService _service = new MainFormDataService();
        private IList<DayData> AllDays => _service.AllDays;

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
                {
                    _service.LoadFiles(dialog.FileNames);
                    this.ApplyFilter();
                }
            }
        }

        private Chart BuildStatusPieChart(DayData day, bool hasTimeFilter, TimeSpan timeFrom, TimeSpan timeTo)
        {
            var chart = new Chart { Dock = DockStyle.Fill };
            var area = new ChartArea();
            area.Area3DStyle.Enable3D = false;
            chart.ChartAreas.Add(area);

            var series = new Series("StatusDuration")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = false,
                Font = new Font("Segoe UI", 8),
                BorderWidth = 1,
                BorderColor = Color.White
            };

            var idleRecords = day.IdleRecords;
            if (hasTimeFilter)
            {
                idleRecords = idleRecords
                    .Where(r => r.StartTimestamp.TimeOfDay < timeTo && r.EndTimestamp.TimeOfDay > timeFrom)
                    .ToList();
            }

            var statusGroups = idleRecords
                .GroupBy(r => r.Status.ToLower())
                .Select(g => new { Status = g.Key, TotalSeconds = g.Sum(r => r.DurationMinutes * 60) })
                .Where(x => x.TotalSeconds > 0)
                .OrderByDescending(x => x.TotalSeconds)
                .ToList();

            if (statusGroups.Count == 0)
            {
                var noData = new Label
                {
                    Text = "No status data for this period",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Gray
                };
                chart.Controls.Add(noData);
                return chart;
            }

            for (int i = 0; i < statusGroups.Count; i++)
            {
                var g = statusGroups[i];
                var point = series.Points.AddXY(g.Status, g.TotalSeconds);
                var ts = TimeSpan.FromSeconds(g.TotalSeconds);
                series.Points[i].Label = $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
                series.Points[i].LegendText = g.Status;
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

        private void BtnApply_Click(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var timeFrom = dtpTimeFrom.Value.TimeOfDay;
            var timeTo = dtpTimeTo.Value.TimeOfDay;
            bool hasTimeFilter = timeFrom != timeTo;

            var filtered = _service.AllDays.OrderBy(d => d.Date).ToList();

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

                var chartContainer = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Vertical,
                    SplitterWidth = 4
                };
                chartContainer.Panel1.Controls.Add(BuildPieChart(day, hasTimeFilter, timeFrom, timeTo));
                chartContainer.Panel2.Controls.Add(BuildStatusPieChart(day, hasTimeFilter, timeFrom, timeTo));
                sc.Panel2.Controls.Add(chartContainer);

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

            for (int i = 0; i < appGroups.Count; i++)
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