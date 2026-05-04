using System;
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
            // start timer to animate the gauge value periodically
            this.timer1.Start();
            _rnd = new Random();
        }

        private readonly Random _rnd;

        private void timer1_Tick(object sender, EventArgs e)
        {
            // animate value toward a random target to simulate RPM changes
            var target = _rnd.Next(0, 2001);
            // simple smooth change
            var current = (double)gauge1.Value;
            var newValue = current + (target - current) * 0.25;
            gauge1.Value = newValue;
            // optional: force repaint
            gauge1.Invalidate();
        }

        // Designer-like initialization moved here to keep single-file Form implementation
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gauge1 = new GaugeControl();
            this.timer1 = new System.Windows.Forms.Timer(this.components);

            // gauge1
            this.gauge1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.gauge1.Location = new Point(12, 12);
            this.gauge1.Name = "gauge1";
            this.gauge1.Size = new Size(560, 320);
            this.gauge1.TabIndex = 0;
            this.gauge1.Text = "gauge1";
            this.gauge1.From = 0;
            this.gauge1.To = 2000;
            this.gauge1.Value = 1200;

            // timer1
            this.timer1.Interval = 800;
            this.timer1.Tick += new EventHandler(this.timer1_Tick);

            // Form1
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new Size(584, 361);
            this.Controls.Add(this.gauge1);
            this.Name = "Form1";
            this.Text = "Gauge Demo";
        }

        private System.ComponentModel.IContainer components;
        private GaugeControl gauge1;
        private System.Windows.Forms.Timer timer1;
    }
}
