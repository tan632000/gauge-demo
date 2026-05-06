namespace TestGauge
{
    partial class Form1
    {

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.alarmControl1 = new TestGauge.AlarmControl();
            this.eventControl1 = new TestGauge.EventControl();
            this.SuspendLayout();
            // 
            // alarmControl1
            // 
            this.alarmControl1.AlarmColor = null;
            this.alarmControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.alarmControl1.BackColorHover = null;
            this.alarmControl1.BackColorNormal = null;
            this.alarmControl1.CornerRadius = 10;
            this.alarmControl1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.alarmControl1.IconSize = 28;
            this.alarmControl1.Location = new System.Drawing.Point(40, 22);
            this.alarmControl1.Name = "alarmControl1";
            this.alarmControl1.Severity = TestGauge.AlarmSeverity.Warning;
            this.alarmControl1.Size = new System.Drawing.Size(600, 65);
            this.alarmControl1.SubtextColor = null;
            this.alarmControl1.TabIndex = 0;
            this.alarmControl1.TextColor = null;
            this.alarmControl1.Time = new System.DateTime(2026, 5, 6, 10, 37, 39, 835);
            this.alarmControl1.Title = "Title";
            this.alarmControl1.WarningColor = null;
            // 
            // eventControl1
            // 
            this.eventControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.eventControl1.BackColorHover = null;
            this.eventControl1.BackColorNormal = null;
            this.eventControl1.CloseColor = null;
            this.eventControl1.CornerRadius = 10;
            this.eventControl1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.eventControl1.IconSize = 20;
            this.eventControl1.Location = new System.Drawing.Point(40, 121);
            this.eventControl1.Name = "eventControl1";
            this.eventControl1.OpenColor = null;
            this.eventControl1.RunColor = null;
            this.eventControl1.Size = new System.Drawing.Size(600, 64);
            this.eventControl1.Status = TestGauge.EventStatus.Run;
            this.eventControl1.StopColor = null;
            this.eventControl1.SubtextColor = null;
            this.eventControl1.TabIndex = 1;
            this.eventControl1.TextColor = null;
            this.eventControl1.Time = new System.DateTime(2026, 5, 6, 10, 37, 50, 998);
            this.eventControl1.Title = "Title location";
            this.eventControl1.TripColor = null;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(854, 414);
            this.Controls.Add(this.eventControl1);
            this.Controls.Add(this.alarmControl1);
            this.Name = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private AlarmControl alarmControl1;
        private EventControl eventControl1;
    }
}
