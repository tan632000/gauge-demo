using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestGauge
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // gauge control contains its own simulation now; no per-form timer required
        }


        // Designer-like initialization moved here to keep single-file Form implementation
        private void InitializeComponent()
        {
            this.skiaGaugeCurrent = new TestGauge.SkiaGaugeControl();
            this.skiaGaugeSpeed = new TestGauge.SkiaGaugeControl();
            this.skiaGaugeTemp = new TestGauge.SkiaGaugeControl();
            this.SuspendLayout();
            // 
            // skiaGaugeCurrent
            // 
            this.skiaGaugeCurrent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.skiaGaugeCurrent.Location = new System.Drawing.Point(804, 40);
            this.skiaGaugeCurrent.Name = "skiaGaugeCurrent";
            this.skiaGaugeCurrent.Simulate = true;
            this.skiaGaugeCurrent.Size = new System.Drawing.Size(371, 300);
            this.skiaGaugeCurrent.TabIndex = 2;
            this.skiaGaugeCurrent.Title = "DÒNG ĐIỆN (A)";
            this.skiaGaugeCurrent.To = 100D;
            this.skiaGaugeCurrent.Unit = "A";
            this.skiaGaugeCurrent.Value = 45.6D;
            this.skiaGaugeCurrent.ProgressColor = System.Drawing.Color.FromArgb(255, 190, 0);
            this.skiaGaugeCurrent.SegmentColor1 = System.Drawing.Color.FromArgb(255, 200, 0);
            this.skiaGaugeCurrent.SegmentColor2 = System.Drawing.Color.FromArgb(255, 140, 0);
            this.skiaGaugeCurrent.SegmentWeight1 = 70D; // larger warm area
            this.skiaGaugeCurrent.SegmentWeight2 = 30D;
            // animate to the actual current value as the setpoint
            this.skiaGaugeCurrent.Setpoint = 45.6D;
            this.skiaGaugeCurrent.LoadPercent = 57D;
            this.skiaGaugeCurrent.NeedleColor = System.Drawing.Color.FromArgb(240,240,240);
            // 
            // skiaGaugeSpeed
            // 
            this.skiaGaugeSpeed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.skiaGaugeSpeed.Location = new System.Drawing.Point(418, 40);
            this.skiaGaugeSpeed.Name = "skiaGaugeSpeed";
            this.skiaGaugeSpeed.Simulate = true;
            this.skiaGaugeSpeed.Size = new System.Drawing.Size(349, 300);
            this.skiaGaugeSpeed.TabIndex = 1;
            this.skiaGaugeSpeed.Title = "TỐC ĐỘ TRỘN (RPM)";
            this.skiaGaugeSpeed.Value = 1200D;
            this.skiaGaugeSpeed.ProgressColor = System.Drawing.Color.FromArgb(85,170,255);
            this.skiaGaugeSpeed.ArcBackgroundColor = System.Drawing.Color.FromArgb(40,50,60);
            this.skiaGaugeSpeed.NeedleColor = System.Drawing.Color.FromArgb(200,200,200);
            this.skiaGaugeSpeed.From = 0D;
            this.skiaGaugeSpeed.To = 2000D;
            this.skiaGaugeSpeed.Setpoint = 1200D;
            // 
            // skiaGaugeTemp
            // 
            this.skiaGaugeTemp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.skiaGaugeTemp.Location = new System.Drawing.Point(30, 40);
            this.skiaGaugeTemp.Name = "skiaGaugeTemp";
            this.skiaGaugeTemp.Simulate = true;
            this.skiaGaugeTemp.Size = new System.Drawing.Size(351, 300);
            this.skiaGaugeTemp.TabIndex = 0;
            this.skiaGaugeTemp.Title = "NHIỆT ĐỘ (°C)";
            this.skiaGaugeTemp.To = 120D;
            this.skiaGaugeTemp.Unit = "°C";
            this.skiaGaugeTemp.Value = 65.2D;
            this.skiaGaugeTemp.SegmentColor1 = System.Drawing.Color.FromArgb(30,200,30);
            this.skiaGaugeTemp.SegmentColor2 = System.Drawing.Color.FromArgb(255,200,0);
            this.skiaGaugeTemp.SegmentColor3 = System.Drawing.Color.FromArgb(255,140,0);
            this.skiaGaugeTemp.SegmentColor4 = System.Drawing.Color.FromArgb(220,40,40);
            this.skiaGaugeTemp.SegmentWeight1 = 50D;
            this.skiaGaugeTemp.SegmentWeight2 = 25D;
            this.skiaGaugeTemp.SegmentWeight3 = 15D;
            this.skiaGaugeTemp.SegmentWeight4 = 10D;
            this.skiaGaugeTemp.Setpoint = 70D;
            this.skiaGaugeTemp.NeedleColor = System.Drawing.Color.FromArgb(240,240,240);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1211, 362);
            this.Controls.Add(this.skiaGaugeCurrent);
            this.Controls.Add(this.skiaGaugeSpeed);
            this.Controls.Add(this.skiaGaugeTemp);
            this.Name = "Form1";
            this.Text = "Gauge Demo";
            this.ResumeLayout(false);

        }

        private System.ComponentModel.IContainer components;
        private SkiaGaugeControl skiaGaugeTemp;
        private SkiaGaugeControl skiaGaugeSpeed;
        private SkiaGaugeControl skiaGaugeCurrent;
    }
}
