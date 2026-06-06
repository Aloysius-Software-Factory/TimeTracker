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

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private NotifyIcon _trayIcon;
        private Timer _timer;
        private string _currentExeName;
        private DateTime _currentStartTime;
        private string _logFilePath;

        private Point _lastCursorPos;
        private double _windowCursorMovement;
        private int _windowKeyPressCount;
        private DateTime _windowStartTime;
        private bool? _currentIdleState;
        private DateTime _currentIdleStateStartTime;
        private string _idleLogFilePath;


        public Tracker()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TimeTracker");
            Directory.CreateDirectory(folder);
            _logFilePath = Path.Combine(folder, DateTime.Now.ToString("yyyy-M-d") + "-app.csv");

            if (!File.Exists(_logFilePath))
                File.WriteAllText(_logFilePath, "LogTimestamp,ApplicationName,StartTimestamp,EndTimestamp,DurationInSeconds\r\n");

            _idleLogFilePath = Path.Combine(folder, DateTime.Now.ToString("yyyy-M-d") + "-idle.csv");
            if (!File.Exists(_idleLogFilePath))
                File.WriteAllText(_idleLogFilePath, "Status,StartTimestamp,EndTimestamp,DurationMinutes\r\n");

            _windowStartTime = DateTime.Now;

            POINT initialPos;
            if (GetCursorPos(out initialPos))
                _lastCursorPos = new Point(initialPos.X, initialPos.Y);

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
            Console.WriteLine(line);
            File.AppendAllText(_logFilePath, line + "\r\n");
        }

        private void WriteIdleRecord(string status, DateTime start, DateTime end)
        {
            double minutes = Math.Round((end - start).TotalMinutes, 2);
            string line = string.Format("{0},{1:yyyy-MM-dd HH:mm:ss},{2:yyyy-MM-dd HH:mm:ss},{3:F2}",
                status, start, end, minutes);
            Console.WriteLine(line);
            File.AppendAllText(_idleLogFilePath, line + "\r\n");
        }

        private int CountNewKeyPresses()
        {
            int count = 0;
            for (int i = 0; i < 256; i++)
            {
                if ((GetAsyncKeyState(i) & 0x0001) != 0)
                    count++;
            }
            return count;
        }

        private void TrackIdleState()
        {
            POINT currentPos;
            if (GetCursorPos(out currentPos))
            {
                int dx = currentPos.X - _lastCursorPos.X;
                int dy = currentPos.Y - _lastCursorPos.Y;
                _windowCursorMovement += Math.Sqrt(dx * dx + dy * dy);
                _lastCursorPos = new Point(currentPos.X, currentPos.Y);
            }

            _windowKeyPressCount += CountNewKeyPresses();

            DateTime now = DateTime.Now;
            if ((now - _windowStartTime).TotalMinutes >= Properties.Settings.Default.IdleThresholdMinutes)
            {
                bool isIdle = _windowCursorMovement < Properties.Settings.Default.CursorMoveThresholdPixels
                              && _windowKeyPressCount < Properties.Settings.Default.KeyInputThreshold;

                if (_currentIdleState.HasValue && isIdle != _currentIdleState.Value)
                {
                    WriteIdleRecord(
                        _currentIdleState.Value ? "idle" : "active",
                        _currentIdleStateStartTime,
                        now);
                    _currentIdleStateStartTime = now;
                }
                else if (!_currentIdleState.HasValue)
                {
                    _currentIdleStateStartTime = _windowStartTime;
                }

                _currentIdleState = isIdle;
                _windowStartTime = now;
                _windowCursorMovement = 0;
                _windowKeyPressCount = 0;
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
                    WriteRecord(_currentExeName, _currentStartTime, now);

                _currentExeName = exeName;
                _currentStartTime = now;
            }

            TrackIdleState();
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

            if (_currentIdleState.HasValue)
                WriteIdleRecord(
                    _currentIdleState.Value ? "idle" : "active",
                    _currentIdleStateStartTime,
                    DateTime.Now);

            _timer.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }
    }
}
