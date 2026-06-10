using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TimeTrackerDisplay
{
    public class TimelinePanel : Panel
    {
        public List<IdleLogRecord> Records { get; set; } = new List<IdleLogRecord>();
        public List<AppLogRecord> AppRecords { get; set; } = new List<AppLogRecord>();
        public TimeSpan? FilterStart { get; set; }
        public TimeSpan? FilterEnd { get; set; }

        private static readonly Color ActiveColor = Color.FromArgb(76, 175, 80);
        private static readonly Color IdleColor = Color.FromArgb(255, 152, 0);
        private static readonly Color BackgroundBarColor = Color.FromArgb(230, 230, 230);
        private static readonly Color[] AppPalette = new[] {
            Color.FromArgb(33, 150, 243),
            Color.FromArgb(156, 39, 176),
            Color.FromArgb(233, 30, 99),
            Color.FromArgb(0, 188, 212),
            Color.FromArgb(255, 193, 7),
            Color.FromArgb(139, 195, 74),
            Color.FromArgb(255, 87, 34),
            Color.FromArgb(121, 85, 72),
            Color.FromArgb(96, 125, 139),
            Color.FromArgb(63, 81, 181),
            Color.FromArgb(0, 150, 136),
            Color.FromArgb(205, 220, 57),
        };
        private static readonly Dictionary<string, Color> _appColorMap = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
        private static int _nextColor;

        public TimelinePanel()
        {
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            BackColor = Color.White;
        }

        public static void ResetAppColors()
        {
            _appColorMap.Clear();
            _nextColor = 0;
        }

        private static Color GetAppColor(string name)
        {
            if (!_appColorMap.TryGetValue(name, out var color))
            {
                color = AppPalette[_nextColor % AppPalette.Length];
                _appColorMap[name] = color;
                _nextColor++;
            }
            return color;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            var rect = ClientRectangle;

            bool hasActivity = Records.Count > 0;
            bool hasApp = AppRecords.Count > 0;

            if (!hasActivity && !hasApp)
            {
                using (var font = new Font("Segoe UI", 10))
                using (var brush = new SolidBrush(Color.Gray))
                {
                    string msg = "No data available for this period";
                    var sz = g.MeasureString(msg, font);
                    g.DrawString(msg, font, brush, (rect.Width - sz.Width) / 2, (rect.Height - sz.Height) / 2);
                }
                return;
            }

            int paddingX = 50;
            int barHeight = 30;
            int gap = 6;
            int totalBars = (hasActivity ? 1 : 0) + (hasApp ? 1 : 0);
            int totalHeight = totalBars * barHeight + (totalBars - 1) * gap;
            int startY = (rect.Height - totalHeight) / 2;
            int timelineLeft = paddingX;
            int timelineWidth = rect.Width - 2 * paddingX;

            if (timelineWidth <= 50) return;

            var dayStart = FilterStart ?? TimeSpan.Zero;
            var dayEnd = FilterEnd ?? new TimeSpan(24, 0, 0);
            double totalMinutes = (dayEnd - dayStart).TotalMinutes;
            if (totalMinutes <= 0) totalMinutes = 24 * 60;

            int barY = startY;

            if (hasActivity)
            {
                DrawBar(g, "Activity", timelineLeft, barY, timelineWidth, barHeight, totalMinutes, dayStart, dayEnd);
                DrawActivitySegments(g, timelineLeft, barY, timelineWidth, barHeight, totalMinutes, dayStart, dayEnd);
                barY += barHeight + gap;
            }

            if (hasApp)
            {
                DrawBar(g, "Application", timelineLeft, barY, timelineWidth, barHeight, totalMinutes, dayStart, dayEnd);
                DrawAppSegments(g, timelineLeft, barY, timelineWidth, barHeight, totalMinutes, dayStart, dayEnd);
                barY += barHeight + gap;
            }

            int bottomBarY = barY - (hasApp ? (barHeight + gap) : 0);
            if (!hasApp) bottomBarY = startY;

            using (var font = new Font("Segoe UI", 8))
            using (var brush = new SolidBrush(Color.Black))
            using (var tickPen = new Pen(Color.FromArgb(180, 180, 180)))
            {
                int firstHour = (int)Math.Floor(dayStart.TotalHours);
                int lastHour = (int)Math.Ceiling(dayEnd.TotalHours);
                for (int hour = firstHour; hour <= lastHour; hour++)
                {
                    var time = new TimeSpan(Math.Max(0, Math.Min(24, hour)), 0, 0);
                    double offset = (time - dayStart).TotalMinutes;
                    int x = timelineLeft + (int)(offset / totalMinutes * timelineWidth);
                    if (x >= timelineLeft && x <= timelineLeft + timelineWidth)
                    {
                        g.DrawLine(tickPen, x, startY, x, startY + totalHeight);
                        string label = time.ToString(@"hh\:mm");
                        var sz = g.MeasureString(label, font);
                        g.DrawString(label, font, brush, x - sz.Width / 2, startY + totalHeight + 4);
                    }
                }
            }

            DrawLegend(g, timelineLeft, rect.Width, hasActivity, hasApp);
        }

        private void DrawBar(Graphics g, string label, int x, int y, int width, int height,
            double totalMinutes, TimeSpan dayStart, TimeSpan dayEnd)
        {
            using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var labelBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
            {
                var sz = g.MeasureString(label, font);
                g.DrawString(label, font, labelBrush, x - sz.Width - 4, y + (height - sz.Height) / 2);
            }
            g.FillRectangle(new SolidBrush(BackgroundBarColor), x, y, width, height);
            g.DrawRectangle(Pens.Gray, x, y, width, height);
        }

        private void DrawActivitySegments(Graphics g, int x, int y, int width, int height,
            double totalMinutes, TimeSpan dayStart, TimeSpan dayEnd)
        {
            foreach (var rec in Records)
            {
                var recStart = rec.StartTimestamp.TimeOfDay;
                var recEnd = rec.EndTimestamp.TimeOfDay;

                if (FilterStart.HasValue && recEnd <= FilterStart.Value) continue;
                if (FilterEnd.HasValue && recStart >= FilterEnd.Value) continue;

                var drawStart = recStart < dayStart ? dayStart : recStart;
                var drawEnd = recEnd > dayEnd ? dayEnd : recEnd;

                double startOffset = (drawStart - dayStart).TotalMinutes;
                double endOffset = (drawEnd - dayStart).TotalMinutes;

                int x1 = x + (int)(startOffset / totalMinutes * width);
                int x2 = x + (int)(endOffset / totalMinutes * width);
                int segWidth = Math.Max(1, x2 - x1);

                var color = rec.Status.Equals("active", StringComparison.OrdinalIgnoreCase) ? ActiveColor : IdleColor;
                using (var brush = new SolidBrush(color))
                    g.FillRectangle(brush, x1, y, segWidth, height);
            }
        }

        private void DrawAppSegments(Graphics g, int x, int y, int width, int height,
            double totalMinutes, TimeSpan dayStart, TimeSpan dayEnd)
        {
            foreach (var rec in AppRecords)
            {
                var recStart = rec.StartTimestamp.TimeOfDay;
                var recEnd = rec.EndTimestamp.TimeOfDay;

                if (FilterStart.HasValue && recEnd <= FilterStart.Value) continue;
                if (FilterEnd.HasValue && recStart >= FilterEnd.Value) continue;

                var drawStart = recStart < dayStart ? dayStart : recStart;
                var drawEnd = recEnd > dayEnd ? dayEnd : recEnd;

                double startOffset = (drawStart - dayStart).TotalMinutes;
                double endOffset = (drawEnd - dayStart).TotalMinutes;

                int x1 = x + (int)(startOffset / totalMinutes * width);
                int x2 = x + (int)(endOffset / totalMinutes * width);
                int segWidth = Math.Max(1, x2 - x1);

                using (var brush = new SolidBrush(GetAppColor(rec.ApplicationName)))
                    g.FillRectangle(brush, x1, y, segWidth, height);
            }
        }

        private void DrawLegend(Graphics g, int startX, int panelWidth, bool hasActivity, bool hasApp)
        {
            int legendY = 6;
            int x = startX;

            using (var font = new Font("Segoe UI", 8))
            {
                if (hasActivity)
                {
                    g.FillRectangle(new SolidBrush(ActiveColor), x, legendY, 10, 10);
                    g.DrawString("Active", font, Brushes.Black, x + 14, legendY - 2);
                    x += 65;
                    g.FillRectangle(new SolidBrush(IdleColor), x, legendY, 10, 10);
                    g.DrawString("Idle", font, Brushes.Black, x + 14, legendY - 2);
                    x += 55;
                }

                if (hasApp)
                {
                    var appNames = AppRecords
                        .Select(r => r.ApplicationName)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var name in appNames)
                    {
                        if (x + 80 > panelWidth) break;
                        var color = GetAppColor(name);
                        g.FillRectangle(new SolidBrush(color), x, legendY, 10, 10);
                        string display = name.Length > 12 ? name.Substring(0, 12) + "..." : name;
                        g.DrawString(display, font, Brushes.Black, x + 14, legendY - 2);
                        x += (int)g.MeasureString(display, font).Width + 30;
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
