using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TimeTracker
{
    class Tracker : ApplicationContext
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private struct ActivityRecord
        {
            public string ExecutableName;
            public DateTime StartTime;
            public DateTime EndTime;
            public TimeSpan Duration;
        }

        private NotifyIcon _trayIcon;
        private Timer _timer;
        private string _currentExeName;
        private DateTime _currentStartTime;
        private List<ActivityRecord> _records;

        public Tracker()
        {
            _records = new List<ActivityRecord>();

            _trayIcon = new NotifyIcon
            {
                Icon = CreatePlaceholderIcon(),
                Text = "TimeTracker",
                Visible = true
            };

            _trayIcon.ContextMenuStrip = new ContextMenuStrip();
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, OnExit);

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private string GetActiveWindowExeName()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return null;

            uint pid;
            GetWindowThreadProcessId(hWnd, out pid);

            try
            {
                using (Process process = Process.GetProcessById((int)pid))
                {
                    return process.ProcessName + ".exe";
                }
            }
            catch
            {
                return null;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            string exeName = GetActiveWindowExeName();
            if (string.IsNullOrEmpty(exeName))
                return;

            if (exeName != _currentExeName)
            {
                DateTime now = DateTime.Now;

                if (_currentExeName != null)
                {
                    _records.Add(new ActivityRecord
                    {
                        ExecutableName = _currentExeName,
                        StartTime = _currentStartTime,
                        EndTime = now,
                        Duration = now - _currentStartTime
                    });
                }

                _currentExeName = exeName;
                _currentStartTime = now;
            }
        }

        private Icon CreatePlaceholderIcon()
        {
            using (var bmp = new Bitmap(16, 16))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.DodgerBlue);
                    using (var brush = new SolidBrush(Color.White))
                    {
                        g.DrawString("T", new Font("Segoe UI", 9, FontStyle.Bold), brush, 2, 0);
                    }
                }
                return Icon.FromHandle(bmp.GetHicon());
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }
    }
}
