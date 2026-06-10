using System;
using System.Drawing;
using System.Windows.Forms;

namespace TimeTrackerDisplay
{
    partial class MainForm
    {
        private Button btnOpenFiles;
        private Label lblTimeFrom;
        private Label lblTimeTo;
        private DateTimePicker dtpTimeFrom;
        private DateTimePicker dtpTimeTo;
        private Button btnApply;
        private TabControl tabControl;
        private Panel topPanel;

        private void InitializeComponent()
        {
            btnOpenFiles = new Button();
            lblTimeFrom = new Label();
            lblTimeTo = new Label();
            dtpTimeFrom = new DateTimePicker();
            dtpTimeTo = new DateTimePicker();
            btnApply = new Button();
            tabControl = new TabControl();
            topPanel = new Panel();

            SuspendLayout();
            topPanel.SuspendLayout();

            // btnOpenFiles
            btnOpenFiles.Text = "Open CSV Files";
            btnOpenFiles.Location = new Point(12, 10);
            btnOpenFiles.Size = new Size(120, 26);
            btnOpenFiles.UseVisualStyleBackColor = true;
            btnOpenFiles.Click += BtnOpenFiles_Click;

            // lblTimeFrom
            lblTimeFrom.Text = "Time from:";
            lblTimeFrom.Location = new Point(150, 14);
            lblTimeFrom.Size = new Size(65, 18);
            lblTimeFrom.TextAlign = ContentAlignment.MiddleRight;

            // dtpTimeFrom
            dtpTimeFrom.Format = DateTimePickerFormat.Time;
            dtpTimeFrom.ShowUpDown = true;
            dtpTimeFrom.Location = new Point(220, 12);
            dtpTimeFrom.Size = new Size(80, 22);
            dtpTimeFrom.Value = DateTime.Today;

            // lblTimeTo
            lblTimeTo.Text = "to:";
            lblTimeTo.Location = new Point(305, 14);
            lblTimeTo.Size = new Size(20, 18);
            lblTimeTo.TextAlign = ContentAlignment.MiddleCenter;

            // dtpTimeTo
            dtpTimeTo.Format = DateTimePickerFormat.Time;
            dtpTimeTo.ShowUpDown = true;
            dtpTimeTo.Location = new Point(330, 12);
            dtpTimeTo.Size = new Size(80, 22);
            dtpTimeTo.Value = DateTime.Today.AddDays(1).AddSeconds(-1);

            // btnApply
            btnApply.Text = "Apply";
            btnApply.Location = new Point(430, 10);
            btnApply.Size = new Size(70, 26);
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += BtnApply_Click;

            // topPanel
            topPanel.Controls.AddRange(new Control[] {
                btnOpenFiles,
                lblTimeFrom, dtpTimeFrom,
                lblTimeTo, dtpTimeTo,
                btnApply
            });
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 48;
            topPanel.BorderStyle = BorderStyle.None;
            topPanel.BackColor = SystemColors.Control;

            // tabControl
            tabControl.Dock = DockStyle.Fill;
            tabControl.Multiline = true;

            // MainForm
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 600);
            Controls.Add(tabControl);
            Controls.Add(topPanel);
            Text = "TimeTracker Display";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(700, 400);

            topPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
