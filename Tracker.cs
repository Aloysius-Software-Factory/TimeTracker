using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TimeTracker
{
    class Tracker : ApplicationContext
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private NotifyIcon _trayIcon;
        private Timer _timer;
        private string _currentExeName;
        private DateTime _currentStartTime;
        private string _logFilePath;

        public Tracker()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TimeTracker");
            Directory.CreateDirectory(folder);
            _logFilePath = Path.Combine(folder, DateTime.Now.ToString("yyyy-M-d") + "-app.csv");

            if (!File.Exists(_logFilePath))
                File.WriteAllText(_logFilePath, "LogTimestamp,ApplicationName,StartTimestamp,EndTimestamp,DurationInSeconds\r\n");

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

        private void WriteRecord(string appName, DateTime start, DateTime end)
        {
            int seconds = (int)(end - start).TotalSeconds;
            string line = string.Format("{0:yyyy-MM-dd HH:mm:ss},{1},{2:yyyy-MM-dd HH:mm:ss},{3:yyyy-MM-dd HH:mm:ss},{4}",
                DateTime.Now, appName, start, end, seconds);
            File.AppendAllText(_logFilePath, line + "\r\n");
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
                    WriteRecord(_currentExeName, _currentStartTime, now);

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

            if (_currentExeName != null)
                WriteRecord(_currentExeName, _currentStartTime, DateTime.Now);

            _timer.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }
    }
}
